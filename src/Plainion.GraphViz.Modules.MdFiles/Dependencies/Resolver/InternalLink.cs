namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Resolver
{
    /// <summary>
    /// Contains the absolute path to an internal link.
    /// </summary>
    internal class InternalLink : ResolvedLink
    {
        public InternalLink(string url) : base(url)
        {
        }
    }
}