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
            // find all assemblies at the given location
            var assemblyHome = Path.GetDirectoryName(assemblyLocation);

            Console.Write(".");

            var assemblies = Directory.EnumerateFiles(assemblyHome, "*.dll")
                .Concat(Directory.EnumerateFiles(assemblyHome, "*.exe"))
                .AsParallel()
                .Where(file => File.Exists(file))
                .Where(file => AssemblyUtils.IsManagedAssembly(file))
                .ToArray();

            Console.Write(".");

            cancellationToken.ThrowIfCancellationRequested();

            // analyse all assemblies
            var assemblyName = AssemblyName.GetAssemblyName(assemblyLocation).ToString();
            var document = new TypeRelationshipDocument();
            foreach (var assemblyFile in assemblies)
            {
                var failedItem = ProcessAssembly(assemblyName, assemblyFile);
                if (failedItem != null)
                {
                    document.AddFailedItem(failedItem);
                }

                Console.Write(".");

                cancellationToken.ThrowIfCancellationRequested();
            }

            // select the "siblings" (base + derived classes, as well as interfaces 
            // and interface implementations of the type for which this relationship 
            // analysis was requested)
            var visitedTypes = new HashSet<TypeDescriptor>();
            TakeSiblingsOf(document, visitedTypes, myIdToTypeMap[type.Id]);

            return document;
        }

        private FailedItem ProcessAssembly(string assemblyName, string assemblyFile)
        {
            try
            {
                var assembly = Assembly.LoadFrom(assemblyFile);
                // only process the assembly of the type to be analyzed and all assemblies 
                // referencing this assembly
                if (assembly.GetName().ToString() == assemblyName || assembly.GetReferencedAssemblies().Any(r => r.ToString() == assemblyName))
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        ProcessType(type);
                    }
                }

                return null;
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

                return new FailedItem(assemblyFile, sb.ToString().Trim());
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                sb.Append("Failed to load assembly: ");
                sb.Append(ex.Message);

                return new FailedItem(assemblyFile, sb.ToString());
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

            // does the type have a relevant base type?
            if (type.BaseType != null && !IsPrimitive(type.BaseType) && !IgnoreType(type.BaseType))
            {
                var baseDesc = TypeDescriptor.Create(type.BaseType);

                var edge = myBuilder.TryAddEdge(typeDesc.Id, baseDesc.Id);
                myIdToTypeMap[baseDesc.Id] = baseDesc;
                myEdgeTypes.Add(edge.Id, ReferenceType.DerivesFrom);
            }

            // add interface implementations
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

        private bool IgnoreType(Type type) =>
            IgnoreDotNetTypes ? type.Namespace == "System" || type.Namespace.StartsWith("System.") : false;

        private bool IsPrimitive(Type type) =>
            type == typeof(object) || type == typeof(ValueType) || type == typeof(Enum);

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
            }

            typesToFollow.ExceptWith(visitedTypes);

            TakeSiblingsOf(document, visitedTypes, typesToFollow.ToArray());
        }
    }
}
