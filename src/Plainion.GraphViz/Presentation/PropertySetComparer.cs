using System.Collections.Generic;

namespace Plainion.GraphViz.Presentation
{
    class PropertySetComparer : IEqualityComparer<AbstractPropertySet>
    {
        public bool Equals( AbstractPropertySet x, AbstractPropertySet y )
        {
            return x.OwnerId == y.OwnerId;
        }

        public int GetHashCode( AbstractPropertySet obj )
        {
            return obj.OwnerId.GetHashCode();
        }
    }
}
