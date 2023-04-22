﻿using System.Threading;
using System.Threading.Tasks;
using Plainion.GraphViz.Modules.CodeInspection.Actors;

namespace Plainion.GraphViz.Modules.CodeInspection.PathFinder.Actors
{
    class PathFinderClient : ActorClientBase
    {
        public async Task<string> AnalyzeAsync(PathFinderRequest request, CancellationToken cancellationToken)
        {
            var response = await this.ProcessAsync(typeof(PathFinderActor), request, cancellationToken);

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
