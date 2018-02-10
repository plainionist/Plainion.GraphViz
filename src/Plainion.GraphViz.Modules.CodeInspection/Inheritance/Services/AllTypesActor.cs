using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance.Services
{
    class AllTypesActor : MarshalByRefObject
    {
        public string AssemblyLocation { get; set; }

        public IEnumerable<TypeDescriptor> Execute()
        {
            Contract.RequiresNotNullNotEmpty(AssemblyLocation, "AssemblyLocation");

            var assembly = Assembly.ReflectionOnlyLoadFrom(AssemblyLocation);
            return assembly.GetTypes()
                .Select(t => new TypeDescriptor(t))
                .ToList();
        }
    }
}
