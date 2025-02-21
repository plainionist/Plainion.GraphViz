using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Plainion.GraphViz.CodeInspection;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Analyzers
{
    public class AnalysisDocument
    {
        private readonly HashSet<string> myNodes;
        // key: edge, value: count
        private readonly Dictionary<Tuple<string, string>, int> myEdges;
        private readonly Dictionary<string, IEnumerable<string>> myClusters;
        // key: id, value: caption
        private readonly Dictionary<string, string> myCaptions;
        // key: id, value: color
        private readonly Dictionary<string, string> myNodeStyles;
        // key: id, value: color
        private readonly Dictionary<string, string> myEdgeStyles;

        public AnalysisDocument()
        {
            myNodes = [];
            myEdges = [];
            myClusters = [];

            myCaptions = [];
            myNodeStyles = [];
            myEdgeStyles = [];
        }

        public IEnumerable<string> Nodes { get { return myNodes; } }

        public IReadOnlyCollection<Tuple<string, string, int>> Edges{ get; set; }

        [OnSerializing]
        private void OnSerializing(StreamingContext _)
        {
            Edges = myEdges.Select(x => Tuple.Create(x.Key.Item1, x.Key.Item2, x.Value)).ToList();
        }

        public IReadOnlyDictionary<string, IEnumerable<string>> Clusters { get { return myClusters; } }

        public IReadOnlyDictionary<string, string> Captions { get { return myCaptions; } }

        public IReadOnlyDictionary<string, string> NodeStyles { get { return myNodeStyles; } }

        public IReadOnlyDictionary<string, string> EdgeStyles { get { return myEdgeStyles; } }

        public void Add(Reference reference)
        {
            var edge = Tuple.Create(NodeId(reference.From), NodeId(reference.To));
            if (!myEdges.TryGetValue(edge, out var count))
            {
                myEdges.Add(edge, 1);
            }
            else
            {
                myEdges[edge] = count + 1;
            }
        }

        private string NodeId(Type t) => t.Namespace != null ? t.FullName : t.AssemblyQualifiedName;

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
