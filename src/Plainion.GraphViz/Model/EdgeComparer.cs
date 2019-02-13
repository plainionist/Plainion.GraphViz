using System.Collections.Generic;

namespace Plainion.GraphViz.Model
{
    internal class EdgeComparer : IEqualityComparer<Edge>
    {
        public bool Equals(Edge x, Edge y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Edge obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
