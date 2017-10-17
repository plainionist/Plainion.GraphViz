using System;

namespace Plainion.GraphViz.Modules.CodeInspection.Core
{
    public class Reference : IEquatable<Reference>
    {
        public Reference(Type from, Type to, ReferenceType type)
        {
            From = from;
            To = to;
            ReferenceType = type;
        }

        internal Reference(Type to, ReferenceType type)
        {
            To = to;
            ReferenceType = type;
        }

        internal Reference(Type to)
            : this(to, ReferenceType.Undefined)
        {
        }

        public Type From { get; internal set; }

        public Type To { get; private set; }

        public ReferenceType ReferenceType { get; private set; }

        public bool Equals(Reference other)
        {
            return ((From == null && other.From == null) || From.Equals(other.From)) &&
                To.Equals(other.To) && ReferenceType == other.ReferenceType;
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
                if (From != null)
                {
                    hash = hash * 23 + From.GetHashCode();
                }
                hash = hash * 23 + To.GetHashCode();
                hash = hash * 23 + ReferenceType.GetHashCode();
                return hash;
            }
        }
    }
}
