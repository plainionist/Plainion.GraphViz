using System;
using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    /// <summary>
    /// Describes all nodes directly and indirectly connected to the given one
    /// </summary>
    public class TransitiveHull
    {
        private readonly IGraphPresentation myPresentation;
        private bool myShow;

        public TransitiveHull( IGraphPresentation presentation, bool show )
        {
            Contract.RequiresNotNull( presentation, "presentation" );

            myPresentation = presentation;
            myShow = show;
        }

        public void Execute( IReadOnlyCollection<Node> nodes )
        {
            var connectedNodes = nodes
                .SelectMany(n => GetReachableNodes(n))
                .Distinct();

            var mask = new NodeMask();
            mask.IsShowMask = myShow;
            if (myShow)
            {
                mask.Set(connectedNodes);
            }
            else
            {
                var transformationModule = myPresentation.GetModule<ITransformationModule>();
                mask.Set(transformationModule.Graph.Nodes.Except(connectedNodes));
            }

            if (nodes.Count == 1)
            {
                var caption = myPresentation.GetPropertySetFor<Caption>().Get(nodes.First().Id);
                mask.Label = "Transitive hull of " + caption.DisplayText;
            }
            else
            {
                mask.Label = "Transitive hull of multiple nodes";
            }

            var module = myPresentation.GetModule<INodeMaskModule>();
            module.Push(mask);
        }

        private IEnumerable<Node> GetReachableNodes( Node node )
        {
            var connectedNodes = new HashSet<Node>();
            connectedNodes.Add( node );

            var recursiveSiblings = Traverse.BreathFirst( new[] { node }, SelectSiblings )
                .SelectMany( e => new[] { e.Source, e.Target } );

            foreach( var n in recursiveSiblings )
            {
                connectedNodes.Add( n );
            }

            return connectedNodes;
        }

        private IEnumerable<Edge> SelectSiblings( Node n )
        {
            if( myShow )
            {
                return n.Out;
            }
            else
            {
                return n.Out.Where( e => myPresentation.Picking.Pick( e.Target ) );
            }
        }
    }
}
