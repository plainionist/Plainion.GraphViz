using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    public class BinaryPresentationReader : IDisposable
    {
        internal static int Version = 1;

        private BinaryReader myReader;

        /// <summary>
        /// The stream is left open.
        /// </summary>
        public BinaryPresentationReader(Stream stream)
        {
            Contract.RequiresNotNull(stream, nameof(stream));
            Contract.Requires(stream.CanRead, "Cannot read from stream");

            myReader = new BinaryReader(stream, Encoding.UTF8, true);
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

            presentation.Graph = ReadGraph();

            ReadNodeMasks(presentation.GetModule<NodeMaskModule>());
            ReadEdgeMasks(presentation.GetModule<EdgeMaskModule>());
            ReadTansformations(presentation.GetModule<TransformationModule>());
            ReadCaptions(presentation.GetModule<CaptionModule>());
            ReadNodeStyles(presentation.GetPropertySetFor<NodeStyle>());
            ReadEdgeStyles(presentation.GetPropertySetFor<EdgeStyle>());
            ReadToolTips(presentation.GetPropertySetFor<ToolTipContent>());

            // intentionally left out
            // - GraphLayoutModule
            // - PropertySetModule<Selection>

            return presentation;
        }

        private IGraph ReadGraph()
        {
            var builder = new RelaxedGraphBuilder();

            var count = myReader.ReadInt32();
            for (int i = 0; i < count; ++i)
            {
                builder.TryAddEdge(myReader.ReadString(), myReader.ReadString());
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

        private void ReadNodeMasks(NodeMaskModule module)
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
            else if (maskType == "AllNodesMask")
            {
                var mask = new AllNodesMask();
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

        private void ReadEdgeMasks(EdgeMaskModule module)
        {
        }

        private void ReadTansformations(TransformationModule module)
        {
        }

        private void ReadCaptions(CaptionModule module)
        {
        }

        private void ReadNodeStyles(IPropertySetModule<NodeStyle> module)
        {
        }

        private void ReadEdgeStyles(IPropertySetModule<EdgeStyle> module)
        {
        }

        private void ReadToolTips(IPropertySetModule<ToolTipContent> module)
        {
        }
    }
}
