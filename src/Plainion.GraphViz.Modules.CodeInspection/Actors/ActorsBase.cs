using System;
using System.Threading;
using Akka.Actor;

namespace Plainion.GraphViz.Modules.CodeInspection.Actors
{
    abstract class ActorsBase : ReceiveActor, IWithUnboundedStash
    {
        private CancellationTokenSource myCTS;

        public IStash Stash { get; set; }

        public ActorsBase()
        {
            myCTS = new CancellationTokenSource();

            Ready();
        }

        protected CancellationToken CancellationToken
        {
            get { return myCTS.Token; }
        }

        protected abstract void Ready();

        protected void Working()
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
                    Sender.Tell(new FailureResponse { Error = msg.Error });
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
