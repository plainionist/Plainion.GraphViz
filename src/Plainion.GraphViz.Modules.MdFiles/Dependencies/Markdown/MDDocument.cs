using System.Collections.Generic;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Markdown
{
    internal class MDDocument
    {
        public MDDocument(IReadOnlyCollection<Link> links)
        {
            Links = links;
        }

        public IReadOnlyCollection<Link> Links { get; }
    }
}