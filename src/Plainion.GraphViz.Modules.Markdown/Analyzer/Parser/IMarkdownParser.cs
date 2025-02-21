using Plainion.GraphViz.Modules.Markdown.Analyzer.Markdown;

namespace Plainion.GraphViz.Modules.Markdown.Analyzer.Parser
{
    internal interface IMarkdownParser
    {
        MDDocument LoadFile(string file);

        MDDocument LoadMarkdown(string markdown);
    }
}