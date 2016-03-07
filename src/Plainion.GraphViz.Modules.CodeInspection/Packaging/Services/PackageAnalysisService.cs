using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Newtonsoft.Json;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    [Export]
    class PackageAnalysisService
    {
        internal async Task<AnalysisDocument> Analyse(AnalysisRequest request, CancellationToken cancellationToken)
        {
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

            var system = ActorSystem.Create("CodeInspectionClient", config);

            var executable = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "Plainion.Graphviz.ActorsHost.exe");
            var info = new ProcessStartInfo(executable);
            //info.CreateNoWindow = true;
            //info.UseShellExecute = false;
            var actorSystemHost = Process.Start(info);

            var remoteAddress = Address.Parse("akka.tcp://CodeInspection@localhost:2525");

            var actor = system.ActorOf(Props.Create(() => new PackageAnalysisActor())
                .WithDeploy(Deploy.None.WithScope(new RemoteScope(remoteAddress))), "PackagingDependencies");

            cancellationToken.Register(() => actor.Tell(new Cancel()));

            var response = await actor.Ask(request);

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            if ((response is FailureResponse))
            {
                throw new Exception(((FailureResponse)response).Error);
            }

            try
            {
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
                system.Stop(actor);
                system.Dispose();
                actorSystemHost.Kill();

                if (File.Exists(request.OutputFile))
                {
                    File.Delete(request.OutputFile);
                }
            }
        }
    }
}
