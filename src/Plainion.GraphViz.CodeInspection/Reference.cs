using System;
using System.Diagnostics;

namespace Plainion.GraphViz.CodeInspection
{
    [DebuggerDisplay("{From.FullName} -> {To.FullName}")]
    public class Reference : IEquatable<Reference>
    {
        public Reference(Type from, Type to, ReferenceType type)
        {
            Contract.RequiresNotNull(from, "from");
            Contract.RequiresNotNull(to, "to");
            Contract.RequiresNotNull(type, "type");

            From = from;
            To = to;
            ReferenceType = type;
        }

        public Type From { get; private set; }

        public Type To { get; private set; }

        public ReferenceType ReferenceType { get; private set; }

        public bool Equals(Reference other)
        {
            return From.Equals(other.From) &&
                To.Equals(other.To) &&
                ReferenceType == other.ReferenceType;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Reference;
            if (other == null)
            {
                return false;
            }

            return Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + From.GetHashCode();
                hash = hash * 23 + To.GetHashCode();
                hash = hash * 23 + ReferenceType.GetHashCode();
                return hash;
            }
        }
    }
}
