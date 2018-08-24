using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;

namespace Plainion.GraphViz.Modules.CodeInspection.Actors
{
    class ActorClientBase : IDisposable
    {
        private ActorSystem mySystem;
        private int myHostPid;

        protected async Task<object> ProcessAsync<TRequest>(Type actorType, TRequest request, CancellationToken cancellationToken)
        {
            StartActorSystemOnDemand();

            var remoteAddress = Address.Parse("akka.tcp://CodeInspection@localhost:2525");

            var actor = mySystem.ActorOf(Props.Create(actorType)
                .WithDeploy(Deploy.None.WithScope(new RemoteScope(remoteAddress))));

            cancellationToken.Register(() => actor.Tell(new CancelMessage()));

            var response = await actor.Ask(request);

            try
            {
                if (response is FailedMessage m)
                {
                    throw new Exception(m.Error);
                }
                else if (response is CanceledMessage)
                {
                    return null;
                }
                else
                {
                    return response;
                }
            }
            finally
            {
                mySystem.Stop(actor);
                //actor.Tell(Kill.Instance);
            }
        }

        private void StartActorSystemOnDemand()
        {
            if (mySystem != null && IsHostRunning())
            {
                return;
            }

            ShutdownActorSystem();

            var executable = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "Plainion.Graphviz.ActorsHost.exe");
            var info = new ProcessStartInfo(executable);
            //info.CreateNoWindow = true;
            //info.UseShellExecute = false;
            myHostPid = Process.Start(info).Id;

            var config = ConfigurationFactory.ParseString(@"
                akka {
                    actor {
                        provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                    }
                    remote {
                        helios.tcp {
                            port = 0
                            hostname = localhost
                            maximum-frame-size = 4000000b
                        }                   
                    }
                }");

            mySystem = ActorSystem.Create("CodeInspectionClient", config);
        }

        private bool IsHostRunning()
        {
            return TryGetHostProcess() != null;
        }

        private Process TryGetHostProcess()
        {
            if (myHostPid == -1)
            {
                return null;
            }

            return Process.GetProcessesByName("Plainion.GraphViz.ActorsHost").SingleOrDefault(p => p.Id == myHostPid);
        }

        private void ShutdownActorSystem()
        {
            if (mySystem != null)
            {
                try
                {
                    mySystem.Dispose();
                }
                catch
                {
                }
                mySystem = null;
            }

            if (myHostPid != -1)
            {
                try
                {
                    var host = TryGetHostProcess();
                    if (host != null)
                    {
                        host.Kill();
                        host.Dispose();
                    }
                }
                catch
                {
                }
                myHostPid = -1;
            }
        }

        public void Dispose()
        {
            ShutdownActorSystem();
        }
    }
}
