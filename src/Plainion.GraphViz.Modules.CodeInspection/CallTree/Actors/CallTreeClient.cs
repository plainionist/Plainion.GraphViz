using System.Threading;
using System.Threading.Tasks;
using Plainion.GraphViz.Modules.CodeInspection.Actors;

namespace Plainion.GraphViz.Modules.CodeInspection.CallTree.Actors
{
    class CallTreeClient : ActorClientBase
    {
        public async Task<string> AnalyzeAsync(CallTreeRequest request, CancellationToken cancellationToken)
        {
            var response = await this.ProcessAsync(typeof(CallTreeActor), request, cancellationToken);

            if (response is CallTreeResponse m)
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
