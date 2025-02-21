using System;
using System.IO;

namespace Plainion.GraphViz.Modules.Markdown.Analyzer.Resolver
{
    internal class LinkResolver : ILinkResolver
    {
        public ResolvedLink ResolveLink(string url, string file, string root)
        {
            if (IsExternalLink(url))
            {
                return new ExternalLink(url);
            }
            else if (IsUncPath(url))
            {
                // Add backslash that went missing during parsing the URL.
                return new ExternalLink("\\" + url);
            }

            return Resolve(url, file, root);
        }

        private static bool IsExternalLink(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        private static bool IsUncPath(string url)
        {
            // Workaround: UNC paths are not supported by MD and are not parsed correctly (doubles backslashes are not escaped).
            // However, since they might appear in the document, we want to correctly specify them as an external link.
            return url.StartsWith("\\") && Uri.TryCreate("\\" + url, UriKind.Absolute, out var uncPath) && uncPath.IsUnc;
        }

        private static ResolvedLink Resolve(string url, string file, string root)
        {
            var path = GetFullPath(url, file);

            if (IsOutsideRoot(path, root))
            {
                return new ExternalLink(path);
            }

            return new InternalLink(path);
        }

        private static string GetFullPath(string url, string file)
        {
            if (IsAnchorLink(url))
            {
                return $"{file}{url}";
            }

            var currentDir = Path.GetDirectoryName(file);

            // Remove leading slash of the url otherwise all subdirectories of the currentDir are lost in the new path,
            // e.g. ("/Folder C/document.md", "C:\Folder A\Folder B\") becomes to "C:\Folder C\document.md".
            return Path.GetFullPath(url.TrimStart('/'), currentDir);
        }

        private static bool IsAnchorLink(string url)
        {
            return url.StartsWith("#");
        }

        private static bool IsOutsideRoot(string path, string root)
        {
            return !path.StartsWith(root);
        }
    }
}