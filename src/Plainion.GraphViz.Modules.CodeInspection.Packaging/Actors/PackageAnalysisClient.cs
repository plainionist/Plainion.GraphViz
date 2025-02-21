using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Plainion.GraphViz.Actors.Client;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Analyzers;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Actors
{
    class PackageAnalysisClient : ActorClientBase
    {
        public async Task<AnalysisDocument> AnalyseAsync(AnalysisRequest request, CancellationToken cancellationToken = default)
        {
            var msg = new AnalysisMessage
            {
                Spec = SpecUtils.Zip(request.Spec),
                PackagesToAnalyze = request.PackagesToAnalyze,
                OutputFile = Path.GetTempFileName(),
            };

            if (request.Spec.Length * 2 > 4000000)
            {
                throw new NotSupportedException("Spec is too big");
            }

            var response = await this.ProcessAsync(typeof(PackageAnalysisActor), msg, cancellationToken);

            try
            {
                if (response is AnalysisResponse m)
                {
                    var serializer = new DocumentSerializer();
                    return serializer.Deserialize<AnalysisDocument>(m.File);
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                File.Delete(msg.OutputFile);
            }
        }
    }
}
