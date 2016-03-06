using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Actors
{
    public class AnalysisResponse
    {
        public IEnumerable<string> Nodes { get; private set; }

        public IEnumerable<Tuple<string, string>> Edges { get; private set; }

        public IReadOnlyDictionary<string, IEnumerable<string>> Clusters { get; private set; }

        public IEnumerable<Caption> Captions { get; private set; }

        public static AnalysisResponse Create( AnalysisDocument doc )
        {
            return new AnalysisResponse
            {
                Nodes = doc.Nodes.ToList(),
                Edges = doc.Edges.ToList(),
                Clusters = doc.Clusters.ToDictionary( e => e.Key, e => e.Value ),
                Captions = doc.Captions.ToList()
            };
        }
    }
}
