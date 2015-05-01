using System.Collections.Generic;
using System.Collections.Specialized;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    /// <summary>
    /// Edges can only be hidden additionally - we do not support showing edges on its own
    /// </summary>
    internal class EdgeMaskModule : AbstractModule<Edge>, IEdgeMaskModule
    {
        private List<Edge> myEdgesToHide;

        public EdgeMaskModule()
        {
            myEdgesToHide = new List<Edge>();
        }

        public void Add( Edge edge )
        {
            myEdgesToHide.Add( edge );

            RaiseCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, edge ) );
        }

        public void Remove( Edge edge )
        {
            myEdgesToHide.Remove( edge );

            RaiseCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, edge ) );
        }

        public override IEnumerable<Edge> Items
        {
            get
            {
                return myEdgesToHide;
            }
        }
    }
}
