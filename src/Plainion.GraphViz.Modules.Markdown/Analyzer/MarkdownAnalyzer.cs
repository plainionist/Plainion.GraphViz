﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Plainion;
using Plainion.Collections;
using Plainion.GraphViz.Modules.Markdown.Analyzer.Markdown;
using Plainion.GraphViz.Modules.Markdown.Analyzer.Parser;
using Plainion.GraphViz.Modules.Markdown.Analyzer.Resolver;
using Plainion.GraphViz.Modules.Markdown.Analyzer.Verifier;

namespace Plainion.GraphViz.Modules.Markdown.Analyzer
{
    internal class MarkdownAnalyzer
    {
        private readonly IFileSystem myFileSystem;
        private readonly IMarkdownParser myMarkdownParser;
        private readonly ILinkResolver myLinkResolver;
        private readonly ILinkVerifier myLinkVerifier;

        public MarkdownAnalyzer(IFileSystem fileSystem,
            IMarkdownParser markdownParser,
            ILinkResolver linkResolver,
            ILinkVerifier linkVerifier)
        {
            myFileSystem = fileSystem;
            myMarkdownParser = markdownParser;
            myLinkResolver = linkResolver;
            myLinkVerifier = linkVerifier;
        }

        public Task<AnalysisDocument> AnalyzeAsync(string folderToAnalyze, CancellationToken token) =>
            Task.Run(() => AnalyzeCore(folderToAnalyze), token);

        private AnalysisDocument AnalyzeCore(string folderToAnalyze)
        {
            var doc = new AnalysisDocument();

            if (!myFileSystem.Directory.Exists(folderToAnalyze))
            {
                doc.FailedItems.Add(new FailedFile
                {
                    FullPath = folderToAnalyze,
                    Exception = new DirectoryNotFoundException()
                });

                return doc;
            }

            var results = myFileSystem.Directory.GetFiles(folderToAnalyze, "*.md", SearchOption.AllDirectories)
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
                var mdDocument = myMarkdownParser.LoadFile(path);
                var docLinks = mdDocument.Links.OfType<DocLink>();

                var resolvedDocLinks = myLinkResolver.ResolveLinks(docLinks, path, root);
                var mdLinks = FilterLinks(resolvedDocLinks, ".md", "");

                var verifiedInternalLinks = myLinkVerifier.VerifyInternalLinks(mdLinks);
                var validInternalMDRefs = GetDistinctLinks<ValidLink>(verifiedInternalLinks);
                var invalidInternalMDRefs = GetDistinctLinks<InvalidLink>(verifiedInternalLinks);

                var verifiedExternalLinks = myLinkVerifier.VerifyExternalLinks(mdLinks);
                var validExternalMDRefs = GetDistinctLinks<ValidLink>(verifiedExternalLinks);
                var invalidExternalMDRefs = GetDistinctLinks<InvalidLink>(verifiedExternalLinks);

                var filename = Path.GetFileNameWithoutExtension(path);

                return new MDFile(
                    name: filename,
                    fullPath: path,
                    validInternalMDRefs: validInternalMDRefs,
                    invalidInternalMDRefs: invalidInternalMDRefs,
                    validExternalMDRefs: validExternalMDRefs,
                    invalidExternalMDRefs: invalidExternalMDRefs
                );
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

        private static IReadOnlyCollection<T> FilterLinks<T>(IEnumerable<T> links, params string[] fileExtensions)
            where T : ResolvedLink
        {
            return links
               .OfType<T>()
               .Where(l => l.Uri.Scheme == Uri.UriSchemeFile && MatchExtension(l.Uri.LocalPath, fileExtensions))
               .ToList();
        }

        private static bool MatchExtension(string url, string[] fileExtensions)
        {
            if (!fileExtensions.Any())
            {
                return true;
            }

            var fileExtenstion = Path.GetExtension(url);

            if (string.IsNullOrEmpty(fileExtenstion) && fileExtensions.Contains(fileExtenstion))
            {
                return true;
            }

            return fileExtensions.Where(f => !string.IsNullOrEmpty(f)).Any(f => fileExtenstion.StartsWith(f));
        }

        private static IReadOnlyCollection<string> GetDistinctLinks<T>(IEnumerable<VerifiedLink> links)
          where T : VerifiedLink
        {
            return links
               .OfType<T>()
               .Select(l => l.Path)
               .Distinct()
               .ToList();
        }
    }
}