using System.IO;
using System.IO.Abstractions;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Verifier
{
    internal class LinkVerifier : ILinkVerifier
    {
        private readonly IFileSystem myFileSystem;

        public LinkVerifier(IFileSystem fileSystem)
        {
            myFileSystem = fileSystem;
        }

        public VerifiedLink VerifyInternalLink(string url)
        {
            var extension = myFileSystem.Path.GetExtension(url);

            if (!myFileSystem.File.Exists(url))
            {
                // Links to markdown files are not supposed to have .md extension
                if (string.IsNullOrEmpty(extension) && myFileSystem.File.Exists($"{url}.md"))
                {
                    return new ValidLink($"{url}.md");
                }

                return new InvalidLink(url, new FileNotFoundException());
            }

            return new ValidLink(url);
        }
    }
}