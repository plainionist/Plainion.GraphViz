using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Plainion.Graphs;
using Plainion.Graphs.Projections;
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

        public bool IgnoreStyle { get; set; }

        public bool PrettyPrint { get; set; }

        public void Write(IGraph graph, IGraphPicking picking, IModuleRepository modules)
        {
            Contract.RequiresNotNull(graph, "graph");
            Contract.RequiresNotNull(picking, "picking");
            Contract.RequiresNotNull(modules, "modules");

            new WriteAction(this, graph, picking, modules).Execute();
        }

        private class WriteAction
        {
            private readonly DotWriter myOwner;
            private readonly IGraph myGraph;
            private readonly IGraphPicking myPicking;
            private readonly IPropertySetModule<Caption> myCaptions;
            private readonly IPropertySetModule<NodeStyle> myNodeStyles;
            private readonly IPropertySetModule<EdgeStyle> myEdgeStyles;
            private readonly Dictionary<string,string> myGraphAttributes;
            private TextWriter myWriter;

            public WriteAction(DotWriter owner, IGraph graph, IGraphPicking picking, IModuleRepository modules)
            {
                myOwner = owner;
                myGraph = graph;
                myPicking = picking;

                myCaptions = modules.GetPropertySetFor<Caption>();
                myNodeStyles = modules.GetPropertySetFor<NodeStyle>();
                myEdgeStyles = modules.GetPropertySetFor<EdgeStyle>();

                var algorithm = modules.GetModule<IGraphLayoutModule>().Algorithm;
                myGraphAttributes = modules.GetModule<IGraphAttributesModule>().ItemsFor(algorithm)
                    .ToDictionary(x => x.Name, x => x.Value);
            }

            public void Execute()
            {
                using (myWriter = new StreamWriter(myOwner.myPath))
                {
                    myWriter.WriteLine("digraph {");

                    var relevantNodes = myGraph.Nodes
                        .Where(myPicking.Pick)
                        .OrderByIf(n => n.Id, myOwner.PrettyPrint)
                        .ToList();

                    WriteGraphAttributes(myGraphAttributes);

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
                        .Where(myPicking.Pick)
                        .OrderByIf(e => e.Id, myOwner.PrettyPrint);

                    foreach (var edge in relevantEdges)
                    {
                        Write(edge, "  ");
                    }

                    myWriter.WriteLine("}");
                }
            }

            private void WriteGraphAttributes(Dictionary<string, string> attributes)
            {
                foreach(var attr in attributes)
                {
                    myWriter.WriteLine($"  {attr.Key.ToLower()}=\"{attr.Value}\"");
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
