using System;
using System.Threading;
using Akka.Actor;
using Akka.Configuration;

namespace Plainion.GraphViz.Pioneer
{
    class Program
    {
        private static void Main( string[] args )
        {
            if( args.Length == 1 && args[ 0 ] == "-SAS" )
            {
                StartActorSystem();
            }
            else
            {
                throw new NotImplementedException( "No commands implemented" );
            }
        }

        private static void StartActorSystem()
        {
            var config = ConfigurationFactory.ParseString( @"
                akka {
                    actor {
                        provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                    }

                    remote {
                        helios.tcp {
                            port = 2525
                            hostname = localhost
                        }
                    }
                }
                " );

            using( var system = ActorSystem.Create( "CodeInspection", config ) )
            {
                Console.WriteLine( "...  running ..." );

                while( true ) Thread.Sleep( 1000 );
            }
        }
    }
}
