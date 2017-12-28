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
            Contract.Requires(stream.CanWrite, "Cannot read from stream");

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
    }
}
