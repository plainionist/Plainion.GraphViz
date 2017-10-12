using System;

namespace Plainion.GraphViz.Modules.CodeInspection.Core
{
    public class Reference
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
            :this(to, ReferenceType.Undefined)
        {
        }

        public Type From { get; internal set; }

        public Type To { get; private set; }

        public ReferenceType ReferenceType { get; private set; }
    }
}
