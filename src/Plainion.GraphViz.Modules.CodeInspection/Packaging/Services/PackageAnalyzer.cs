using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    class PackageAnalyzer : AnalyzeBase
    {
        private static string[] Colors = { "LightBlue", "LightGreen", "LightGray" };

        private readonly Dictionary<string, List<Type>> myPackages = new Dictionary<string, List<Type>>();

        public string PackageName { get; set; }

        protected override void Load()
        {
            var relevantPackages = Config.Packages
                .Where( p => string.IsNullOrEmpty( PackageName ) || p.Name.Equals( PackageName, StringComparison.OrdinalIgnoreCase ) );

            foreach( var package in relevantPackages )
            {
                CancellationToken.ThrowIfCancellationRequested();

                myPackages[ package.Name ] = Load( package )
                    .SelectMany( asm => asm.GetTypes() )
                    .ToList();
            }
        }

        protected override Edge[] Analyze()
        {
            var relevantPackages = Config.Packages
                .Where( p => string.IsNullOrEmpty( PackageName ) || p.Name.Equals( PackageName, StringComparison.OrdinalIgnoreCase ) );

            return relevantPackages
                .SelectMany( p => myPackages[ p.Name ]
                    .Select( t => new
                    {
                        Package = p,
                        Type = t
                    } )
                )
                .AsParallel()
                .WithCancellation( CancellationToken )
                .SelectMany( e => Analyze( e.Package, e.Type ) )
                .ToArray();
        }

        private IEnumerable<Edge> Analyze( Package package, Type type )
        {
            Console.Write( "." );

            CancellationToken.ThrowIfCancellationRequested();

            var focusedPackageTypes = string.IsNullOrEmpty( PackageName )
                ? null
                : myPackages.Single( p => p.Key.Equals( PackageName, StringComparison.OrdinalIgnoreCase ) ).Value;

            return new Reflector( AssemblyLoader, type ).GetUsedTypes()
                // if only one package is given we analyse the deps within the package - otherwise between the packages
                .Where( edge => focusedPackageTypes != null ? focusedPackageTypes.Contains( edge.Target ) : IsForeignPackage( package, edge.Target ) )
                .Select( edge => GraphUtils.Edge( edge ) )
                .Where( edge => edge.Source != edge.Target );
        }

        private bool IsForeignPackage( Package package, Type dep )
        {
            return myPackages.Where( e => e.Key != package.Name ).Any( entry => entry.Value.Contains( dep ) );
        }

        protected override AnalysisDocument GenerateDocument( IReadOnlyCollection<Edge> edges )
        {
            var doc = new AnalysisDocument();

            var nodesWithEdgesIndex = new HashSet<Type>();
            foreach( var edge in edges )
            {
                nodesWithEdgesIndex.Add( edge.Source );
                nodesWithEdgesIndex.Add( edge.Target );
            }

            for( int i = 0; i < Config.Packages.Count; ++i )
            {
                var package = Config.Packages[ i ];

                if( !myPackages.ContainsKey( package.Name ) )
                {
                    // package not analyzed
                    continue;
                }

                foreach( var node in myPackages[ package.Name ].Select( GraphUtils.Node ).Distinct() )
                {
                    if( !nodesWithEdgesIndex.Contains( node ) )
                    {
                        continue;
                    }

                    // color coding of nodes we only need if multiple packages were analyzed
                    doc.Add( node, package, myPackages.Count == 1 ? null : Colors[ i % Colors.Length ] );
                }
            }

            doc.Add( edges );

            return doc;
        }
    }
}
