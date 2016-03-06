using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Plainion.GraphViz.Infrastructure;
using Plainion.GraphViz.Modules.Reflection.Services.Framework;

namespace Plainion.GraphViz.Modules.Reflection.Analysis.Inheritance
{
    class InheritanceGraphInspector : AsyncInspectorBase<TypeRelationshipDocument>
    {
        private string mySelectedAssemblyName;

        public InheritanceGraphInspector(string applicationBase)
            : base(applicationBase)
        {
        }

        public bool IgnoreDotNetTypes { get; set; }

        public string AssemblyLocation { get; set; }

        public TypeDescriptor SelectedType { get; set; }

        public override TypeRelationshipDocument Execute()
        {
            Contract.RequiresNotNullNotEmpty(AssemblyLocation, "AssemblyLocation");
            Contract.RequiresNotNull(SelectedType, "SelectedType");

            var assemblyHome = Path.GetDirectoryName(AssemblyLocation);
            mySelectedAssemblyName = AssemblyName.GetAssemblyName(AssemblyLocation).ToString();

            ReportProgress(1);

            var assemblies = Directory.EnumerateFiles(assemblyHome, "*.dll")
                .Concat(Directory.EnumerateFiles(assemblyHome, "*.exe"))
                .AsParallel()
                .Where(file => File.Exists(file))
                .Where(file => AssemblyUtils.IsManagedAssembly(file))
                .ToArray();

            double progressCounter = assemblies.Length;

            ReportProgress(( int )( ( assemblies.Length - progressCounter ) / assemblies.Length * 100 ));

            if (IsCancellationRequested)
            {
                return null;
            }

            var document = new TypeRelationshipDocument();

            var builder = new InheritanceGraphBuilder();
            builder.IgnoreDotNetTypes = IgnoreDotNetTypes;

            foreach (var assemblyFile in assemblies)
            {
                ProcessAssembly(document, builder, assemblyFile);

                progressCounter--;

                ReportProgress(( int )( ( assemblies.Length - progressCounter ) / assemblies.Length * 100 ));

                if (IsCancellationRequested)
                {
                    return null;
                }
            }

            builder.WriteTo(SelectedType.Id, document);

            return document;
        }

        private void ProcessAssembly(TypeRelationshipDocument document, InheritanceGraphBuilder builder, string assemblyFile)
        {
            try
            {
                var assembly = Assembly.ReflectionOnlyLoadFrom(assemblyFile);
                if (assembly.GetName().ToString() == mySelectedAssemblyName
                    || assembly.GetReferencedAssemblies().Any(r => r.ToString() == mySelectedAssemblyName))
                {
                    builder.Process(assembly);
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

                document.FailedItems.Add(new FailedItem(assemblyFile, sb.ToString().Trim()));
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                sb.Append("Failed to load assembly: ");
                sb.Append(ex.Message);

                document.FailedItems.Add(new FailedItem(assemblyFile, sb.ToString()));
            }
        }
    }
}
