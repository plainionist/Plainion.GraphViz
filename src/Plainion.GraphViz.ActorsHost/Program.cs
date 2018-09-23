using System;
using System.Diagnostics;
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
                        provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                    }
                    remote {
                        helios.tcp {
                            port = 2525
                            hostname = localhost
                            maximum-frame-size = 4000000b
                        }                   
                    }
                }
                ");

            try
            {
                using (var system = ActorSystem.Create("CodeInspection", config))
                {
                    Console.WriteLine("...  running ...");

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
