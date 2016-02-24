using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Plainion.GraphViz.Pioneer.Services
{
    class GraphUtils
    {
        public static Tuple<Type, Type> Edge(Type source, Type target)
        {
            return new Tuple<Type, Type>(Node(source), Node(target));
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

            return nodeType;
        }
    }
}
