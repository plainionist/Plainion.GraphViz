using Plainion.GraphViz.Actors.Client;

namespace Plainion.GraphViz.Modules.CodeInspection.PathFinder.Actors
{
    class PathFinderRequest
    {
        public string ConfigFile { get; set; }
        public bool AssemblyReferencesOnly { get; set; }
    }

    class PathFinderResponse : FinishedMessage
    {
        public string DotFile { get; set; }
    }

}
