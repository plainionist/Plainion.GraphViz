using Plainion.GraphViz.Modules.Markdown.Dependencies.Analyzer.Markdown;

namespace Plainion.GraphViz.Modules.Markdown.Dependencies.Analyzer.Parser
{
    internal interface IMarkdownParser
    {
        MDDocument LoadFile(string file);

        MDDocument LoadMarkdown(string markdown);
    }
}