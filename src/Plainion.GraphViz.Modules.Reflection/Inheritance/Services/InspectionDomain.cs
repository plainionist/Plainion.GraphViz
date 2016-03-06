using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Policy;
using Plainion.GraphViz.Modules.Reflection.Services.Framework;
using Plainion;

namespace Plainion.GraphViz.Modules.Reflection.Services
{
    internal class InspectionDomain : IDisposable
    {
        private AppDomain myDomain;
        private AssemblyResolveHandler myAssemblyResolveHandler;

        public InspectionDomain( string appBase )
        {
            Contract.RequiresNotNullNotEmpty( appBase, "appBase" );

            ApplicationBase = appBase;

            var evidence = new Evidence( AppDomain.CurrentDomain.Evidence );
            var setup = AppDomain.CurrentDomain.SetupInformation;
            setup.ApplicationBase = ApplicationBase;
            myDomain = AppDomain.CreateDomain( "GraphViz.Modules.Reflection.Sandbox-" + appBase.GetHashCode(), evidence, setup );

            myAssemblyResolveHandler = CreateInstance<AssemblyResolveHandler>();
            myAssemblyResolveHandler.Attach();
        }

        private T CreateInstance<T>()
        {
            return ( T )myDomain.CreateInstanceFrom( typeof( T ).Assembly.Location, typeof( T ).FullName ).Unwrap();
        }

        public string ApplicationBase
        {
            get;
            private set;
        }

        public T CreateInspector<T>() where T : InspectorBase
        {
            return ( T )myDomain.CreateInstanceFrom( typeof( T ).Assembly.Location, typeof( T ).FullName, false,
                BindingFlags.Default, null, new[] { ApplicationBase }, null, null ).Unwrap();
        }

        public void Dispose()
        {
            if( myDomain != null )
            {
                Debug.WriteLine( "Unloading AppDomain");

                myAssemblyResolveHandler.Detach();
                AppDomain.Unload( myDomain );
                myDomain = null;

                GC.Collect();
            }
        }
    }
}
