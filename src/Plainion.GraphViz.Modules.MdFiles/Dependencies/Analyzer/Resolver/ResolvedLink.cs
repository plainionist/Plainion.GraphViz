using System;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Resolver
{
    internal abstract record ResolvedLink
    {
        protected ResolvedLink(string url)
        {
            Contract.RequiresNotNullNotEmpty(url);

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException($"Url '{url}' is not an absolute uri.");
            }

            Url = uri;
        }

        public Uri Url { get; }
    }
}