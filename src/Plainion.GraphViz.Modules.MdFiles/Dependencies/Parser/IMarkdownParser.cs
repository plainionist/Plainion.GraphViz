using Plainion.GraphViz.Modules.MdFiles.Dependencies.Markdown;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Parser
{
    internal interface IMarkdownParser
    {
        MDDocument LoadFile(string file);

        MDDocument LoadMarkdown(string markdown);
    }
}