using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Akka.Actor;
using Plainion.GraphViz.Actors.Client;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance.Analyzers;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance.Actors
{
    class AllTypesActor : ActorsBase
    {
        protected override void Ready()
        {
            Receive<GetAllTypesMessage>(r =>
            {
                Console.WriteLine("WORKING");

                var self = Self;
                var sender = Sender;

                Task.Run<IEnumerable<TypeDescriptor>>(() =>
                {
                    var assembly = Assembly.LoadFrom(r.AssemblyLocation);
                    return assembly.GetTypes()
                        .Select(t => TypeDescriptor.Create(t))
                        .ToList();
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

                    return new AllTypesMessage { Types = x.Result };
                }, TaskContinuationOptions.ExecuteSynchronously)
                .PipeTo(self, sender);

                Become(Working);
            });
        }
    }
}
