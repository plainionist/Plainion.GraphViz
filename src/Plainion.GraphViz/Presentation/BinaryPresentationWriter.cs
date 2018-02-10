using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    public class BinaryPresentationWriter : IDisposable
    {
        internal static int Version = 1;

        private BinaryWriter myWriter;
        private BrushConverter myBrushConverter;

        /// <summary>
        /// The stream is left open.
        /// </summary>
        public BinaryPresentationWriter(Stream stream)
        {
            Contract.RequiresNotNull(stream, nameof(stream));
            Contract.Requires(stream.CanWrite, "Cannot write to stream");

            myWriter = new BinaryWriter(stream, Encoding.UTF8, true);
            myBrushConverter = new BrushConverter();
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
            WriteTansformations(presentation.GetModule<TransformationModule>());
            WriteCaptions(presentation.GetModule<CaptionModule>());
            WriteNodeStyles(presentation.GetPropertySetFor<NodeStyle>());
            WriteEdgeStyles(presentation.GetPropertySetFor<EdgeStyle>());

            // intentionally left out
            // - GraphLayoutModule
            // - PropertySetModule<Selection>
            // - PropertySetModule<ToolTipContent>
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

        // for bookmarks
        internal void WriteNodeMasks(NodeMaskModule module)
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

            if (mask is NodeMask nodeMask)
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

        // for bookmarks
        internal void WriteTansformations(TransformationModule module)
        {
            myWriter.Write(module.Items.Count());
            foreach (var transformation in module.Items)
            {
                WriteTransformation(transformation);
            }
        }

        private void WriteTransformation(IGraphTransformation transformation)
        {
            if (transformation is DynamicClusterTransformation dct)
            {
                myWriter.Write("DynamicClusterTransformation");

                myWriter.Write(dct.ClusterVisibility.Count());
                foreach (var entry in dct.ClusterVisibility)
                {
                    myWriter.Write(entry.Key);
                    myWriter.Write(entry.Value);
                }

                myWriter.Write(dct.NodeToClusterMapping.Count());
                foreach (var entry in dct.NodeToClusterMapping)
                {
                    myWriter.Write(entry.Key);
                    myWriter.Write(entry.Value ?? string.Empty);
                }
            }
            else if (transformation is ClusterFoldingTransformation cft)
            {
                myWriter.Write("ClusterFoldingTransformation");

                myWriter.Write(cft.Clusters.Count());
                foreach (var cluster in cft.Clusters)
                {
                    myWriter.Write(cluster);
                }
            }
            else
            {
                throw new NotSupportedException("Unknown transformation type: " + transformation.GetType());
            }
        }

        private void WriteCaptions(CaptionModule module)
        {
            myWriter.Write(module.Items.Count());
            foreach (var caption in module.Items)
            {
                myWriter.Write(caption.OwnerId);
                myWriter.Write(caption.Label);
                myWriter.Write(caption.DisplayText);
            }
        }

        private void WriteNodeStyles(IPropertySetModule<NodeStyle> module)
        {
            myWriter.Write(module.Items.Count());
            foreach (var style in module.Items)
            {
                myWriter.Write(style.OwnerId);
                myWriter.Write(myBrushConverter.ConvertToString(style.BorderColor));
                myWriter.Write(myBrushConverter.ConvertToString(style.FillColor));
                myWriter.Write(style.Shape);
                myWriter.Write(style.Style);
            }
        }

        private void WriteEdgeStyles(IPropertySetModule<EdgeStyle> module)
        {
            myWriter.Write(module.Items.Count());
            foreach (var style in module.Items)
            {
                myWriter.Write(style.OwnerId);
                myWriter.Write(myBrushConverter.ConvertToString(style.Color));
                myWriter.Write(style.Style);
            }
        }
    }
}
