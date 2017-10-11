using System;

namespace Plainion.GraphViz.Modules.CodeInspection.Core
{
    public class Edge
    {
        public Type Source { get; set; }

        public Type Target { get; set; }

        public EdgeType EdgeType { get; set; }
    }
}
