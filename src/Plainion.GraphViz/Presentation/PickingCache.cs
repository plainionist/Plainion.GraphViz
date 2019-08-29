using System;
using System.Collections.Generic;
using System.Diagnostics;
using Plainion.GraphViz.Model;
using Plainion;

namespace Plainion.GraphViz.Presentation
{
    class PickingCache : IGraphPicking
    {
        private IGraphPresentation myPresentation;
        private IGraphPicking myPicking;
        private Dictionary<string, bool> myCache;

        public PickingCache(IGraphPresentation presentation, IGraphPicking realPicking)
        {
            Contract.RequiresNotNull(presentation, "presentation");
            Contract.RequiresNotNull(realPicking, "realPicking");

            myPresentation = presentation;
            myPicking = realPicking;

            myCache = new Dictionary<string, bool>();

            myPresentation.GraphVisibilityChanged += OnGraphVisibilityChanged;
        }

        private void OnGraphVisibilityChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("Picking cache cleared");
            myCache.Clear();
        }

        public bool Pick(Node node)
        {
            if (!myCache.ContainsKey(node.Id))
            {
                myCache[node.Id] = myPicking.Pick(node);
            }

            return myCache[node.Id];
        }

        public bool Pick(Edge edge)
        {
            if (!myCache.ContainsKey(edge.Id))
            {
                myCache[edge.Id] = myPicking.Pick(edge);
            }

            return myCache[edge.Id];
        }

        public bool Pick(Cluster cluster)
        {
            if (!myCache.ContainsKey(cluster.Id))
            {
                myCache[cluster.Id] = myPicking.Pick(cluster);
            }

            return myCache[cluster.Id];
        }
    }
}
