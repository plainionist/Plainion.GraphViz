using System;
using System.Reflection;
using System.Runtime.CompilerServices;
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
            return new Reference(Node(source), Node(target), edgeType);
        }

        public static Type Node(Type type)
        {
            var nodeType = type;

            if (type.GetCustomAttribute(typeof(CompilerGeneratedAttribute), true) != null)
            {
                nodeType = type.DeclaringType;
            }

            if (nodeType == null)
            {
                // e.g. code generated from Xml like ResourceManager
                nodeType = type;
            }

            if (nodeType == null)
            {
                throw new InvalidOperationException($"Failed to determine node type for: {type}");
            }

            return nodeType;
        }
    }
}
