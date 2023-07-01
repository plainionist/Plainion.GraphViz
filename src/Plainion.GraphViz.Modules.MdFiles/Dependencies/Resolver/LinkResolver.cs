using System;
using System.IO;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Resolver
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
                || url.StartsWith("\\"); // Workaround: UNC paths are not supported by MD
        }

        private static ResolvedLink Resolve(string url, string currentDir, string root)
        {
            var path = Path.GetFullPath(url, currentDir);

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