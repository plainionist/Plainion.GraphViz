using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    class AnalyzeInnerPackageDependencies : AnalyzeBase
    {
        private Package myPackage;
        private List<Type> myTypes;

        public string PackageName { get; set; }

        protected override void Load()
        {
            myPackage = Config.Packages.Single( p => p.Name.Equals( PackageName, StringComparison.OrdinalIgnoreCase ) );

            myTypes = Load( myPackage )
                .SelectMany( asm => asm.GetTypes() )
                .ToList();
        }

        protected override Edge[] Analyze()
        {
            return myTypes
                .AsParallel()
                .WithCancellation( CancellationToken )
                .SelectMany( t => Analyze( t ) )
                .ToArray();
        }

        private IEnumerable<Edge> Analyze( Type type )
        {
            Console.Write( "." );

            CancellationToken.ThrowIfCancellationRequested();

            var cluster = myPackage.Clusters.FirstOrDefault( c => c.Matches( type.FullName ) );

            return new Reflector( AssemblyLoader, type ).GetUsedTypes()
                .Where( myTypes.Contains )
                .Where( t => type != t )
                .Where( t => cluster == null || cluster != myPackage.Clusters.FirstOrDefault( c => c.Matches( t.FullName ) ) )
                .Select( usedType => GraphUtils.Edge( type, usedType ) )
                .Where( edge => edge.Source != edge.Target );
        }

        protected override AnalysisDocument GenerateDocument( IReadOnlyCollection<Edge> edges )
        {
            var doc = new AnalysisDocument();

            foreach( var node in myTypes.Select( GraphUtils.Node ).Distinct() )
            {
                if( !edges.Any( e => e.Source == node || e.Target == node ) )
                {
                    continue;
                }

                doc.AddNode( node.FullName );
                doc.Add( new Caption( node.FullName, node.Name ) );

                // in case multiple cluster match we just take the first one
                var matchedCluster = myPackage.Clusters.FirstOrDefault( c => c.Matches( node.FullName ) );
                if( matchedCluster != null )
                {
                    doc.AddToCluster( matchedCluster.Name, node.FullName );
                }
            }

            foreach( var edge in edges )
            {
                doc.AddEdge( edge.Source.FullName, edge.Target.FullName );
            }

            return doc;
        }
    }
}
