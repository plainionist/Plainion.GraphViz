using System.Collections.Generic;
using Plainion.GraphViz.Infrastructure;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Actors
{
    public abstract class AbstractGraphDocument 
    {
        private RelaxedGraphBuilder myGraphBuilder;
        private IList<FailedItem> myFailedItems;

        protected AbstractGraphDocument()
        {
            myGraphBuilder = new RelaxedGraphBuilder();
            myFailedItems = new List<FailedItem>();
        }

        public virtual IGraph Graph
        {
            get { return myGraphBuilder.Graph; }
        }

        public string Filename
        {
            get;
            private set;
        }

        public void Load(string path)
        {
            myGraphBuilder = new RelaxedGraphBuilder();
            Filename = path;

            Load();
        }

        protected abstract void Load();

        protected internal Node TryAddNode(string nodeId)
        {
            var node = myGraphBuilder.TryAddNode(nodeId);
            if (node == null)
            {
                myFailedItems.Add(new FailedItem(nodeId, "Node already exists"));
                return null;
            }

            return node;
        }

        protected internal Edge TryAddEdge(string sourceNodeId, string targetNodeId)
        {
            var edge = myGraphBuilder.TryAddEdge(sourceNodeId, targetNodeId);

            if (edge == null)
            {
                myFailedItems.Add(new FailedItem(Edge.CreateId(sourceNodeId, targetNodeId), "Edge already exists"));
                return null;
            }

            return edge;
        }

        protected internal Cluster TryAddCluster(string clusterId, IEnumerable<string> nodes)
        {
            var cluster = myGraphBuilder.TryAddCluster(clusterId, nodes);
            if (cluster == null)
            {
                myFailedItems.Add(new FailedItem(clusterId, "Cluster already exists"));
                return null;
            }

            return cluster;
        }

        public IEnumerable<FailedItem> FailedItems
        {
            get { return myFailedItems; }
        }
    }

    class AnalysisDocument : AbstractGraphDocument
    {
        private List<Caption> myCaptions;
        private List<NodeStyle> myNodeStyles;
        private List<EdgeStyle> myEdgeStyles;

        public AnalysisDocument()
        {
            myCaptions = new List<Caption>();
            myNodeStyles = new List<NodeStyle>();
            myEdgeStyles = new List<EdgeStyle>();
        }

        public IEnumerable<Caption> Captions
        {
            get { return myCaptions; }
        }

        public IEnumerable<NodeStyle> NodeStyles
        {
            get { return myNodeStyles; }
        }

        public IEnumerable<EdgeStyle> EdgeStyles
        {
            get { return myEdgeStyles; }
        }

        internal void Add(Caption caption)
        {
            myCaptions.Add(caption);
        }

        internal void Add(NodeStyle style)
        {
            myNodeStyles.Add(style);
        }

        internal void Add(EdgeStyle style)
        {
            myEdgeStyles.Add(style);
        }

        protected override void Load()
        {
            throw new System.NotImplementedException();
        }
    }
}
