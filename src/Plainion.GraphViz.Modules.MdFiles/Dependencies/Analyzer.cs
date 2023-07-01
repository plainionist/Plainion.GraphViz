using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Plainion.Collections;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Markdown;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Parser;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Resolver;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Verifier;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies
{
    internal class Analyzer
    {
        private readonly IMarkdownParser myMarkdownParser;
        private readonly ILinkResolver myLinkResolver;
        private readonly ILinkVerifier myLinkVerifier;

        public Analyzer(IMarkdownParser markdownParser,
            ILinkResolver linkResolver,
            ILinkVerifier linkVerifier)
        {
            myMarkdownParser = markdownParser;
            myLinkResolver = linkResolver;
            myLinkVerifier = linkVerifier;
        }

        public Task<AnalysisDocument> AnalyzeAsync(string folderToAnalyze, CancellationToken token) =>
            Task.Run(() => AnalyzeCore(folderToAnalyze), token);

        private AnalysisDocument AnalyzeCore(string folderToAnalyze)
        {
            var doc = new AnalysisDocument();

            if (!Directory.Exists(folderToAnalyze))
            {
                doc.FailedItems.Add(new FailedFile
                {
                    FullPath = folderToAnalyze,
                    Exception = new DirectoryNotFoundException()
                });

                return doc;
            }

            var results = Directory.GetFiles(folderToAnalyze, "*.md", SearchOption.AllDirectories)
                .Select(path => TryLoadFile(path, folderToAnalyze))
                .ToList();

            doc.Files.AddRange(results.OfType<MDFile>());
            doc.FailedItems.AddRange(results.OfType<FailedFile>());

            return doc;
        }

        private object TryLoadFile(string path, string root)
        {
            try
            {
                var currentDir = Path.GetDirectoryName(path);
                var mdDocument = myMarkdownParser.LoadFile(path);
                var docLinks = mdDocument.Links.OfType<DocLink>();

                var resolvedDocLinks = myLinkResolver.ResolveLinks(docLinks, currentDir, root);
                var internalDocLinks = resolvedDocLinks.OfType<InternalLink>();

                var verifiedLinks = myLinkVerifier.VerifyInternalLinks(internalDocLinks);
                var validMDReferences = FilterLinks<ValidLink>(verifiedLinks, ".md");
                var invalidMDReferences = FilterLinks<InvalidLink>(verifiedLinks, ".md");

                var filename = Path.GetFileNameWithoutExtension(path);

                return new MDFile
                {
                    Name = filename,
                    FullPath = path,
                    ValidMDReferences = validMDReferences,
                    InvalidMDReferences = invalidMDReferences
                };
            }
            catch (Exception ex)
            {
                return new FailedFile
                {
                    FullPath = path,
                    Exception = ex
                };
            }
        }

        private static IReadOnlyCollection<string> FilterLinks<T>(IEnumerable<VerifiedLink> links, string endsWith = "") 
            where T : VerifiedLink
        {
            return links
               .OfType<T>()
               .Where(l => l.Url.EndsWith(endsWith, StringComparison.OrdinalIgnoreCase))
               .Select(l => l.Url)
               .Distinct()
               .ToList();
        }
    }
}