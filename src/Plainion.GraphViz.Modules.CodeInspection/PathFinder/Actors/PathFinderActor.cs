using System;
using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using Plainion.GraphViz.Modules.CodeInspection.Actors;
using Plainion.GraphViz.Modules.CodeInspection.Batch;

namespace Plainion.GraphViz.Modules.CodeInspection.PathFinder.Actors
{
    class PathFinderActor : ActorsBase
    {
        protected override void Ready()
        {
            Receive<PathFinderRequest>(r =>
            {
                Console.WriteLine("WORKING");

                var self = Self;
                var sender = Sender;

                Task.Run<string>(() =>
                {
                    var outputFile = Path.GetTempFileName() + ".dot";
                    var analyzer = new PathFinderAnalyzer();
                    analyzer.Execute(r.ConfigFile, r.AssemblyReferencesOnly, outputFile);
                    return outputFile;
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

                    return new PathFinderResponse { DotFile = x.Result };
                }, TaskContinuationOptions.ExecuteSynchronously)
                .PipeTo(self, sender);

                Become(Working);
            });
        }
    }
}
