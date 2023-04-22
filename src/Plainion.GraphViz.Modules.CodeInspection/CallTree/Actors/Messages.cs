using Plainion.GraphViz.Modules.CodeInspection.Actors;

namespace Plainion.GraphViz.Modules.CodeInspection.CallTree.Actors
{
    class CallTreeRequest
    {
        public string ConfigFile { get; set; }
        public bool AssemblyReferencesOnly { get; set; }
        public bool StrictCallsOnly { get; set; }
    }

    class CallTreeResponse : FinishedMessage
    {
        public string DotFile { get; set; }
    }

}
