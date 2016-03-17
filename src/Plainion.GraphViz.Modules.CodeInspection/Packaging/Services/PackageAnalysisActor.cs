using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml;
using Akka.Actor;
using Newtonsoft.Json;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    class PackageAnalysisActor : ReceiveActor, IWithUnboundedStash
    {
        private CancellationTokenSource myCTS;

        public IStash Stash { get; set; }

        public PackageAnalysisActor()
        {
            myCTS = new CancellationTokenSource();

            Ready();
        }

        private void Ready()
        {
            Receive<AnalysisRequest>(r =>
            {
                var self = Self;
                var sender = Sender;

                Task.Run<AnalysisDocument>(() =>
                {
                    var activity = r.AnalysisMode == AnalysisMode.InnerPackageDependencies ?
                        (AnalyzeBase)new AnalyzeInnerPackageDependencies { PackageName = r.PackageName } :
                        (AnalyzeBase)new AnalyzeCrossPackageDependencies();

                    using (var reader = new StringReader(r.Spec))
                    {
                        var spec = (SystemPackaging)XamlReader.Load(XmlReader.Create(reader));
                        return activity.Execute(spec, myCTS.Token);
                    }
                }, myCTS.Token)
                .ContinueWith<object>(x =>
                {
                    if (x.IsCanceled || x.IsFaulted)
                    {
                        // https://github.com/akkadotnet/akka.net/issues/1409
                        // -> exceptions are currently not serializable in raw version
                        //return x.Exception;
                        return new Finished { Error = x.Exception.ToString() };
                    }

                    var serializer = new JsonSerializer();
                    using (var sw = new StreamWriter(r.OutputFile))
                    {
                        using (var writer = new JsonTextWriter(sw))
                        {
                            serializer.Serialize(writer, x.Result);
                        }
                    }

                    return new Finished { ResponseFile = r.OutputFile };
                }, TaskContinuationOptions.ExecuteSynchronously)
                .PipeTo(self, sender);

                Become(Working);
            });
        }

        private void Working()
        {
            Receive<Cancel>(msg =>
            {
                Console.WriteLine("CANCELED");

                myCTS.Cancel();

                Sender.Tell("canceled");

                BecomeReady();
            });
            Receive<Finished>(msg =>
            {
                if (msg.Error != null)
                {
                    // https://github.com/akkadotnet/akka.net/issues/1409
                    // -> exceptions are currently not serializable in raw version
                    Sender.Tell(new FailureResponse {Error = msg.Error});
                }
                else
                {
                    Sender.Tell(msg.ResponseFile);
                }

                Console.WriteLine("FINISHED");
                
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
