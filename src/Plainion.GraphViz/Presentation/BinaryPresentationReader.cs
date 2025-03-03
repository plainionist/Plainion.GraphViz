using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Media;
using Plainion.Graphs;
using Plainion.Graphs.Projections;

namespace Plainion.GraphViz.Presentation
{
    public class BinaryPresentationReader : IDisposable
    {
        private const int Version = 2;

        private BinaryReader myReader;
        private readonly BrushConverter myBrushConverter;

        /// <summary>
        /// The stream is left open.
        /// </summary>
        public BinaryPresentationReader(Stream stream)
        {
            Contract.RequiresNotNull(stream, nameof(stream));
            Contract.Requires(stream.CanRead, "Cannot read from stream");

            myReader = new BinaryReader(stream, Encoding.UTF8, true);
            myBrushConverter = new BrushConverter();
        }

        public void Dispose()
        {
            if (myReader != null)
            {
                myReader.Dispose();
                myReader = null;
            }
        }

        public IGraphPresentation Read()
        {
            var presentation = new GraphPresentation();

            var version = myReader.ReadInt32();
            Contract.Invariant(version == Version, "Unexpected version: " + version);

            presentation.Graph = ReadGraph(version);

            ReadNodeMasks(presentation.GetModule<NodeMaskModule>());
            ReadTansformations(presentation);
            ReadCaptions(presentation.GetModule<CaptionModule>());
            ReadNodeStyles(presentation.GetPropertySetFor<NodeStyle>());
            ReadEdgeStyles(presentation.GetPropertySetFor<EdgeStyle>());

            // intentionally left out
            // - GraphLayoutModule
            // - PropertySetModule<Selection>
            // - PropertySetModule<ToolTipContent>

            return presentation;
        }

        private IGraph ReadGraph(int version)
        {
            var builder = new RelaxedGraphBuilder();

            var count = myReader.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                var sourceId = myReader.ReadString();
                var targetId = myReader.ReadString();
                var weight = version >= 2 ? new int?(myReader.ReadInt32()) : null;

                builder.TryAddEdge(sourceId, targetId, weight);
            }

            count = myReader.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                var clusterId = myReader.ReadString();
                var nodeCount = myReader.ReadInt32();
                var nodes = new List<string>(nodeCount);
                for (int j = 0; j < nodeCount; ++j)
                {
                    nodes.Add(myReader.ReadString());
                }
                builder.TryAddCluster(clusterId, nodes);
            }

            count = myReader.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                builder.TryAddNode(myReader.ReadString());
            }

            return builder.Graph;
        }

        // for bookmarks
        internal void ReadNodeMasks(NodeMaskModule module)
        {
            var count = myReader.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                module.Push(ReadNodeMask());
            }
        }

        private INodeMask ReadNodeMask()
        {
            var label = myReader.ReadString();
            var isApplied = myReader.ReadBoolean();
            var isShowMask = myReader.ReadBoolean();

            var maskType = myReader.ReadString();

            if (maskType == "NodeMask")
            {
                var count = myReader.ReadInt32();
                var nodes = new List<string>(count);
                for (int i = 0; i < count; ++i)
                {
                    nodes.Add(myReader.ReadString());
                }

                var mask = new NodeMask(nodes);
                mask.Label = label;
                mask.IsApplied = isApplied;
                mask.IsShowMask = isShowMask;
                return mask;
            }
            else
            {
                throw new NotSupportedException("Unknown mask type: " + maskType);
            }
        }

        // for bookmarks
        internal void ReadTansformations(IGraphPresentation presentation)
        {
            var module = presentation.GetModule<TransformationModule>();

            // TransformationModule adds default transformations
            // -> remove those
            module.Clear();

            var count = myReader.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                module.Add(ReadTransformation(presentation));
            }
        }

        private IGraphTransformation ReadTransformation(IGraphPresentation presentation)
        {
            var transformationType = myReader.ReadString();

            if (transformationType == "DynamicClusterTransformation")
            {
                var t = new DynamicClusterTransformation();

                var count = myReader.ReadInt32();
                for (int i = 0; i < count; ++i)
                {
                    var id = myReader.ReadString();
                    if (myReader.ReadBoolean())
                    {
                        t.AddCluster(id);
                    }
                    else
                    {
                        t.HideCluster(id);
                    }
                }

                count = myReader.ReadInt32();
                for (int i = 0; i < count; ++i)
                {
                    var nodeId = myReader.ReadString();
                    var clusterId = myReader.ReadString();

                    if (clusterId == string.Empty)
                    {
                        t.RemoveFromClusters(nodeId);
                    }
                    else
                    {
                        t.AddToCluster(nodeId, clusterId);
                    }
                }

                return t;
            }
            else if (transformationType == "ClusterFoldingTransformation")
            {
                var t = new ClusterFoldingTransformation(presentation);

                var count = myReader.ReadInt32();
                for (int i = 0; i < count; ++i)
                {
                    t.Add(myReader.ReadString());
                }

                return t;
            }
            else
            {
                throw new NotSupportedException("Unknown transformation type: " + transformationType);
            }
        }

        private void ReadCaptions(CaptionModule module)
        {
            var count = myReader.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                var caption = new Caption(myReader.ReadString(), myReader.ReadString());
                caption.DisplayText = myReader.ReadString();
                module.Add(caption);
            }
        }

        private void ReadNodeStyles(IPropertySetModule<NodeStyle> module)
        {
            var count = myReader.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                var style = new NodeStyle(myReader.ReadString());
                style.BorderColor = (Brush)myBrushConverter.ConvertFromString(myReader.ReadString());
                style.FillColor = (Brush)myBrushConverter.ConvertFromString(myReader.ReadString());
                style.Shape = myReader.ReadString();
                style.Style = myReader.ReadString();
                module.Add(style);
            }
        }

        private void ReadEdgeStyles(IPropertySetModule<EdgeStyle> module)
        {
            var count = myReader.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                var style = new EdgeStyle(myReader.ReadString());
                style.Color = (Brush)myBrushConverter.ConvertFromString(myReader.ReadString());
                style.Style = myReader.ReadString();
                module.Add(style);
            }
        }
    }
}
