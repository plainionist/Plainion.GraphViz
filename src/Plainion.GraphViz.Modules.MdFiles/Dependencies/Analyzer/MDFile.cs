using System.Collections.Generic;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer
{
    internal class MDFile
    {
        public MDFile(string name, string fullPath,
            IReadOnlyCollection<string> validMDReferences, IReadOnlyCollection<string> invalidMDReferences)
        {
            Contract.RequiresNotNullNotEmpty(name);
            Contract.RequiresNotNullNotEmpty(fullPath);
            Contract.RequiresNotNull(validMDReferences);
            Contract.RequiresNoDuplicates(validMDReferences);
            Contract.RequiresNotNull(invalidMDReferences);
            Contract.RequiresNoDuplicates(invalidMDReferences);

            Name = name;
            FullPath = fullPath;
            ValidMDReferences = validMDReferences;
            InvalidMDReferences = invalidMDReferences;
        }

        /// <summary>
        /// Without file extension
        /// </summary>
        public string Name { get;}

        public string FullPath { get;}

        public IReadOnlyCollection<string> ValidMDReferences { get;}
        public IReadOnlyCollection<string> InvalidMDReferences { get; }
    }
}