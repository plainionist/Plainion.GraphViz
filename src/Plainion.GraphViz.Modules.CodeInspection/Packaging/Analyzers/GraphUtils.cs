using System;
using Plainion.GraphViz.Modules.CodeInspection.Core;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Analyzers
{
    static class GraphUtils
    {
        public static Reference Edge(Reference edge)
        {
            return Edge(edge.From, edge.To, edge.ReferenceType);
        }

        public static Reference Edge(Type source, Type target)
        {
            return Edge(source, target, ReferenceType.Undefined);
        }

        public static Reference Edge(Type source, Type target, ReferenceType edgeType)
        {
            return new Reference(source, target, edgeType);
        }
    }
}
