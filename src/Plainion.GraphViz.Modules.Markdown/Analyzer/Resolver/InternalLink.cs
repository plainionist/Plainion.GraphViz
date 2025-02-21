namespace Plainion.GraphViz.Modules.Markdown.Analyzer.Resolver
{
    /// <summary>
    /// Contains the absolute path to an internal link.
    /// </summary>
    internal record InternalLink : ResolvedLink
    {
        public InternalLink(string url) : base(url)
        {
        }
    }
}