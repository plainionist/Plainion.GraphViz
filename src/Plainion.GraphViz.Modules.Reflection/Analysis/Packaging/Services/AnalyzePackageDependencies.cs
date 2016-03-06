using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plainion.GraphViz.Modules.Reflection.Analysis.Packaging;
using Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Spec;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Services
{
    class AnalyzePackageDependencies : AnalyzeBase
    {
        private static string[] Colors = { "lightblue", "lightgreen", "lightgray" };

        private readonly Dictionary<string, List<Type>> myPackages = new Dictionary<string, List<Type>>();

        protected override void Load()
        {
            foreach( var package in Config.Packages )
            {
                myPackages[ package.Name ] = Load( package )
                    .SelectMany( asm => asm.GetTypes() )
                    .ToList();
            }
        }

        protected override Task<Tuple<Type, Type>[]>[] Analyze()
        {
            return Config.Packages
                .Select( p => Task.Run<Tuple<Type, Type>[]>( () => Analyze( p ) ) )
                .ToArray();
        }

        private Tuple<Type, Type>[] Analyze( Package package )
        {
            var tasks = myPackages[ package.Name ]
                .Select( t => Task.Run<IEnumerable<Tuple<Type, Type>>>( () => Analyze( package, t ) ) )
                .ToArray();

            Task.WaitAll( tasks );

            return tasks.SelectMany( t => t.Result ).ToArray();
        }

        private IEnumerable<Tuple<Type, Type>> Analyze( Package package, Type type )
        {
            Console.Write( "." );

            return new Reflector( AssemblyLoader, type ).GetUsedTypes()
                .Where( usedType => IsForeignPackage( package, usedType ) )
                .Select( usedType => GraphUtils.Edge( type, usedType ) );
        }

        private bool IsForeignPackage( Package package, Type dep )
        {
            return myPackages.Where( e => e.Key != package.Name ).Any( entry => entry.Value.Contains( dep ) );
        }

        protected override AnalysisDocument GenerateDocument( IReadOnlyCollection<Tuple<Type, Type>> edges )
        {
            var doc = new AnalysisDocument();

            for( int i = 0; i < Config.Packages.Count; ++i )
            {
                var package = Config.Packages[ i ];

                foreach( var node in myPackages[ package.Name ].Select( GraphUtils.Node ).Distinct() )
                {
                    if( !edges.Any( e => e.Item1 == node || e.Item2 == node ) )
                    {
                        continue;
                    }

                    doc.AddNode( node.FullName );
                    doc.Captions.Add( new Caption( node.FullName, node.Name ) );

                    // in case multiple cluster match we just take the first one
                    var matchedCluster = package.Clusters.FirstOrDefault( c => c.Matches( node.FullName ) );
                    if( matchedCluster != null )
                    {
                        doc.AddToCluster( matchedCluster.Name, node.FullName );
                    }
                }
            }

            foreach( var edge in edges )
            {
                doc.AddEdge( edge.Item1.FullName, edge.Item2.FullName );
            }

            return doc;
        }
    }
}
