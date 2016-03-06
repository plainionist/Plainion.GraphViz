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
            Receive<GraphBuildRequest>( r =>
            {
                var self = Self;
                var sender = Sender;

                Task.Run<AnalysisDocument>( () =>
                {
                    var activity = new AnalyzePackageDependencies();

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

                    return new Finished( sender ) { Document = x.Result };
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
                    msg.Sender.Tell( new Failure { Exception = msg.Exception }, Self );
                }
                else
                {
                    msg.Sender.Tell( msg.Document, Self );
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
