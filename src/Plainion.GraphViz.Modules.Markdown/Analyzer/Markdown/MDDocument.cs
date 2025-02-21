using System.Collections.Generic;

namespace Plainion.GraphViz.Modules.Markdown.Analyzer.Markdown
{
    internal class MDDocument
    {
        public MDDocument(IReadOnlyCollection<Link> links)
        {
            Contract.RequiresNotNull(links);

            Links = links;
        }

        public IReadOnlyCollection<Link> Links { get; }
    }
}