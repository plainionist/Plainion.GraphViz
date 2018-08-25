using System.Collections.Generic;
using Plainion.GraphViz.Modules.CodeInspection.Actors;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance.Analyzers;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance.Actors
{
    class GetAllTypesMessage
    {
        public string AssemblyLocation { get; set; }
    }

    class AllTypesMessage : FinishedMessage
    {
        public IEnumerable<TypeDescriptor> Types { get; set; }
    }

    class GetInheritanceGraphMessage
    {
        public bool IgnoreDotNetTypes { get; set; }
        public string AssemblyLocation { get; set; }
        public TypeDescriptor SelectedType { get; set; }
    }

    class InheritanceGraphMessage : FinishedMessage
    {
        public byte[] Document { get; set; }
    }
}
