using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Resolver;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Verifier;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer
{
    internal static class LinkVerifierExtensions
    {
        public static IReadOnlyCollection<VerifiedLink> VerifyInternalLinks(this ILinkVerifier self, IEnumerable<ResolvedLink> links)
        {
            return links
                .OfType<InternalLink>()
                .Select(l => self.VerifyLink(l.Uri.LocalPath))
                .ToList();
        }

        public static IEnumerable<VerifiedLink> VerifyExternalLinks(this ILinkVerifier self, IEnumerable<ResolvedLink> links)
        {
            return links
                .OfType<ExternalLink>()
                .Select(l => self.VerifyLink(l.Uri.LocalPath))
                .ToList();
        }
    }
}