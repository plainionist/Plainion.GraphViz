using System.IO.Abstractions;
using Plainion.GraphViz.Modules.Markdown.Dependencies.Analyzer.Markdown;

namespace Plainion.GraphViz.Modules.Markdown.Dependencies.Analyzer.Parser
{
    internal abstract class AbstractParser : IMarkdownParser
    {
        private readonly IFileSystem myFileSystem;

        protected AbstractParser(IFileSystem fileSystem)
        {
            myFileSystem = fileSystem;
        }

        public MDDocument LoadFile(string file)
        {
            var markdown = myFileSystem.File.ReadAllText(file);

            return ParseMarkdown(markdown);
        }

        public MDDocument LoadMarkdown(string markdown)
        {
            return ParseMarkdown(markdown);
        }

        protected abstract MDDocument ParseMarkdown(string markdown);
    }
}