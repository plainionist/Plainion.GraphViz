using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Plainion.GraphViz.Modules.CodeInspection.Core;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    class PackageAnalyzer
    {
        private static string[] Colors = { "LightBlue", "LightGreen", "LightCoral", "Brown", "DarkTurquoise", "MediumAquamarine", "Orange", 
                                           "LawnGreen","DarkKhaki","BurlyWood","SteelBlue","Goldenrod", "Tomato","Crimson","CadetBlue" };

        private SystemPackaging myConfig;
        private CancellationToken myCancellationToken;
        private readonly MonoLoader myAssemblyLoader;
        private Dictionary<string, List<Type>> myPackageToTypesMap;
        private List<Package> myRelevantPackages;

        public PackageAnalyzer()
        {
            myAssemblyLoader = new MonoLoader();
            PackagesToAnalyze = new List<string>();
        }

        /// <summary>
        /// If empty the dependencies between all packages will be analyzed
        /// </summary>
        public IList<string> PackagesToAnalyze { get; private set; }

        public bool UsedTypesOnly { get; set; }

        public bool AllEdges { get; set; }

        /// <summary>
        /// If no matching cluster was found for a node it will be put in a cluster for its namespace
        /// </summary>
        public bool CreateClustersForNamespaces { get; set; }

        public AnalysisDocument Execute( SystemPackaging config, CancellationToken cancellationToken )
        {
            myConfig = config;
            myCancellationToken = cancellationToken;

            myRelevantPackages = PackagesToAnalyze.Any()
                ? myConfig.Packages
                    .Where( p => PackagesToAnalyze.Any( name => p.Name.Equals( name, StringComparison.OrdinalIgnoreCase ) ) )
                    .ToList()
                : myConfig.Packages;

            myPackageToTypesMap = new Dictionary<string, List<Type>>();

            Load();

            Console.WriteLine( "Analyzing ..." );

            var edges = Analyze()
                .Distinct()
                .ToList();

            Console.WriteLine();

            if( myAssemblyLoader.SkippedAssemblies.Any() )
            {
                Console.WriteLine( "Skipped assemblies:" );
                foreach( var asm in myAssemblyLoader.SkippedAssemblies )
                {
                    Console.WriteLine( "  {0}", asm );
                }
                Console.WriteLine();
            }

            Console.WriteLine( "Building Graph ..." );

            return GenerateDocument( edges );
        }

        private void Load()
        {
            foreach( var package in myRelevantPackages )
            {
                myCancellationToken.ThrowIfCancellationRequested();

                myPackageToTypesMap[ package.Name ] = Load( package )
                    .SelectMany( asm =>
                    {
                        try
                        {
                            return asm.GetTypes()
                                .Where( t => !IsCompilerGenerated( t ) );
                        }
                        catch( ReflectionTypeLoadException ex )
                        {
                            Console.WriteLine( "WARNING: not all types could be loaded from assembly {0}. Error: {1}{2}", asm.Location,
                                Environment.NewLine, ex.Dump() );
                            return ex.Types.Where( t => t != null );
                        }
                    } )
                    .ToList();
            }
        }

        private IEnumerable<Assembly> Load( Package package )
        {
            Console.WriteLine( "Assembly root {0}", Path.GetFullPath( myConfig.AssemblyRoot ) );
            Console.WriteLine( "Loading package {0}", package.Name );

            return package.Includes
                .SelectMany( i => Directory.GetFiles( myConfig.AssemblyRoot, i.Pattern ) )
                .Where( file => !package.Excludes.Any( e => e.Matches( file ) ) )
                .Select( Load )
                .Where( asm => asm != null )
                .ToList();
        }

        private static Assembly Load(string path)
        {
            try
            {
                Console.WriteLine("Loading {0}", path);

                return Assembly.LoadFrom(path);
            }
            catch
            {
                Console.WriteLine("ERROR: failed to load assembly {0}", path);
                return null;
            }
        }

        private Reference[] Analyze()
        {
            return myRelevantPackages
                .SelectMany( p => myPackageToTypesMap[ p.Name ]
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

        private bool IsCompilerGenerated( Type type )
        {
            // do we want that?
            //if (type.GetCustomAttribute(typeof(CompilerGeneratedAttribute), true) != null)
            //{
            //    return true;
            //}

            if( type.FullName.Contains( "$", StringComparison.OrdinalIgnoreCase ) || type.FullName.Contains( "@", StringComparison.OrdinalIgnoreCase ) )
            {
                // TODO: log ignorance of these types
                // here we ignore generated closures from FSharp
                return true;
            }

            return false;
        }

        private IEnumerable<Reference> Analyze( Package package, Type type )
        {
            Console.Write( "." );

            myCancellationToken.ThrowIfCancellationRequested();

            var focusedPackageTypes = myPackageToTypesMap.Count > 1
                ? null
                : myPackageToTypesMap.Single().Value;

            return new Inspector( myAssemblyLoader, type ).GetUsedTypes()
                // if only one package is given we analyse the deps within the package - otherwise between the packages
                .Where( edge => ( AllEdges && myPackageToTypesMap.Any( e => e.Value.Contains( edge.To ) ) )
                    || ( focusedPackageTypes != null ? focusedPackageTypes.Contains( edge.To ) : IsForeignPackage( package, edge.To ) ) )
                .Where( edge => !IsCompilerGenerated( edge.To ) )
                .Select( edge => GraphUtils.Edge( edge ) )
                .Where( edge => edge.From != edge.To );
        }

        private bool IsForeignPackage( Package package, Type dep )
        {
            return myPackageToTypesMap.Where( e => e.Key != package.Name ).Any( entry => entry.Value.Contains( dep ) );
        }

        private AnalysisDocument GenerateDocument( IReadOnlyCollection<Reference> edges )
        {
            var doc = new AnalysisDocument();

            var nodesWithEdgesIndex = new HashSet<Type>();
            if( UsedTypesOnly )
            {
                foreach( var edge in edges )
                {
                    nodesWithEdgesIndex.Add( edge.From );
                    nodesWithEdgesIndex.Add( edge.To );
                }
            }

            var relevantNotesWithCluster = myPackageToTypesMap
                .Select( e => new
                {
                    Package = myRelevantPackages.Single( p => p.Name == e.Key ),
                    Types = e.Value
                } )
                .SelectMany( ( e, idx ) => e.Types
                    .Select( t => new
                    {
                        Type = t,
                        Package = e.Package,
                        PackageIndex = idx
                    } ) )
                .AsParallel()
                .Where( e => !UsedTypesOnly || nodesWithEdgesIndex.Contains( e.Type ) )
                .Select( e => new
                {
                    Node = GraphUtils.Node( e.Type ),
                    Cluster = GetCluster( e.Package, e.Type ),
                    PackageIndex = e.PackageIndex
                } );

            foreach( var entry in relevantNotesWithCluster )
            {
                doc.Add( entry.Node );

                if( entry.Cluster != null )
                {
                    doc.AddToCluster( entry.Node, entry.Cluster );
                }

                if( myPackageToTypesMap.Count > 1 )
                {
                    // color coding of nodes we only need if multiple packages were analyzed
                    doc.AddNodeColor( entry.Node, Colors[ entry.PackageIndex % Colors.Length ] );
                }
            }

            foreach( var edge in edges )
            {
                doc.Add( edge );

                var color = GetEdgeColor( edge );
                if( color != null )
                {
                    doc.AddEdgeColor( edge, color );
                }
            }

            return doc;
        }

        private Cluster GetCluster( Package package, Type type )
        {
            var cluster = package.Clusters.FirstOrDefault( c => c.Matches( type.FullName ) );
            if( cluster != null )
            {
                return cluster;
            }

            if( CreateClustersForNamespaces )
            {
                return new Cluster { Name = type.Namespace, Id = Guid.NewGuid().ToString() };
            }

            return null;
        }

        private static string GetEdgeColor( Reference edge )
        {
            if( edge.ReferenceType == ReferenceType.DerivesFrom || edge.ReferenceType == ReferenceType.Implements )
            {
                return "Blue";
            }
            else if( edge.ReferenceType != ReferenceType.Calls )
            {
                return "Gray";
            }
            return null;
        }
    }
}
