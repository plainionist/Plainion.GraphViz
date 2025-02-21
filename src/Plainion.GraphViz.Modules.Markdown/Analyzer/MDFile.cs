using System.Collections.Generic;

namespace Plainion.GraphViz.Modules.Markdown.Analyzer
{
    internal class MDFile
    {
        public MDFile(string name, string fullPath,
            IReadOnlyCollection<string> validInternalMDRefs,
            IReadOnlyCollection<string> invalidInternalMDRefs,
            IReadOnlyCollection<string> validExternalMDRefs,
            IReadOnlyCollection<string> invalidExternalMDRefs)
        {
            Contract.RequiresNotNullNotEmpty(name);
            Contract.RequiresNotNullNotEmpty(fullPath);
            Contract.RequiresNotNull(validInternalMDRefs);
            Contract.RequiresNoDuplicates(validInternalMDRefs);
            Contract.RequiresNotNull(invalidInternalMDRefs);
            Contract.RequiresNoDuplicates(invalidInternalMDRefs);
            Contract.RequiresNotNull(validExternalMDRefs);
            Contract.RequiresNoDuplicates(validExternalMDRefs);
            Contract.RequiresNotNull(invalidExternalMDRefs);
            Contract.RequiresNoDuplicates(invalidExternalMDRefs);

            Name = name;
            FullPath = fullPath;
            ValidInternalMDRefs = validInternalMDRefs;
            InvalidInternalMDRefs = invalidInternalMDRefs;
            ValidExternalMDRefs = validExternalMDRefs;
            InvalidExternalMDRefs = invalidExternalMDRefs;
        }

        /// <summary>
        /// Without file extension
        /// </summary>
        public string Name { get; }

        public string FullPath { get; }

        public IReadOnlyCollection<string> ValidInternalMDRefs { get; }
        public IReadOnlyCollection<string> InvalidInternalMDRefs { get; }
        public IReadOnlyCollection<string> ValidExternalMDRefs { get; }
        public IReadOnlyCollection<string> InvalidExternalMDRefs { get; }
    }
}