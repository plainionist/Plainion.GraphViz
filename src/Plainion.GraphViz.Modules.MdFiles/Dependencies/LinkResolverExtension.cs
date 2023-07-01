using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Markdown;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Resolver;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies
{
    internal static class LinkResolverExtension
    {
        public static IReadOnlyCollection<ResolvedLink> ResolveLinks(this ILinkResolver self, IEnumerable<Link> links,
            string currentDir, string root)
        {
            return links.Select(l => self.ResolveLink(l.Url, currentDir, root)).ToList();
        }
    }
}