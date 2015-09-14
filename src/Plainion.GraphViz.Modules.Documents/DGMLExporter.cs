using System;
using System.IO;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Modules.Documents
{
    class DgmlExporter
    {
        public static void Export(IGraph graph, Func<Node, string> GetNodeLabel, TextWriter writer)
        {
            Contract.RequiresNotNull(graph, "graph");
            Contract.RequiresNotNull(writer, "writer");

            writer.WriteLine("<DirectedGraph xmlns=\"http://schemas.microsoft.com/vs/2009/dgml\">");

            writer.WriteLine("  <Nodes>");
            foreach (var node in graph.Nodes)
            {
                writer.WriteLine("    <Node Id=\"{0}\" Label=\"{1}\" />", node.Id, GetNodeLabel(node));
            }
            writer.WriteLine("  </Nodes>");

            writer.WriteLine("  <Links>");
            foreach (var edge in graph.Edges)
            {
                writer.WriteLine("    <Link Source=\"{0}\" Target=\"{1}\" />", edge.Source.Id, edge.Target.Id);
            }
            writer.WriteLine("  </Links>");

            writer.WriteLine("</DirectedGraph>");
        }
    }
}
