using System;
using System.IO;
using System.Threading;
using System.Windows.Markup;
using System.Xml;
using Akka.Actor;
using Akka.Configuration;
using Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Actors;
using Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Spec;

namespace Plainion.GraphViz.Pioneer
{
    class Program
    {
        private static string myPackageName;
        private static string myConfigFile;
        private static string myOutputFile;

        private static void Main( string[] args )
        {
            if( args.Length == 1 && args[ 0 ] == "-SAS" )
            {
                StartActorSystem();
            }
            else
            {
                ConsoleMain( args );
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

        private static void ConsoleMain( string[] args )
        {
            try
            {
                myOutputFile = Path.GetFullPath( "." );

                for( int i = 0; i < args.Length; ++i )
                {
                    if( args[ i ] == "-p" )
                    {
                        myPackageName = args[ i + 1 ];
                    }
                    else if( args[ i ] == "-o" )
                    {
                        myOutputFile = args[ i + 1 ];
                    }
                    else
                    {
                        myConfigFile = args[ i ];
                    }
                }

                if( myConfigFile == null )
                {
                    Console.WriteLine( "Config file missing" );
                    Environment.Exit( 1 );
                }

                if( !File.Exists( myConfigFile ) )
                {
                    Console.WriteLine( "Config file does not exist: " + myConfigFile );
                    Environment.Exit( 1 );
                }

                var config = ( SystemPackaging )XamlReader.Load( XmlReader.Create( myConfigFile ) );

                var analyzer = CreateAnalyzer( config );
                analyzer.OutputFile = myOutputFile;
                analyzer.Execute( config );
            }
            catch( Exception ex )
            {
                Console.WriteLine( ex.ToString() );
                Environment.Exit( 1 );
            }
        }

        private static AnalyzeBase CreateAnalyzer( SystemPackaging config )
        {
            if( myPackageName == null )
            {
                return new AnalyzePackageDependencies();
            }
            else
            {
                return new AnalyzeSubSystemDependencies()
                {
                    PackageName = myPackageName
                };
            }
        }
    }
}
