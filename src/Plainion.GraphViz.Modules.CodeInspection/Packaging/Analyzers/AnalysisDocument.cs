using System;
using System.Collections.Generic;
using Plainion.GraphViz.Modules.CodeInspection.Core;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Analyzers
{
    [Serializable]
    class AnalysisDocument
    {
        private HashSet<string> myNodes;
        private HashSet<Tuple<string, string>> myEdges;
        private Dictionary<string, IEnumerable<string>> myClusters;
        // key: id, value: caption
        private Dictionary<string, string> myCaptions;
        // key: id, value: color
        private Dictionary<string, string> myNodeStyles;
        // key: id, value: color
        private Dictionary<string, string> myEdgeStyles;

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

        public void Add(Reference edge)
        {
            myEdges.Add(CreateEdge(edge));
        }

        private string NodeId(Type t) => t.Namespace != null ? t.FullName : t.AssemblyQualifiedName;

        private Tuple<string, string> CreateEdge(Reference edge) => Tuple.Create(NodeId(edge.From), NodeId(edge.To));

        public void AddEdgeColor(Reference edge, string color)
        {
            var edgeId = Model.Edge.CreateId(NodeId(edge.From), NodeId(edge.To));
            if (!myEdgeStyles.ContainsKey(edgeId))
            {
                myEdgeStyles.Add(edgeId, color);
            }
        }

        public void Add(Type node)
        {
            var nodeId = NodeId(node);

            myNodes.Add(nodeId);

            if (!myCaptions.ContainsKey(nodeId))
            {
                myCaptions.Add(nodeId, node.Name);
            }
        }

        public void AddToCluster(Type node, Cluster cluster)
        {
            IEnumerable<string> existing;
            if (!myClusters.TryGetValue(cluster.Id, out existing))
            {
                existing = new HashSet<string>();
                myClusters.Add(cluster.Id, existing);

                // accept if already exists
                myCaptions[cluster.Id] = cluster.Name;
            }

            ((HashSet<string>)existing).Add(NodeId(node));
        }

        public void AddNodeColor(Type node, string fillColor)
        {
            var nodeId = NodeId(node);

            if (!myNodeStyles.ContainsKey(nodeId))
            {
                myNodeStyles.Add(nodeId, fillColor);
            }
        }
    }
}
