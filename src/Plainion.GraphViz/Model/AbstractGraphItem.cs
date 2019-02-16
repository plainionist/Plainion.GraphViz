using System;

namespace Plainion.GraphViz.Model
{
    [Serializable]
    public class AbstractGraphItem : IGraphItem
    {
        public AbstractGraphItem(string id)
        {
            Contract.RequiresNotNullNotEmpty(id, nameof(id));

            Id = id;
        }

        public string Id { get; private set; }

        public bool Equals(IGraphItem other)
        {
            if (other == null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (obj is IGraphItem other)
            {
                return Equals(other);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
