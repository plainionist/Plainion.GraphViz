namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Resolver
{
    internal abstract class ResolvedLink
    {
        protected ResolvedLink(string url)
        {
            Url = url;
        }

        public string Url { get; }
    }
}