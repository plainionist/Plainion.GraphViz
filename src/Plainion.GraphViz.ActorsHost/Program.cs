using System;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;

namespace Plainion.GraphViz.ActorsHost
{
    class Program
    {
        private static void Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(@"
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

            try
            {
                Console.WriteLine("==> Starting");

                using (var system = ActorSystem.Create("CodeInspection", config))
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
    }
}
