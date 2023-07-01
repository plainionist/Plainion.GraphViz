using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Resolver;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Verifier;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies
{
    internal static class LinkVerifierExtensions
    {
        public static IReadOnlyCollection<VerifiedLink> VerifyInternalLinks(this ILinkVerifier self, IEnumerable<InternalLink> links)
        {
            return links
                .Select(l => self.VerifyInternalLink(l.Url))
                .ToList();
        }
    }
}