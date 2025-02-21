using System.IO.Abstractions;
using System.Linq;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Plainion.GraphViz.Modules.Markdown.Dependencies.Analyzer.Markdown;

namespace Plainion.GraphViz.Modules.Markdown.Dependencies.Analyzer.Parser
{
    internal class MarkdigParser : AbstractParser
    {
        public MarkdigParser(IFileSystem fileSystem) : base(fileSystem)
        {
        }

        protected override MDDocument ParseMarkdown(string markdown)
        {
            var doc = Markdig.Markdown.Parse(markdown);
            var inlineLinks = doc.Descendants<ParagraphBlock>().SelectMany(x => x.Inline.Descendants<LinkInline>());

            var links = inlineLinks
                .Where(l => !string.IsNullOrEmpty(l.Url))
                .Select(CreateLink)
                .ToList();

            return new MDDocument(
                links: links);
        }

        private Link CreateLink(LinkInline link)
        {
            if (link.IsImage)
            {
                return new ImageLink(link.Url, link.Label);
            }

            return new DocLink(link.Url, link.Label);
        }
    }
}