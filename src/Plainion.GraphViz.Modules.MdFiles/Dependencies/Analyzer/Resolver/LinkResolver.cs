using System;
using System.IO;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Resolver
{
    internal class LinkResolver : ILinkResolver
    {
        public ResolvedLink ResolveLink(string url, string currentDir, string root)
        {
            if (IsExternalLink(url))
            {
                return new ExternalLink(url);
            }

            return Resolve(url, currentDir, root);
        }

        private static bool IsExternalLink(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _)
                || IsUncPath(url);
        }

        private static bool IsUncPath(string url)
        {
            // Workaround: UNC paths are not supported by MD and are not parsed correctly (doubles backslashes are not escaped).
            // However, since they might appear in the document, we want to correctly specify them as an external link.
            return url.StartsWith("\\");
        }

        private static ResolvedLink Resolve(string url, string currentDir, string root)
        {
            // Remove leading slash of the url otherwise all subdirectories of the currentDir are lost in the new path,
            // e.g. ("/Folder C/document.md", "C:\Folder A\Folder B\") becomes to "C:\Folder C\document.md".
            var path = Path.GetFullPath(url.TrimStart('/'), currentDir);

            if (IsOutsideRoot(path, root))
            {
                return new ExternalLink(path);
            }

            return new InternalLink(path);
        }

        private static bool IsOutsideRoot(string path, string root)
        {
            return !path.StartsWith(root);
        }
    }
}