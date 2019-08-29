using System;
using System.Diagnostics;

namespace Plainion.GraphViz.Modules.CodeInspection.Core
{
    [DebuggerDisplay("{DeclaringType.Name}.{Name}")]
    public class Method : IEquatable<Method>
    {
        public Method(Type declaringType, string name)
        {
            Contract.RequiresNotNull(declaringType, "declaringType");
            Contract.RequiresNotNullNotEmpty(name, "name");

            DeclaringType = declaringType;
            Name = name;
        }

        public Type DeclaringType { get; private set; }

        public string Name { get; private set; }

        public bool Equals(Method other)
        {
            return DeclaringType.Equals(other.DeclaringType) && Name.Equals(other.Name);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Method;
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
                hash = hash * 23 + DeclaringType.GetHashCode();
                hash = hash * 23 + Name.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}", DeclaringType.FullName, Name);
        }
    }
}