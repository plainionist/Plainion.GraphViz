using System.IO;
using System.Linq;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Dot
{
    public class DotWriter
    {
        private readonly string myPath;

        public DotWriter(string path)
        {
            Contract.RequiresNotNull(path, "path");

            myPath = path;
        }

        internal int? FastRenderingNodeCountLimit { get; set; }

        // TODO: we have to consider StyleModule
        // http://www.graphviz.org/Gallery/directed/cluster.html
        // returns written nodes
        public int Write(IGraphPresentation presentation)
        {
            var labelModule = presentation.GetPropertySetFor<Caption>();
            var transformationModule = presentation.GetModule<ITransformationModule>();

            using (var writer = new StreamWriter(myPath))
            {
                writer.WriteLine("digraph {");

                writer.WriteLine("  ratio=\"compress\"");
                writer.WriteLine("  rankdir=BT");
                writer.WriteLine("  ranksep=\"2.0 equally\"");

                var visibleNodes = transformationModule.Graph.Nodes
                    .Where(n => presentation.Picking.Pick(n))
                    .ToList();

                if (FastRenderingNodeCountLimit.HasValue && visibleNodes.Count > FastRenderingNodeCountLimit.Value)
                {
                    // http://www.graphviz.org/content/attrs#dnslimit
                    writer.WriteLine("  nslimit=0.2");
                    writer.WriteLine("  nslimit1=0.2");
                    writer.WriteLine("  splines=line");
                    writer.WriteLine("  mclimit=0.5");
                }

                foreach (var cluster in transformationModule.Graph.Clusters)
                {
                    var visibleClusterNodes = cluster.Nodes
                        .Where(visibleNodes.Contains)
                        .ToList();

                    if (visibleClusterNodes.Count == 0)
                    {
                        continue;
                    }

                    var clusterId = cluster.Id.StartsWith("cluster_") ? cluster.Id : "cluster_" + cluster.Id;
                    writer.WriteLine("  subgraph \"" + clusterId + "\" {");

                    // pass label to trigger dot.exe to create proper size of node bounding box
                    writer.WriteLine("    label = \"{0}\"", labelModule.Get(cluster.Id).DisplayText);

                    foreach (var node in visibleClusterNodes)
                    {
                        // pass label to trigger dot.exe to create proper size of node bounding box
                        writer.WriteLine("    \"{0}\" [label=\"{1}\"]", node.Id, labelModule.Get(node.Id).DisplayText);
                    }

                    writer.WriteLine("  }");
                }

                foreach (var node in visibleNodes)
                {
                    // pass label to trigger dot.exe to create proper size of node bounding box
                    var label = labelModule.Get(node.Id).DisplayText;

                    writer.WriteLine("  \"{0}\" [label=\"{1}\"]", node.Id, label);
                }

                foreach (var edge in transformationModule.Graph.Edges.Where(e => presentation.Picking.Pick(e)))
                {
                    // pass label to trigger dot.exe to create position of the label
                    var label = labelModule.Get(edge.Id);

                    // always pass label otherwise parser will fail :(
                    writer.WriteLine("  \"{0}\" -> \"{1}\" [label=\"{2}\"]", edge.Source.Id, edge.Target.Id, label.DisplayText != label.OwnerId ? label.DisplayText : ".");
                }

                writer.WriteLine("}");

                return visibleNodes.Count;
            }
        }
    }
}
