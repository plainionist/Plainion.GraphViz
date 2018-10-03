using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Plainion.GraphViz.Modules.CodeInspection.Common.Actors;

namespace Plainion.GraphViz.Modules.CodeInspection.CallTree.Actors
{
    [Export]
    class CallTreeClient : ActorClientBase
    {
        public async Task<string> AnalyzePathAsync(string configFile, bool assemblyReferencesOnly, bool strictCallsOnly, CancellationToken cancellationToken)
        {
            var msg = new CallTreeRequest
            {
                ConfigFile = configFile,
                AssemblyReferencesOnly = assemblyReferencesOnly,
                StrictCallsOnly = strictCallsOnly
            };

            var response = await this.ProcessAsync(typeof(CallTreeActor), msg, cancellationToken);

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
