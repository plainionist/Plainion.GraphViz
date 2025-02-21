namespace Plainion.GraphViz.Modules.Markdown.Dependencies.Analyzer.Markdown
{
    internal record ImageLink : Link
    {
        public ImageLink(string url, string label) : base(url, label)
        {
        }
    }
}