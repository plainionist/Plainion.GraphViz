using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Plainion.GraphViz.Infrastructure;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Modules.CodeInspection.Core;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance.Analyzers
{
    class InheritanceAnalyzer
    {
        private readonly RelaxedGraphBuilder myBuilder;
        private readonly IDictionary<string, TypeDescriptor> myIdToTypeMap;
        private readonly IDictionary<string, ReferenceType> myEdgeTypes;

        public InheritanceAnalyzer()
        {
            myBuilder = new RelaxedGraphBuilder();
            myIdToTypeMap = new Dictionary<string, TypeDescriptor>();
            myEdgeTypes = new Dictionary<string, ReferenceType>();
        }

        public bool IgnoreDotNetTypes { get; set; }

        public TypeRelationshipDocument Execute(string assemblyLocation, TypeDescriptor type, CancellationToken cancellationToken)
        {
            var assemblyHome = Path.GetDirectoryName(assemblyLocation);
            var assemblyName = AssemblyName.GetAssemblyName(assemblyLocation).ToString();

            Console.Write(".");

            var assemblies = Directory.EnumerateFiles(assemblyHome, "*.dll")
                .Concat(Directory.EnumerateFiles(assemblyHome, "*.exe"))
                .AsParallel()
                .Where(file => File.Exists(file))
                .Where(file => AssemblyUtils.IsManagedAssembly(file))
                .ToArray();

            Console.Write(".");

            cancellationToken.ThrowIfCancellationRequested();

            var document = new TypeRelationshipDocument();


            foreach (var assemblyFile in assemblies)
            {
                ProcessAssembly(assemblyName, document, assemblyFile);

                Console.Write(".");

                cancellationToken.ThrowIfCancellationRequested();
            }

            var visitedTypes = new HashSet<TypeDescriptor>();
            TakeSiblingsOf(document, visitedTypes, myIdToTypeMap[type.Id]);

            return document;
        }

        private void ProcessAssembly(string assemblyName, TypeRelationshipDocument document, string assemblyFile)
        {
            try
            {
                var assembly = Assembly.LoadFrom(assemblyFile);
                if (assembly.GetName().ToString() == assemblyName
                    || assembly.GetReferencedAssemblies().Any(r => r.ToString() == assemblyName))
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        ProcessType(type);
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Failed to load assembly");

                foreach (var loaderEx in ex.LoaderExceptions)
                {
                    sb.Append("  LoaderException (");
                    sb.Append(loaderEx.GetType().Name);
                    sb.Append(") ");
                    sb.AppendLine(loaderEx.Message);
                }

                document.AddFailedItem(new FailedItem(assemblyFile, sb.ToString().Trim()));
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                sb.Append("Failed to load assembly: ");
                sb.Append(ex.Message);

                document.AddFailedItem(new FailedItem(assemblyFile, sb.ToString()));
            }
        }

        private void ProcessType(Type type)
        {
            if (type.Namespace == null)
            {
                // some .net internal implementation detail ...
                return;
            }

            if (IgnoreType(type))
            {
                return;
            }

            var typeDesc = TypeDescriptor.Create(type);
            myBuilder.TryAddNode(typeDesc.Id);
            myIdToTypeMap[typeDesc.Id] = typeDesc;

            if (type.BaseType != null && !IsPrimitive(type.BaseType) && !IgnoreType(type.BaseType))
            {
                var baseDesc = TypeDescriptor.Create(type.BaseType);

                var edge = myBuilder.TryAddEdge(typeDesc.Id, baseDesc.Id);
                myIdToTypeMap[baseDesc.Id] = baseDesc;
                myEdgeTypes.Add(edge.Id, ReferenceType.DerivesFrom);
            }

            var interfaces = type.GetInterfaces();
            if (type.BaseType != null)
            {
                interfaces = interfaces.Except(type.BaseType.GetInterfaces()).ToArray();
            }
            foreach (var iface in interfaces)
            {
                if (IgnoreType(iface))
                {
                    continue;
                }

                var ifaceDesc = TypeDescriptor.Create(iface);

                var edge = myBuilder.TryAddEdge(typeDesc.Id, ifaceDesc.Id);
                if (edge != null)
                {
                    myIdToTypeMap[ifaceDesc.Id] = ifaceDesc;
                    myEdgeTypes.Add(edge.Id, ReferenceType.Implements);
                }
                else
                {
                    // edge already added - cycle?
                }
            }
        }

        private bool IgnoreType(Type type)
        {
            if (IgnoreDotNetTypes)
            {
                return type.Namespace == "System" || type.Namespace.StartsWith("System.");
            }

            return false;
        }

        private bool IsPrimitive(Type type)
        {
            return type == typeof(object) || type == typeof(ValueType) || type == typeof(Enum);
        }

        private void TakeSiblingsOf(TypeRelationshipDocument document, HashSet<TypeDescriptor> visitedTypes, params TypeDescriptor[] roots)
        {
            if (!roots.Any())
            {
                return;
            }

            var typesToFollow = new HashSet<TypeDescriptor>();

            foreach (var root in roots)
            {
                visitedTypes.Add(root);

                var node = myBuilder.Graph.FindNode(root.Id);

                foreach (var edge in node.In)
                {
                    var source = myIdToTypeMap[edge.Source.Id];
                    document.AddEdge(source, root, myEdgeTypes[edge.Id]);
                    typesToFollow.Add(source);
                }

                // TODO: down only - otherwise we need a "MaxDepth"
                //foreach( var edge in node.Out )
                //{
                //    var target = myIdToTypeMap[ edge.Target.Id ];
                //    document.AddEdge( root, target );
                //    typesToFollow.Add( target );
                //}
            }

            typesToFollow.ExceptWith(visitedTypes);

            TakeSiblingsOf(document, visitedTypes, typesToFollow.ToArray());
        }
    }
}
