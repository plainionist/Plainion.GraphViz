using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance.Services
{
    internal class AssemblyResolveHandler : MarshalByRefObject
    {
        //private volatile object myAssemblyRefereces;

        public void Attach()
        {
            //myAssemblyRefereces = GetType().Assembly.GetTypes();

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += OnReflectionOnlyAssemblyResolve;
        }

        // http://blogs.microsoft.co.il/sasha/2008/07/19/appdomains-and-remoting-life-time-service/
        public override object InitializeLifetimeService()
        {
            return null;
        }

        private Assembly OnAssemblyResolve( object sender, ResolveEventArgs args )
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault( asm => string.Equals( asm.FullName, args.Name, StringComparison.OrdinalIgnoreCase ) );
        }

        private Assembly OnReflectionOnlyAssemblyResolve( object sender, ResolveEventArgs args )
        {
            var loadedAssembly = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies()
                    .FirstOrDefault( asm => string.Equals( asm.FullName, args.Name, StringComparison.OrdinalIgnoreCase ) );

            if( loadedAssembly != null )
            {
                return loadedAssembly;
            }

            var assemblyName = new AssemblyName( args.Name );
            var dependentAssemblyPath = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, assemblyName.Name + ".dll" );

            if( !File.Exists( dependentAssemblyPath ) )
            {
                dependentAssemblyPath = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, assemblyName.Name + ".exe" );

                if( !File.Exists( dependentAssemblyPath ) )
                {
                    try
                    {
                        // e.g. .NET assemblies, assemblies from GAC
                        return Assembly.ReflectionOnlyLoad( args.Name );
                    }
                    catch
                    {
                        // ignore exception here - e.g. System.Windows.Interactivity - app will work without
                        Debug.WriteLine( "Failed to load: " + assemblyName );
                        return null;
                    }
                }
            }

            var assembly = Assembly.ReflectionOnlyLoadFrom( dependentAssemblyPath );
            return assembly;
        }

        public void Detach()
        {
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= OnReflectionOnlyAssemblyResolve;
            AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
        }
    }
}
