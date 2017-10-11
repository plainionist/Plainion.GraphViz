using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Plainion.GraphViz.Modules.CodeInspection.Core;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    static class GraphUtils
    {
        public static Edge Edge( Edge edge )
        {
            return Edge( edge.Source, edge.Target, edge.EdgeType );
        }

        public static Edge Edge( Type source, Type target )
        {
            return Edge( source, target, EdgeType.Undefined );
        }

        public static Edge Edge( Type source, Type target, EdgeType edgeType )
        {
            return new Edge
            {
                Source = Node( source ),
                Target = Node( target ),
                EdgeType = edgeType
            };
        }

        public static Type Node( Type type )
        {
            var nodeType = type;

            if( type.GetCustomAttribute( typeof( CompilerGeneratedAttribute ), true ) != null )
            {
                nodeType = type.DeclaringType;
            }

            if( nodeType == null )
            {
                // e.g. code generated from Xml like ResourceManager
                nodeType = type;
            }

            return nodeType;
        }
    }
}
