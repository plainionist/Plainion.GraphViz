using System.Collections.Generic;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Documents
{
    public interface IStyleDocument
    {
        IEnumerable<NodeStyle> NodeStyles { get; }
        IEnumerable<EdgeStyle> EdgeStyles { get; }
    }
}
