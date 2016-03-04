using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml;
using Akka.Actor;
using Akka.Dispatch.SysMsg;

using Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Spec;

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
            Receive<GraphBuildRequest>(r =>
            {
                var self = Self;
                var sender = Sender;

                Task.Run<string>(() =>
                {
                    var activity = new AnalyzePackageDependencies();
                    activity.OutputFile = r.OutputFile;

                    using (var reader = new StringReader(r.Spec))
                    {
                        var spec = (SystemPackaging)XamlReader.Load(XmlReader.Create(reader));
                        activity.Execute(spec);
                    }

                    return activity.OutputFile;
                }, myCTS.Token)
                .ContinueWith<object>(x =>
                {
                    if (x.IsCanceled || x.IsFaulted)
                    {
                        return new Finished(sender) { Exception = x.Exception };
                    }

                    return new Finished(sender) {OutputFile = x.Result};
                }, TaskContinuationOptions.ExecuteSynchronously)
                .PipeTo(self);

                Become(Working);
            });
        }

        private void Working()
        {
            Receive<Cancel>(msg =>
            {
                myCTS.Cancel();
                BecomeReady();
            });
            Receive<Finished>(msg =>
            {
                msg.Sender.Tell(msg.OutputFile, Self);
                BecomeReady();
            });
            ReceiveAny(o => Stash.Stash());
        }

        private void BecomeReady()
        {
            myCTS = new CancellationTokenSource();
            Stash.UnstashAll();
            Become(Ready);
        }
    }
}
