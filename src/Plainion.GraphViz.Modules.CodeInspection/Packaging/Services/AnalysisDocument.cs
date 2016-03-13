using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    // we use private properties here to support Json serialiation
    class AnalysisDocument
    {
        private HashSet<string> myNodes { get; set; }
        private HashSet<Tuple<string, string>> myEdges { get; set; }
        private Dictionary<string, IEnumerable<string>> myClusters { get; set; }
        private List<Caption> myCaptions { get; set; }
        private List<NodeStyle> myNodeStyles { get; set; }
        private List<EdgeStyle> myEdgeStyles { get; set; }

        public AnalysisDocument()
        {
            myNodes = new HashSet<string>();
            myEdges = new HashSet<Tuple<string, string>>();
            myClusters = new Dictionary<string, IEnumerable<string>>();

            myCaptions = new List<Caption>();
            myNodeStyles = new List<NodeStyle>();
            myEdgeStyles = new List<EdgeStyle>();
        }

        public IEnumerable<string> Nodes { get { return myNodes; } }

        public IEnumerable<Tuple<string, string>> Edges { get { return myEdges; } }

        public IReadOnlyDictionary<string, IEnumerable<string>> Clusters { get { return myClusters; } }

        public IEnumerable<Caption> Captions { get { return myCaptions; } }

        public IEnumerable<NodeStyle> NodeStyles { get { return myNodeStyles; } }

        public IEnumerable<EdgeStyle> EdgeStyles { get { return myEdgeStyles; } }

        public void AddNode(string nodeId)
        {
            myNodes.Add(nodeId);
        }

        public void AddEdge(string sourceNodeId, string targetNodeId)
        {
            myEdges.Add(Tuple.Create(sourceNodeId, targetNodeId));
        }

        public void AddToCluster(string clusterId, string nodeId)
        {
            IEnumerable<string> existing;
            if (!myClusters.TryGetValue(clusterId, out existing))
            {
                existing = new HashSet<string>();
                myClusters.Add(clusterId, existing);
            }

            ((HashSet<string>)existing).Add(nodeId);
        }

        public void Add(Caption caption)
        {
            if (!myCaptions.Any(c => c.OwnerId == caption.OwnerId))
            {
                myCaptions.Add(caption);
            }
        }

        public void Add( NodeStyle nodeStyle )
        {
            if( !myNodeStyles.Any( n => n.OwnerId == nodeStyle.OwnerId ) )
            {
                myNodeStyles.Add( nodeStyle );
            }
        }

        public void Add( EdgeStyle edgeStyle )
        {
            if( !myEdgeStyles.Any( e => e.OwnerId == edgeStyle.OwnerId ) )
            {
                myEdgeStyles.Add( edgeStyle );
            }
        }

        internal void Add( IReadOnlyCollection<Edge> edges )
        {
            foreach( var edge in edges )
            {
                AddEdge( edge.Source.FullName, edge.Target.FullName );

                Brush edgeBrush = null;
                if( edge.EdgeType == EdgeType.DerivesFrom || edge.EdgeType == EdgeType.Implements )
                {
                    edgeBrush = Brushes.Blue;
                }
                else if( edge.EdgeType != EdgeType.Calls )
                {
                    edgeBrush = Brushes.Brown;
                }

                if( edgeBrush != null )
                {
                    var edgeId = Model.Edge.CreateId( edge.Source.FullName, edge.Target.FullName );
                    Add( new EdgeStyle( edgeId ) { Color = edgeBrush } );
                }
            }
        }

        internal void Add( Type node, Package package )
        {
            AddNode( node.FullName );
            Add( new Caption( node.FullName, node.Name ) );

            // in case multiple cluster match we just take the first one
            var matchedCluster = package.Clusters.FirstOrDefault( c => c.Matches( node.FullName ) );
            if( matchedCluster != null )
            {
                AddToCluster( matchedCluster.Name, node.FullName );
            }
        }
    }
}
