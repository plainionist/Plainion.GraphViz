using System;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    class Edge
    {
        public Type Source { get; set; }

        public Type Target { get; set; }

        public EdgeType EdgeType { get; set; }
    }
}
