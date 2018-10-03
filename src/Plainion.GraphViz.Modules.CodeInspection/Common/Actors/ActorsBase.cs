using System;
using System.Threading;
using Akka.Actor;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Actors
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
            Receive<CancelMessage>(msg =>
            {
                Console.WriteLine("CANCELING");

                myCTS.Cancel();

                Become(Cancelling);
            });

            Receive<FinishedMessage>(msg =>
            {
                Sender.Tell(msg);

                Console.WriteLine("FINISHED");

                BecomeReady();
            });

            Receive<FailedMessage>(msg =>
            {
                Sender.Tell(msg);

                Console.WriteLine("FAILED");

                BecomeReady();
            });

            ReceiveAny(o => Stash.Stash());
        }

        private void Cancelling()
        {
            Receive<CanceledMessage>(msg =>
            {
                Sender.Tell(msg);

                Console.WriteLine("CANCELED");

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
