namespace Plainion.GraphViz.Modules.Markdown.Dependencies.Analyzer.Resolver
{
    internal record ExternalLink : ResolvedLink
    {
        public ExternalLink(string url) : base(url)
        {
        }
    }
}