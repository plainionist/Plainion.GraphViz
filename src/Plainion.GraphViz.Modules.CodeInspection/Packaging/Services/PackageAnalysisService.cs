using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Newtonsoft.Json;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    [Export]
    class PackageAnalysisService : IDisposable
    {
        private ActorSystem mySystem;
        private int myHostPid;

        internal async Task<AnalysisDocument> Analyse(AnalysisRequest request, CancellationToken cancellationToken)
        {
            StartActorSystemOnDemand();

            var remoteAddress = Address.Parse("akka.tcp://CodeInspection@localhost:2525");

            var actor = mySystem.ActorOf(Props.Create(() => new PackageAnalysisActor())
                .WithDeploy(Deploy.None.WithScope(new RemoteScope(remoteAddress))));

            Action ShutdownAction = () =>
            {
                mySystem.Stop(actor);
                //actor.Tell(Kill.Instance);

                if (File.Exists(request.OutputFile))
                {
                    File.Delete(request.OutputFile);
                }
            };

            cancellationToken.Register(() =>
            {
                actor.Tell(new Cancel());
                ShutdownAction();
            });

            var response = await actor.Ask(request);

            try
            {
                if ((response is FailureResponse))
                {
                    throw new Exception(((FailureResponse)response).Error);
                }

                AnalysisDocument analysisResponse;

                var settings = new JsonSerializerSettings();
                settings.ContractResolver = new PrivateSettersContractResolver();

                var serializer = JsonSerializer.Create(settings);
                using (var sr = new StreamReader((string)response))
                {
                    using (var reader = new JsonTextReader(sr))
                    {
                        analysisResponse = serializer.Deserialize<AnalysisDocument>(reader);
                    }
                }

                return analysisResponse;
            }
            finally
            {
                ShutdownAction();
            }
        }

        private void StartActorSystemOnDemand()
        {
            if (mySystem != null && IsHostRunning())
            {
                return;
            }

            ShutdownActorSystem();

            var config = ConfigurationFactory.ParseString(@"
                akka {
                    actor {
                        provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                    }
                    remote {
                        helios.tcp {
                            port = 0
                            hostname = localhost
                        }
                    }
                }");

            mySystem = ActorSystem.Create("CodeInspectionClient", config);

            var executable = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "Plainion.Graphviz.ActorsHost.exe");
            var info = new ProcessStartInfo(executable);
            //info.CreateNoWindow = true;
            //info.UseShellExecute = false;
            myHostPid = Process.Start(info).Id;
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
                    host.Kill();
                    host.Dispose();
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
