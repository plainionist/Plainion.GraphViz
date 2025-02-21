namespace Plainion.GraphViz.Modules.Markdown.Analyzer.Resolver
{
    internal record ExternalLink : ResolvedLink
    {
        public ExternalLink(string url) : base(url)
        {
        }
    }
}