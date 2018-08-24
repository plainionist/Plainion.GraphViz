using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Plainion.GraphViz.Infrastructure;
using Plainion.GraphViz.Modules.CodeInspection.Actors;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance.Analyzers;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance.Actors
{
    class InheritanceActor : ActorsBase
    {
        protected override void Ready()
        {
            Receive<GetInheritanceGraphMessage>(r =>
            {
                Console.WriteLine("WORKING");

                var self = Self;
                var sender = Sender;

                Task.Run<TypeRelationshipDocument>(() =>
                {
                    var assemblyHome = Path.GetDirectoryName(r.AssemblyLocation);
                    var assemblyName = AssemblyName.GetAssemblyName(r.AssemblyLocation).ToString();

                    ReportProgress();

                    var assemblies = Directory.EnumerateFiles(assemblyHome, "*.dll")
                        .Concat(Directory.EnumerateFiles(assemblyHome, "*.exe"))
                        .AsParallel()
                        .Where(file => File.Exists(file))
                        .Where(file => AssemblyUtils.IsManagedAssembly(file))
                        .ToArray();

                    ReportProgress();

                    CancellationToken.ThrowIfCancellationRequested();

                    var document = new TypeRelationshipDocument();

                    var builder = new InheritanceAnalyzer();
                    builder.IgnoreDotNetTypes = r.IgnoreDotNetTypes;

                    foreach (var assemblyFile in assemblies)
                    {
                        ProcessAssembly(assemblyName, document, builder, assemblyFile);

                        ReportProgress();

                        CancellationToken.ThrowIfCancellationRequested();
                    }

                    builder.WriteTo(r.SelectedType.Id, document);

                    return document;
                }, CancellationToken)
                .ContinueWith<object>(x =>
                {
                    if (x.IsCanceled)
                    {
                        return new CanceledMessage();
                    }

                    if (x.IsFaulted)
                    {
                        // https://github.com/akkadotnet/akka.net/issues/1409
                        // -> exceptions are currently not serializable in raw version
                        //return x.Exception;
                        return new FailedMessage { Error = x.Exception.Dump() };
                    }

                    return new InheritanceGraphMessage { Document = x.Result };
                }, TaskContinuationOptions.ExecuteSynchronously)
                .PipeTo(self, sender);

                Become(Working);
            });
        }

        private void ReportProgress()
        {
            Console.Write(".");
        }

        private void ProcessAssembly(string assemblyName, TypeRelationshipDocument document, InheritanceAnalyzer builder, string assemblyFile)
        {
            try
            {
                var assembly = Assembly.LoadFrom(assemblyFile);
                if (assembly.GetName().ToString() == assemblyName
                    || assembly.GetReferencedAssemblies().Any(r => r.ToString() == assemblyName))
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
