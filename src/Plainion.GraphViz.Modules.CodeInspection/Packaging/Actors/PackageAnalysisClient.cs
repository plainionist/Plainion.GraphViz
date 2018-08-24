using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Plainion.GraphViz.Modules.CodeInspection.Actors;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Analyzers;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Actors
{
    [Export]
    class PackageAnalysisClient : ActorClientBase
    {
        public async Task<AnalysisDocument> AnalyseAsync(AnalysisRequest request, CancellationToken cancellationToken)
        {
            var msg = new AnalysisMessage
            {
                Spec = SpecUtils.Zip(request.Spec),
                PackagesToAnalyze = request.PackagesToAnalyze,
                OutputFile = Path.GetTempFileName(),
                UsedTypesOnly = request.UsedTypesOnly,
                AllEdges = request.AllEdges,
                CreateClustersForNamespaces = request.CreateClustersForNamespaces
            };

            if (request.Spec.Length * 2 > 4000000)
            {
                throw new NotSupportedException("Spec is too big");
            }

            var response = await this.ProcessAsync(typeof(PackageAnalysisActor), msg, cancellationToken);

            try
            {
                var serializer = new AnalysisDocumentSerializer();
                return serializer.Deserialize((string)response);
            }
            finally
            {
                File.Delete(msg.OutputFile);
            }
        }
    }
}
