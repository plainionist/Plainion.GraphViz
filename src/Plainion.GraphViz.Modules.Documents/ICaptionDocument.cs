using System.Collections.Generic;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Documents
{
    public interface ICaptionDocument
    {
        IEnumerable<Caption> Captions { get; }
    }
}
