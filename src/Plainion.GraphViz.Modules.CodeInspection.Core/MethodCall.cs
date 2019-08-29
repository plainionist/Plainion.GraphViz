using System;
using System.Diagnostics;
using Plainion;

namespace Plainion.GraphViz.Modules.CodeInspection.Core
{
    [DebuggerDisplay("{From} -> {To}")]
    public class MethodCall : IEquatable<MethodCall>
    {
        public MethodCall(Method from, Method to)
        {
            Contract.RequiresNotNull(from, "from");
            Contract.RequiresNotNull(to, "to");

            From = from;
            To = to;
        }

        public Method From { get; private set; }

        public Method To { get; private set; }

        public bool Equals(MethodCall other)
        {
            return From.Equals(other.From) && To.Equals(other.To);
        }

        public override bool Equals(object obj)
        {
            var other = obj as MethodCall;
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
                return hash;
            }
        }
    }
}