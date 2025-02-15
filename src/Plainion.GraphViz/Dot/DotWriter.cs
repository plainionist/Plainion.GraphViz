using System;
using System.Collections.Generic;
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
            Settings = DotPresets.Default;
        }

        public int? FastRenderingNodeCountLimit { get; set; }

        public bool IgnoreStyle { get; set; }

        public bool PrettyPrint { get; set; }

        public DotSettings Settings { get; set; }

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

                    ApplySettings();

                    var relevantNodes = myGraph.Nodes
                        .Where(n => myPicking.Pick(n))
                        .OrderByIf(n => n.Id, myOwner.PrettyPrint)
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
                            .OrderByIf(n => n.Id, myOwner.PrettyPrint)
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

                    var relevantEdges = myGraph.Edges
                        .Where(e => myPicking.Pick(e))
                        .OrderByIf(e => e.Id, myOwner.PrettyPrint);

                    foreach (var edge in relevantEdges)
                    {
                        Write(edge, "  ");
                    }

                    myWriter.WriteLine("}");

                    return relevantNodes.Count;
                }
            }

            private void ApplySettings()
            {
                if (myOwner.Settings == null)
                {
                    return;
                }

                if (myOwner.Settings.Ratio != null)
                {
                    myWriter.WriteLine("  ratio=\"{0}\"", myOwner.Settings.Ratio);
                }
                if (myOwner.Settings.RankDir != null)
                {
                    myWriter.WriteLine("  rankdir={0}", myOwner.Settings.RankDir);
                }
                if (myOwner.Settings.RankSep != null)
                {
                    myWriter.WriteLine("  ranksep=\"{0}\"", myOwner.Settings.RankSep);
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
                    myWriter.Write(", color=\"{0}\"", fillColor);
                }

                myWriter.WriteLine("]");
            }

            private void Write(Edge edge, string indent)
            {
                var label = myCaptions.TryGet(edge.Id);

                myWriter.Write(indent);

                myWriter.Write("\"{0}\" -> \"{1}\"", edge.Source.Id, edge.Target.Id);

                var attributes = new List<string>();

                if (edge.Weight != 1)
                {
                    attributes.Add($"weight=\"{edge.Weight}\"");
                }

                if (label != null && label.DisplayText != label.OwnerId)
                {
                    attributes.Add($"label=\"{label.DisplayText}\"");
                }

                if (!myOwner.IgnoreStyle)
                {
                    var color = myEdgeStyles.Get(edge.Id).Color;
                    attributes.Add($"color=\"{color}\"");
                }

                if (attributes.Any())
                {
                    myWriter.Write(" [{0}]", string.Join(", ", attributes));
                }

                myWriter.WriteLine();
            }
        }
    }

    internal static class OrderExtensions
    {
        public static IEnumerable<TSource> OrderByIf<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, bool doIt)
        {
            return doIt ? source.OrderBy(keySelector) : source;
        }
    }
}
