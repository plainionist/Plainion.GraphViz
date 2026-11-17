using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;
using Microsoft.Extensions.Logging;

namespace Plainion.GraphViz.Actors.Host;

class Program
{
    private static readonly Config ActorSystemConfig = ConfigurationFactory.ParseString(@"
            akka {
                actor {
                    provider = remote
                }
                remote {
                    dot-netty.tcp {
                        port = 2525
                        hostname = localhost
                        maximum-frame-size = 4000000b
                    }                   
                }
            }
            ");

    private static void Main(string[] args)
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        var logger = loggerFactory.CreateLogger<Program>();

        try
        {
            logger.LogInformation("==> Starting");

            logger.LogInformation("Loading modules");

            LoadModules(logger);

            logger.LogInformation("Creating actor system");

            var config = args.Length == 0 ? ActorSystemConfig
                : ConfigurationFactory.ParseString("akka.remote.dot-netty.tcp.port = 0").WithFallback(ActorSystemConfig);

            using (var system = ActorSystem.Create("CodeInspection", config))
            {
                if (args.Length > 0)
                {
                    using var pipe = new NamedPipeClientStream(".", args[0], PipeDirection.Out);
                    pipe.Connect(30000);
                    using var writer = new StreamWriter(pipe);
                    writer.WriteLine(((ExtendedActorSystem)system).Provider.DefaultAddress);
                }

                Console.WriteLine("==> Running");

                while (true) Thread.Sleep(1000);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Actor host failed");
            Environment.ExitCode = 1;
        }
    }

    // load all relevant assemblyies so that akka/newtonsoft.json can easily 
    // find types during deserialization
    private static void LoadModules(ILogger logger)
    {
        var moduleAssemblies = Directory.EnumerateFiles(Path.GetDirectoryName(typeof(Program).Assembly.Location), "*.dll")
            .Where(x => Path.GetFileNameWithoutExtension(x).StartsWith("Plainion.GraphViz.Modules.", StringComparison.OrdinalIgnoreCase))
            .Where(x => Path.GetExtension(x).Equals(".dll", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var file in moduleAssemblies)
        {
            try
            {
                // fetching types to force loading dependencies
                Assembly.LoadFrom(file).GetTypes();
            }
            catch(Exception ex)
            {
                logger.LogError($"Failed to load module '{file}' with {Environment.NewLine}{ex}");
            }
        }
    }
}
