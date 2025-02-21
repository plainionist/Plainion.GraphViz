namespace Plainion.GraphViz.Modules.Markdown.Analyzer.Markdown
{
    internal record DocLink : Link
    {
        public DocLink(string url, string label) : base(url, label)
        {
        }
    }
}