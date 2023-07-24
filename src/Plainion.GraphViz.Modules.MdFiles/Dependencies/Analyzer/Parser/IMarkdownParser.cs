using Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Markdown;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Parser
{
    internal interface IMarkdownParser
    {
        MDDocument LoadFile(string file);

        MDDocument LoadMarkdown(string markdown);
    }
}