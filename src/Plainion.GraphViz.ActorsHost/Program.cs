using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;

namespace Plainion.GraphViz.ActorsHost
{
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

            try
            {
                Console.WriteLine("==> Starting");

                Console.WriteLine("Loading modules");

                LoadModules();

                Console.WriteLine("Creating actor system");

                using (var system = ActorSystem.Create("CodeInspection", ActorSystemConfig))
                {
                    Console.WriteLine("==> Running");

                    while (true) Thread.Sleep(1000);
                }
            }
            catch
            {
                Console.WriteLine(" === DEAD === ");
            }
        }

        // load all relevant assemblyies so that akka/newtonsoft.json can easily 
        // find types during deserialization
        private static void LoadModules()
        {
            var moduleAssemblies = Directory.EnumerateFiles(Path.GetDirectoryName(typeof(Program).Assembly.Location), "*.dll")
                .Where(x => Path.GetFileNameWithoutExtension(x).StartsWith("Plainion.GraphViz.Modules.", StringComparison.OrdinalIgnoreCase))
                .Where(x => Path.GetExtension(x).Equals(".dll", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var file in moduleAssemblies)
            {
                try
                {
                    Assembly.LoadFrom(file);
                }
                catch
                {
                    Console.WriteLine($"Warning: failed to load module: {file}");
                }
            }
        }
    }
}
