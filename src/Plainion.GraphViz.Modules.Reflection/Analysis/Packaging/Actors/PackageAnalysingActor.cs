using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml;
using Akka.Actor;
using Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Spec;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Actors
{
    class PackageAnalysingActor : ReceiveActor, IWithUnboundedStash
    {
        private CancellationTokenSource myCTS;

        public IStash Stash { get; set; }

        public PackageAnalysingActor()
        {
            myCTS = new CancellationTokenSource();

            Ready();
        }

        private void Ready()
        {
            Receive<AnalysisRequest>( r =>
            {
                var self = Self;
                var sender = Sender;

                Task.Run<AnalysisDocument>( () =>
                {
                    var activity = r.AnalysisMode == AnalysisMode.InnerPackageDependencies ?
                        ( AnalyzeBase )new AnalyzeSubSystemDependencies { PackageName = r.PackageName } :
                        ( AnalyzeBase )new AnalyzePackageDependencies();

                    using( var reader = new StringReader( r.Spec ) )
                    {
                        var spec = ( SystemPackaging )XamlReader.Load( XmlReader.Create( reader ) );
                        return activity.Execute( spec );
                    }
                }, myCTS.Token )
                .ContinueWith<object>( x =>
                {
                    if( x.IsCanceled || x.IsFaulted )
                    {
                        return new Finished( sender ) { Exception = x.Exception };
                    }

                    return new Finished( sender ) { Response = AnalysisResponse.Create( x.Result ) };
                }, TaskContinuationOptions.ExecuteSynchronously )
                .PipeTo( self );

                Become( Working );
            } );
        }

        private void Working()
        {
            Receive<Cancel>( msg =>
            {
                myCTS.Cancel();
                BecomeReady();
            } );
            Receive<Finished>( msg =>
            {
                if( msg.Exception != null )
                {
                    // https://github.com/akkadotnet/akka.net/issues/1409
                    // -> exceptions are currently not serializable in raw version
                    msg.Sender.Tell( new FailureResponse { Error = msg.Exception.ToString() }, Self );
                }
                else
                {
                    msg.Sender.Tell( msg.Response, Self );
                }
                BecomeReady();
            } );
            ReceiveAny( o => Stash.Stash() );
        }

        private void BecomeReady()
        {
            myCTS = new CancellationTokenSource();
            Stash.UnstashAll();
            Become( Ready );
        }
    }
}
