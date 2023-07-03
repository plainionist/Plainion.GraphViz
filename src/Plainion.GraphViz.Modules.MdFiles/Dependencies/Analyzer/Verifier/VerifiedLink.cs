namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Verifier
{
    internal abstract record VerifiedLink
    {
        protected VerifiedLink(string url)
        {
            Contract.RequiresNotNullNotEmpty(url);

            Url = url;
        }

        public string Url { get; }
    }
}