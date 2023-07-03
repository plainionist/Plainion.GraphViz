namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Resolver
{
    internal abstract record ResolvedLink
    {
        protected ResolvedLink(string url)
        {
            Contract.RequiresNotNullNotEmpty(url);

            Url = url;
        }

        public string Url { get; }
    }
}