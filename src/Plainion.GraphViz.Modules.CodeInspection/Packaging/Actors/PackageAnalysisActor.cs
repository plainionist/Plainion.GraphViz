using System;
using System.Threading.Tasks;
using Akka.Actor;
using Plainion.Collections;
using Plainion.GraphViz.Modules.CodeInspection.Common.Actors;
using Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Analyzers;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Actors
{
    class PackageAnalysisActor : ActorsBase
    {
        protected override void Ready()
        {
            Receive<AnalysisMessage>(r =>
                {
                    Console.WriteLine("WORKING");

                    var self = Self;
                    var sender = Sender;

                    Task.Run<AnalysisDocument>(() =>
                    {
                        var loader = new TypesLoader(AssemblyLoaderFactory.Create());
                        var analyzer = new PackageAnalyzer(loader);

                        analyzer.UsedTypesOnly = r.UsedTypesOnly;
                        analyzer.CreateClustersForNamespaces = r.CreateClustersForNamespaces;

                        if (r.PackagesToAnalyze != null)
                        {
                            analyzer.PackagesToAnalyze.AddRange(r.PackagesToAnalyze);
                        }

                        var spec = SpecUtils.Deserialize(SpecUtils.Unzip(r.Spec));
                        return analyzer.Execute(spec, CancellationToken);
                    }, CancellationToken)
                    .ContinueWith<object>(x =>
                    {
                        if (x.IsCanceled)
                        {
                            return new CanceledMessage();
                        }

                        if (x.IsFaulted)
                        {
                            return new FailedMessage { Error = x.Exception.Dump() };
                        }

                        Console.WriteLine("Writing response ...");

                        var serializer = new DocumentSerializer();
                        serializer.Serialize(x.Result, r.OutputFile);

                        return new AnalysisResponse { File = r.OutputFile };
                    }, TaskContinuationOptions.ExecuteSynchronously)
                    .PipeTo(self, sender);

                    Become(Working);
                });
        }
    }
}
