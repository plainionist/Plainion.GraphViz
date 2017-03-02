using System.IO;
using System.Linq;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Dot
{
    // Hint: also for rendering we pass label to trigger dot.exe to create proper size of node bounding box
    public class DotWriter
    {
        private readonly string myPath;

        public DotWriter(string path)
        {
            Contract.RequiresNotNull(path, "path");

            myPath = path;
        }

        internal int? FastRenderingNodeCountLimit { get; set; }

        internal bool IgnoreStyle { get; set; }

        // http://www.graphviz.org/Gallery/directed/cluster.html
        // returns written nodes
        public int Write(IGraph graph, IGraphPicking picking, IModuleRepository modules)
        {
            Contract.RequiresNotNull(graph, "graph");
            Contract.RequiresNotNull(picking, "picking");
            Contract.RequiresNotNull(modules, "modules");

            return new WriteAction(this, graph, picking, modules).Execute();
        }

        private class WriteAction
        {
            private readonly DotWriter myOwner;
            private readonly IGraph myGraph;
            private readonly IGraphPicking myPicking;
            private readonly IPropertySetModule<Caption> myCaptions;
            private readonly IPropertySetModule<NodeStyle> myNodeStyles;
            private readonly IPropertySetModule<EdgeStyle> myEdgeStyles;
            private TextWriter myWriter;

            public WriteAction(DotWriter owner, IGraph graph, IGraphPicking picking, IModuleRepository modules)
            {
                myOwner = owner;
                myGraph = graph;
                myPicking = picking;

                myCaptions = modules.GetPropertySetFor<Caption>();
                myNodeStyles = modules.GetPropertySetFor<NodeStyle>();
                myEdgeStyles = modules.GetPropertySetFor<EdgeStyle>();
            }

            public int Execute()
            {
                using (myWriter = new StreamWriter(myOwner.myPath))
                {
                    myWriter.WriteLine("digraph {");

                    myWriter.WriteLine("  ratio=\"compress\"");
                    myWriter.WriteLine("  rankdir=BT");
                    myWriter.WriteLine("  ranksep=\"2.0 equally\"");

                    var relevantNodes = myGraph.Nodes
                        .Where(n => myPicking.Pick(n))
                        .ToList();

                    if (myOwner.FastRenderingNodeCountLimit.HasValue && relevantNodes.Count > myOwner.FastRenderingNodeCountLimit.Value)
                    {
                        // http://www.graphviz.org/content/attrs#dnslimit
                        myWriter.WriteLine("  nslimit=0.2");
                        myWriter.WriteLine("  nslimit1=0.2");
                        myWriter.WriteLine("  splines=line");
                        myWriter.WriteLine("  mclimit=0.5");
                    }

                    foreach (var cluster in myGraph.Clusters)
                    {
                        var relevantClusterNodes = cluster.Nodes
                            .Where(relevantNodes.Contains)
                            .ToList();

                        if (relevantClusterNodes.Count == 0)
                        {
                            continue;
                        }

                        var clusterId = cluster.Id.StartsWith("cluster_") ? cluster.Id : "cluster_" + cluster.Id;
                        myWriter.WriteLine("  subgraph \"" + clusterId + "\" {");

                        myWriter.WriteLine("    label = \"{0}\"", myCaptions.Get(cluster.Id).DisplayText);

                        foreach (var node in relevantClusterNodes)
                        {
                            Write(node, "    ");
                        }

                        myWriter.WriteLine("  }");
                    }

                    foreach (var node in relevantNodes)
                    {
                        Write(node, "  ");
                    }

                    var relevantEdges = myGraph.Edges.Where(e => myPicking.Pick(e));

                    foreach (var edge in relevantEdges)
                    {
                        Write(edge, "  ");
                    }

                    myWriter.WriteLine("}");

                    return relevantNodes.Count;
                }
            }

            private void Write(Node node, string indent)
            {
                myWriter.Write(indent);

                var label = myCaptions.Get(node.Id).DisplayText;
                myWriter.Write("\"{0}\" [label=\"{1}\"", node.Id, label);

                if (!myOwner.IgnoreStyle)
                {
                    var fillColor = myNodeStyles.Get(node.Id).FillColor;
                    myWriter.Write(", color={0}", fillColor);
                }

                myWriter.WriteLine("]");
            }

            private void Write(Edge edge, string indent)
            {
                var label = myCaptions.Get(edge.Id);

                // Hint (rendering): always pass label otherwise parser will fail :(
                myWriter.Write(indent);

                myWriter.Write("\"{0}\" -> \"{1}\" [label=\"{2}\"",
                    edge.Source.Id,
                    edge.Target.Id,
                    label.DisplayText != label.OwnerId ? label.DisplayText : ".");

                if (!myOwner.IgnoreStyle)
                {
                    var color = myEdgeStyles.Get(edge.Id).Color;
                    myWriter.Write(", color={0}", color);
                }

                myWriter.WriteLine("]");
            }
        }
    }
}
