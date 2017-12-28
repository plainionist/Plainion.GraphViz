using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    public class BinaryPresentationWriter : IDisposable
    {
        internal static int Version = 1;

        private BinaryWriter myWriter;

        /// <summary>
        /// The stream is left open.
        /// </summary>
        public BinaryPresentationWriter(Stream stream)
        {
            Contract.RequiresNotNull(stream, nameof(stream));
            Contract.Requires(stream.CanWrite, "Cannot write to stream");

            myWriter = new BinaryWriter(stream, Encoding.UTF8, true);
        }

        public void Dispose()
        {
            if (myWriter != null)
            {
                myWriter.Dispose();
                myWriter = null;
            }
        }

        public void Write(IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, nameof(presentation));

            myWriter.Write(Version);

            WriteGraph(presentation.Graph);
        }

        private void WriteGraph(IGraph graph)
        {
            var writtenNodes = new HashSet<string>();
            Action<Node> writeNode = new Action<Node>(n =>
            {
                myWriter.Write(n.Id);
                writtenNodes.Add(n.Id);
            });

            myWriter.Write(graph.Edges.Count());
            foreach (var edge in graph.Edges)
            {
                writeNode(edge.Source);
                writeNode(edge.Target);
            }

            myWriter.Write(graph.Clusters.Count());
            foreach (var cluster in graph.Clusters)
            {
                myWriter.Write(cluster.Id);
                myWriter.Write(cluster.Nodes.Count());
                foreach (var node in cluster.Nodes)
                {
                    writeNode(node);
                }
            }

            var remainingNodes = graph.Nodes
                .Where(n => !writtenNodes.Contains(n.Id))
                .ToList();

            myWriter.Write(remainingNodes.Count);
            foreach (var node in remainingNodes)
            {
                writeNode(node);
            }
        }
    }
}
