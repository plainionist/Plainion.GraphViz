using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    /// <summary>
    /// Generates a "hide mask" removing all visible nodes not having edges of the given type.
    /// </summary>
    public class RemoveNodesWithoutEdges : AbstractAlgorithm
    {
        private Mode myMode;

        public enum Mode
        {
            /// <summary>
            /// Remove nodes without any siblings
            /// </summary>
            All,
            /// <summary>
            /// Remove nodes without sources
            /// </summary>
            Sources,
            /// <summary>
            /// Remove nodes without targets
            /// </summary>
            Targets
        }

        public RemoveNodesWithoutEdges(IGraphPresentation presentation)
            : this(presentation, Mode.All)
        {
        }

        public RemoveNodesWithoutEdges(IGraphPresentation presentation, Mode mode)
            : base(presentation)
        {
            myMode = mode;
        }

        public INodeMask Compute()
        {
            var transformationModule = Presentation.GetModule<ITransformationModule>();
            return Compute(transformationModule.Graph.Nodes);
        }

        private INodeMask Compute(IEnumerable<Node> nodes)
        {
            var nodesToHide = nodes
                .Where(n => HideNode(n));

            var mask = new NodeMask();
            mask.IsShowMask = false;
            mask.Set(nodesToHide);
            mask.Label = "Nodes without ";

            if (myMode == Mode.Sources)
            {
                mask.Label += "sources";
            }
            else if (myMode == Mode.Targets)
            {
                mask.Label += "targets";
            }
            else
            {
                mask.Label += "siblings";
            }

            return mask;
        }

        public INodeMask Compute(Cluster cluster)
        {
            return Compute(cluster.Nodes.Where(n => Presentation.Picking.Pick(n)));
        }

        private bool HideNode(Node node)
        {
            var noSources = !node.In.Any(e => Presentation.Picking.Pick(e));
            var noTargets = !node.Out.Any(e => Presentation.Picking.Pick(e));

            if (myMode == Mode.All && noSources && noTargets)
            {
                return true;
            }
            else if (myMode == Mode.Sources && noSources)
            {
                return true;
            }
            else if (myMode == Mode.Targets && noTargets)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
