using System.Collections.Generic;
using Plainion.GraphViz.Infrastructure;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Modules.Documents
{
    public abstract class AbstractGraphDocument : IGraphDocument
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

        public void Load( string path )
        {
            myGraphBuilder = new RelaxedGraphBuilder();
            Filename = path;

            Load();
        }

        protected abstract void Load();

        protected Node TryAddNode( string nodeId )
        {
            var node = myGraphBuilder.TryAddNode( nodeId );
            if( node == null )
            {
                myFailedItems.Add( new FailedItem( nodeId, "Node already exists" ) );
                return null;
            }

            return node;
        }

        protected Edge TryAddEdge( string sourceNodeId, string targetNodeId )
        {
            var edge = myGraphBuilder.TryAddEdge( sourceNodeId, targetNodeId );

            if( edge == null )
            {
                myFailedItems.Add( new FailedItem( Edge.CreateId( sourceNodeId, targetNodeId ), "Edge already exists" ) );
                return null;
            }

            return edge;
        }

        public IEnumerable<FailedItem> FailedItems
        {
            get { return myFailedItems; }
        }
    }
}
