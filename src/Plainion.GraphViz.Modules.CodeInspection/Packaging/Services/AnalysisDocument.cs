using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    // we use private properties here to support Json serialiation
    [Serializable]
    class AnalysisDocument
    {
        private HashSet<string> myNodes { get; set; }
        private HashSet<Tuple<string, string>> myEdges { get; set; }
        private Dictionary<string, IEnumerable<string>> myClusters { get; set; }
        // key: id, value: caption
        private Dictionary<string, string> myCaptions { get; set; }
        // key: id, value: color
        private Dictionary<string, string> myNodeStyles { get; set; }
        // key: id, value: color
        private Dictionary<string, string> myEdgeStyles { get; set; }

        public AnalysisDocument()
        {
            myNodes = new HashSet<string>();
            myEdges = new HashSet<Tuple<string, string>>();
            myClusters = new Dictionary<string, IEnumerable<string>>();

            myCaptions = new Dictionary<string, string>();
            myNodeStyles = new Dictionary<string, string>();
            myEdgeStyles = new Dictionary<string, string>();
        }

        public IEnumerable<string> Nodes { get { return myNodes; } }

        public IEnumerable<Tuple<string, string>> Edges { get { return myEdges; } }

        public IReadOnlyDictionary<string, IEnumerable<string>> Clusters { get { return myClusters; } }

        public IReadOnlyDictionary<string, string> Captions { get { return myCaptions; } }

        public IReadOnlyDictionary<string, string> NodeStyles { get { return myNodeStyles; } }

        public IReadOnlyDictionary<string, string> EdgeStyles { get { return myEdgeStyles; } }

        internal void Add(IReadOnlyCollection<Edge> edges)
        {
            foreach (var edge in edges)
            {
                myEdges.Add(Tuple.Create(edge.Source.FullName, edge.Target.FullName));

                string edgeBrush = null;
                if (edge.EdgeType == EdgeType.DerivesFrom || edge.EdgeType == EdgeType.Implements)
                {
                    edgeBrush = "Blue";
                }
                else if (edge.EdgeType != EdgeType.Calls)
                {
                    edgeBrush = "Gray";
                }

                if (edgeBrush != null)
                {
                    var edgeId = Model.Edge.CreateId(edge.Source.FullName, edge.Target.FullName);
                    if (!myEdgeStyles.ContainsKey(edgeId))
                    {
                        myEdgeStyles.Add(edgeId, edgeBrush);
                    }
                }
            }
        }

        internal void Add(Type node, Package package)
        {
            Add(node, package, null);
        }

        internal void Add(Type node, Package package, string fillColor)
        {
            myNodes.Add(node.FullName);

            if (!myCaptions.ContainsKey(node.FullName))
            {
                myCaptions.Add(node.FullName, node.Name);
            }

            // in case multiple cluster match we just take the first one
            var matchedCluster = package.Clusters.FirstOrDefault(c => c.Matches(node.FullName));
            if (matchedCluster != null)
            {
                IEnumerable<string> existing;
                if (!myClusters.TryGetValue(matchedCluster.Name, out existing))
                {
                    existing = new HashSet<string>();
                    myClusters.Add(matchedCluster.Name, existing);
                }

                ((HashSet<string>)existing).Add(node.FullName);
            }

            if (fillColor != null && !myNodeStyles.ContainsKey(node.FullName))
            {
                myNodeStyles.Add(node.FullName, fillColor);
            }
        }
    }
}
