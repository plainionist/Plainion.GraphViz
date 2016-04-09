using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    class PackageAnalyzer 
    {
        private static string[] Colors = { "LightBlue", "LightGreen", "LightGray" };

        private SystemPackaging myConfig;
        private CancellationToken myCancellationToken;
        private AssemblyLoader myAssemblyLoader;
        private readonly Dictionary<string, List<Type>> myPackages;

        public PackageAnalyzer()
        {
            myAssemblyLoader = new AssemblyLoader();
            myPackages = new Dictionary<string, List<Type>>();
        }

        public string PackageName { get; set; }
        
        public AnalysisDocument Execute( SystemPackaging config, CancellationToken cancellationToken )
        {
            myConfig = config;
            myCancellationToken = cancellationToken;

            Load();

            Console.WriteLine("Analyzing ...");

            var edges = Analyze()
                .Distinct()
                .ToList();

            Console.WriteLine();

            if (myAssemblyLoader.SkippedAssemblies.Any())
            {
                Console.WriteLine("Skipped assemblies:");
                foreach (var asm in myAssemblyLoader.SkippedAssemblies)
                {
                    Console.WriteLine("  {0}", asm);
                }
                Console.WriteLine();
            }

            Console.WriteLine("Building Graph ...");

            return GenerateDocument(edges);
        }

        private void Load()
        {
            var relevantPackages = myConfig.Packages
                .Where( p => string.IsNullOrEmpty( PackageName ) || p.Name.Equals( PackageName, StringComparison.OrdinalIgnoreCase ) );

            foreach( var package in relevantPackages )
            {
                myCancellationToken.ThrowIfCancellationRequested();

                myPackages[ package.Name ] = Load( package )
                    .SelectMany( asm => asm.GetTypes() )
                    .ToList();
            }
        }

        private IEnumerable<Assembly> Load( Package package )
        {
            Console.WriteLine("Assembly root {0}", Path.GetFullPath(myConfig.AssemblyRoot));
            Console.WriteLine("Loading package {0}", package.Name);

            return package.Includes
                .SelectMany(i => Directory.GetFiles(myConfig.AssemblyRoot, i.Pattern))
                .Where(file => !package.Excludes.Any(e => e.Matches(file)))
                .Select(myAssemblyLoader.Load)
                .Where(asm => asm != null)
                .ToList();
        }

        private Edge[] Analyze()
        {
            var relevantPackages = myConfig.Packages
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
                .WithCancellation( myCancellationToken )
                .SelectMany( e => Analyze( e.Package, e.Type ) )
                .ToArray();
        }

        private IEnumerable<Edge> Analyze( Package package, Type type )
        {
            Console.Write( "." );

            myCancellationToken.ThrowIfCancellationRequested();

            var focusedPackageTypes = string.IsNullOrEmpty( PackageName )
                ? null
                : myPackages.Single( p => p.Key.Equals( PackageName, StringComparison.OrdinalIgnoreCase ) ).Value;

            return new Reflector( myAssemblyLoader, type ).GetUsedTypes()
                // if only one package is given we analyse the deps within the package - otherwise between the packages
                .Where( edge => focusedPackageTypes != null ? focusedPackageTypes.Contains( edge.Target ) : IsForeignPackage( package, edge.Target ) )
                .Select( edge => GraphUtils.Edge( edge ) )
                .Where( edge => edge.Source != edge.Target );
        }

        private bool IsForeignPackage( Package package, Type dep )
        {
            return myPackages.Where( e => e.Key != package.Name ).Any( entry => entry.Value.Contains( dep ) );
        }

        private AnalysisDocument GenerateDocument( IReadOnlyCollection<Edge> edges )
        {
            var doc = new AnalysisDocument();

            var nodesWithEdgesIndex = new HashSet<Type>();
            foreach( var edge in edges )
            {
                nodesWithEdgesIndex.Add( edge.Source );
                nodesWithEdgesIndex.Add( edge.Target );
            }

            for( int i = 0; i < myConfig.Packages.Count; ++i )
            {
                var package = myConfig.Packages[ i ];

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
