using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance.Services.Framework;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance
{
    class AllTypesInspector : InspectorBase
    {
        public AllTypesInspector( string applicationBase )
            : base( applicationBase )
        {
        }

        public string AssemblyLocation
        {
            get;
            set;
        }

        public IEnumerable<TypeDescriptor> Execute()
        {
            Contract.RequiresNotNullNotEmpty( AssemblyLocation, "AssemblyLocation" );

            var assembly = Assembly.ReflectionOnlyLoadFrom( AssemblyLocation );
            return assembly.GetTypes()
                .Select( t => new TypeDescriptor( t ) )
                .ToList();
        }
    }
}
