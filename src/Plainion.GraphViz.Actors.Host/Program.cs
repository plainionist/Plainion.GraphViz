using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;
using Plainion.Logging;

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
        LoggerFactory.AddSink(new ConsoleLoggingSink());
        LoggerFactory.LogLevel = LogLevel.Debug;

        var logger = LoggerFactory.GetLogger(typeof(Program));

        try
        {
            logger.Notice("==> Starting");

            logger.Notice("Loading modules");

            LoadModules(logger);

            logger.Notice("Creating actor system");

            using (var system = ActorSystem.Create("CodeInspection", ActorSystemConfig))
            {
                Console.WriteLine("==> Running");

                while (true) Thread.Sleep(1000);
            }
        }
        catch
        {
            logger.Error(" === DEAD === ");
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
                logger.Error($"Failed to load module '{file}' with {Environment.NewLine}{ex}");
            }
        }
    }
}
