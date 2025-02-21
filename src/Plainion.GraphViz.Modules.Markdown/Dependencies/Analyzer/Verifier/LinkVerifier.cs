using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace Plainion.GraphViz.Modules.Markdown.Dependencies.Analyzer.Verifier
{
    internal class LinkVerifier : ILinkVerifier
    {
        private readonly IFileSystem myFileSystem;

        public LinkVerifier(IFileSystem fileSystem)
        {
            myFileSystem = fileSystem;
        }

        public VerifiedLink VerifyLink(string link)
        {
            string path;

            try
            {
                path = RemoveAnchorLink(link);
            }
            catch (Exception ex)
            {
                return new InvalidLink(link, ex);
            }

            if (myFileSystem.File.Exists(path))
            {
                return new ValidLink(path);
            }

            var extension = myFileSystem.Path.GetExtension(path);

            // Links to markdown files are not supposed to have .md extension
            if (string.IsNullOrEmpty(extension) && myFileSystem.File.Exists($"{path}.md"))
            {
                return new ValidLink($"{path}.md");
            }

            return new InvalidLink(path, new FileNotFoundException());
        }

        private static string RemoveAnchorLink(string link)
        {
            var path = link.Split("#");

            if (path.Length > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(link), "More than one # not allowed in a link.");
            }

            return path.First();
        }
    }
}