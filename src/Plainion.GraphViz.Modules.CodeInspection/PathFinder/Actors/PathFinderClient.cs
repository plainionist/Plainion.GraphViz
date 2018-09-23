using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Plainion.GraphViz.Modules.CodeInspection.Actors;

namespace Plainion.GraphViz.Modules.CodeInspection.PathFinder.Actors
{
    [Export]
    class PathFinderClient : ActorClientBase
    {
        public async Task<string> AnalyzePathAsync(string configFile, bool assemblyReferencesOnly, CancellationToken cancellationToken)
        {
            var msg = new PathFinderRequest
            {
                ConfigFile = configFile,
                AssemblyReferencesOnly = assemblyReferencesOnly
            };

            var response = await this.ProcessAsync(typeof(PathFinderActor), msg, cancellationToken);

            if (response is PathFinderResponse m)
            {
                return m.DotFile;
            }
            else
            {
                return null;
            }
        }
    }
}
