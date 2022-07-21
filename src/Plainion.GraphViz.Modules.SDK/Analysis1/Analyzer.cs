using System.Threading;
using System.Threading.Tasks;

namespace Plainion.GraphViz.Modules.SDK.Analysis1
{
    class Analyzer
    {
        public Task<AnalysisDocument> AnalyzeAsync(string folderToAnalyze, CancellationToken token) 
        {
            return Task.FromResult(new AnalysisDocument());
        }
    }
}