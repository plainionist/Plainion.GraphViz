namespace Plainion.GraphViz.Modules.Markdown.Analyzer.Markdown
{
    internal record ImageLink : Link
    {
        public ImageLink(string url, string label) : base(url, label)
        {
        }
    }
}