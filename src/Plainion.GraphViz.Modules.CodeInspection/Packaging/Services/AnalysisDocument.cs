using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Infrastructure;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    // we use private properties here to support Json serialiation
    public class AnalysisDocument
    {
        private HashSet<string> myNodes { get; set; }
        private HashSet<Tuple<string, string>> myEdges { get; set; }
        private Dictionary<string, IEnumerable<string>> myClusters { get; set; }

        public AnalysisDocument()
        {
            myNodes = new HashSet<string>();
            myEdges = new HashSet<Tuple<string, string>>();
            myClusters = new Dictionary<string, IEnumerable<string>>();

            Captions = new List<Caption>();
        }

        public IEnumerable<string> Nodes { get { return myNodes; } }

        public IEnumerable<Tuple<string, string>> Edges { get { return myEdges; } }

        public IReadOnlyDictionary<string, IEnumerable<string>> Clusters { get { return myClusters; } }

        public List<Caption> Captions { get; private set; }

        public void AddNode( string nodeId )
        {
            myNodes.Add( nodeId );
        }

        public void AddEdge( string sourceNodeId, string targetNodeId )
        {
            myEdges.Add( Tuple.Create( sourceNodeId, targetNodeId ) );
        }

        public void AddToCluster( string clusterId, string nodeId )
        {
            IEnumerable<string> existing;
            if( !myClusters.TryGetValue( clusterId, out existing ) )
            {
                existing = new HashSet<string>();
                myClusters.Add( clusterId, existing );
            }

            ( ( HashSet<string> )existing ).Add( nodeId );
        }
    }
}
