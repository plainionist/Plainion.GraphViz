namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Resolver
{
    internal record ExternalLink : ResolvedLink
    {
        public ExternalLink(string url) : base(url)
        {
        }
    }
}