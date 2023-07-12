using System.Collections.Generic;
using System.Linq;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Resolver;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Verifier;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer
{
    internal static class LinkVerifierExtensions
    {
        public static IReadOnlyCollection<VerifiedLink> VerifyInternalLinks(this ILinkVerifier self, IEnumerable<InternalLink> links)
        {
            return links
                .Select(l => self.VerifyLink(l.Uri.LocalPath))
                .ToList();
        }
    }
}