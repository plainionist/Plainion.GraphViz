using System;
using System.Threading.Tasks;
using Akka.Actor;
using Plainion.GraphViz.Modules.CodeInspection.Common.Actors;
using Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers;
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
                    using (var resolver = new AssemblyResolver())
                    {
                        var analyzer = new InheritanceAnalyzer();
                        analyzer.IgnoreDotNetTypes = r.IgnoreDotNetTypes;
                        return analyzer.Execute(r.AssemblyLocation, r.TypeToAnalyze, CancellationToken);
                    }
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

                    var serializer = new DocumentSerializer();
                    var blob = serializer.Serialize(x.Result);

                    return new InheritanceGraphMessage { Document = blob };
                }, TaskContinuationOptions.ExecuteSynchronously)
                .PipeTo(self, sender);

                Become(Working);
            });
        }
    }
}
