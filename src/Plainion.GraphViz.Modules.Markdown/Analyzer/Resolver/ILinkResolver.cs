namespace Plainion.GraphViz.Modules.Markdown.Analyzer.Resolver
{
    internal interface ILinkResolver
    {
        /// <summary>
        /// Resolves if the passed url is an external link (website, network share or just outside the analyzed folder)
        /// or an internal link. in case of an internal link the absolute path will be resolved.
        /// </summary>
        /// <param name="url">Absolute or relative url.</param>
        /// <param name="file">The current processed file.</param>
        /// <param name="root">Origin folder to be analyzed.</param>
        /// <returns>The resolved link which is an absolute path.</returns>
        ResolvedLink ResolveLink(string url, string file, string root);
    }
}