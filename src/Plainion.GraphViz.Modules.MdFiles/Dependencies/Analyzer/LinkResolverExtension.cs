using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Markdown;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Resolver;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer
{
    internal static class LinkResolverExtension
    {
        public static IReadOnlyCollection<ResolvedLink> ResolveLinks(this ILinkResolver self, IEnumerable<Link> links,
            string file, string root)
        {
            return links.Select(l => self.ResolveLink(l.Url, file, root)).ToList();
        }
    }
}