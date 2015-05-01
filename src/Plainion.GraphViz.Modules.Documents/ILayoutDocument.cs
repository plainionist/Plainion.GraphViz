using System.Collections.Generic;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Documents
{
    public interface ILayoutDocument
    {
        IEnumerable<NodeLayout> NodeLayouts { get; }
        IEnumerable<EdgeLayout> EdgeLayouts { get; }
    }
}
