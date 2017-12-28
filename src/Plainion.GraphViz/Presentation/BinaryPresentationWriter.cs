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

            WriteNodeMasks(presentation.GetModule<NodeMaskModule>());
            WriteEdgeMasks(presentation.GetModule<EdgeMaskModule>());
            WriteTansformations(presentation.GetModule<TransformationModule>());
            WriteCaptions(presentation.GetModule<CaptionModule>());
            WriteNodeStyles(presentation.GetPropertySetFor<NodeStyle>());
            WriteEdgeStyles(presentation.GetPropertySetFor<EdgeStyle>());
            WriteToolTips(presentation.GetPropertySetFor<ToolTipContent>());

            // intentionally left out
            // - GraphLayoutModule
            // - PropertySetModule<Selection>
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

        private void WriteNodeMasks(NodeMaskModule module)
        {
            myWriter.Write(module.Items.Count());
            foreach (var mask in module.Items.Reverse())
            {
                WriteNodeMask(mask);
            }
        }

        private void WriteNodeMask(INodeMask mask)
        {
            myWriter.Write(mask.Label ?? string.Empty);
            myWriter.Write(mask.IsApplied);
            myWriter.Write(mask.IsShowMask);

            var nodeMask = mask as NodeMask;
            if (nodeMask != null)
            {
                myWriter.Write("NodeMask");
                myWriter.Write(nodeMask.Values.Count());
                foreach (var value in nodeMask.Values)
                {
                    myWriter.Write(value);
                }
            }
            else if (mask is AllNodesMask)
            {
                myWriter.Write("AllNodesMask");
            }
            else
            {
                throw new NotSupportedException("Unknown mask type: " + mask.GetType());
            }
        }

        private void WriteEdgeMasks(EdgeMaskModule module)
        {
        }

        private void WriteTansformations(TransformationModule module)
        {
        }

        private void WriteCaptions(CaptionModule module)
        {
        }

        private void WriteNodeStyles(IPropertySetModule<NodeStyle> module)
        {
        }

        private void WriteEdgeStyles(IPropertySetModule<EdgeStyle> module)
        {
        }

        private void WriteToolTips(IPropertySetModule<ToolTipContent> module)
        {
        }
    }
}
