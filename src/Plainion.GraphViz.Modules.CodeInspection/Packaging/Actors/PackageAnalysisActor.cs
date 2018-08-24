using System;
using System.Threading.Tasks;
using Akka.Actor;
using Plainion.Collections;
using Plainion.GraphViz.Modules.CodeInspection.Actors;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Analyzers;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Actors
{
    class PackageAnalysisActor : ActorsBase
    {
        protected override void Ready()
        {
            Receive<AnalysisMessage>( r =>
            {
                Console.WriteLine( "WORKING" );

                //System.Diagnostics.Debugger.Launch();

                var self = Self;
                var sender = Sender;

                Task.Run<AnalysisDocument>( () =>
                {
                    var analyzer = new PackageAnalyzer();
                    analyzer.UsedTypesOnly = r.UsedTypesOnly;
                    analyzer.AllEdges = r.AllEdges;
                    analyzer.CreateClustersForNamespaces = r.CreateClustersForNamespaces;

                    if( r.PackagesToAnalyze != null )
                    {
                        analyzer.PackagesToAnalyze.AddRange( r.PackagesToAnalyze );
                    }

                    var spec = SpecUtils.Deserialize( SpecUtils.Unzip( r.Spec ) );
                    return analyzer.Execute( spec, CancellationToken);
                }, CancellationToken)
                .ContinueWith<object>( x =>
                {
                    if( x.IsCanceled || x.IsFaulted )
                    {
                        // https://github.com/akkadotnet/akka.net/issues/1409
                        // -> exceptions are currently not serializable in raw version
                        //return x.Exception;
                        return new Finished { Error = x.Exception.Dump() };
                    }

                    Console.WriteLine( "Writing response ..." );

                    var serializer = new AnalysisDocumentSerializer();
                    serializer.Serialize( x.Result, r.OutputFile );

                    return new Finished { ResponseFile = r.OutputFile };
                }, TaskContinuationOptions.ExecuteSynchronously )
                .PipeTo( self, sender );

                Become( Working );
            } );
        }
    }
}
