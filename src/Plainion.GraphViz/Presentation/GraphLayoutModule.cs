using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Plainion.GraphViz.Dot;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    class GraphLayoutModule : AbstractModule<AbstractPropertySet>, IGraphLayoutModule
    {
        private Dictionary<string, NodeLayout> myNodeLayouts;
        private Dictionary<string, EdgeLayout> myEdgeLayouts;
        private LayoutAlgorithm myLayoutAlgorithm;

        public GraphLayoutModule()
        {
            myNodeLayouts = new Dictionary<string, NodeLayout>();
            myEdgeLayouts = new Dictionary<string, EdgeLayout>();

            Algorithm = LayoutAlgorithm.Auto;
        }

        public LayoutAlgorithm Algorithm
        {
            get { return myLayoutAlgorithm; }
            set { SetProperty( ref myLayoutAlgorithm, value ); }
        }

        public void Add( NodeLayout layout )
        {
            myNodeLayouts.Add( layout.OwnerId, layout );

            OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, layout ) );
        }

        public void Add( EdgeLayout layout )
        {
            myEdgeLayouts.Add( layout.OwnerId, layout );

            OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, layout ) );
        }

        public void Clear()
        {
            var items = Items.ToList();

            myNodeLayouts.Clear();
            myEdgeLayouts.Clear();

            OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, items ) );
        }

        public NodeLayout GetLayout( Node node )
        {
            if( !myNodeLayouts.ContainsKey( node.Id ) )
            {
                return null;
            }

            return myNodeLayouts[ node.Id ];
        }

        public EdgeLayout GetLayout( Edge edge )
        {
            if( !myEdgeLayouts.ContainsKey( edge.Id ) )
            {
                return null;
            }

            return myEdgeLayouts[ edge.Id ];
        }

        public void Set( IEnumerable<NodeLayout> nodeLayouts, IEnumerable<EdgeLayout> edgeLayouts )
        {
            Clear();

            foreach( var layout in nodeLayouts )
            {
                Add( layout );
            }

            foreach( var layout in edgeLayouts )
            {
                Add( layout );
            }

            OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, Items ) );
        }

        public override IEnumerable<AbstractPropertySet> Items
        {
            get
            {
                return myNodeLayouts
                    .OfType<AbstractPropertySet>()
                    .Concat( myEdgeLayouts.OfType<AbstractPropertySet>() )
                    .ToList();
            }
        }
    }
}
