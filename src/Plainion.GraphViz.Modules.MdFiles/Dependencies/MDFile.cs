using System.Collections.Generic;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies
{
    internal class MDFile
    {
        /// <summary>
        /// Without file extension
        /// </summary>
        public string Name { get; init; }

        public string FullPath { get; init; }

        public IReadOnlyCollection<string> ValidMDReferences { get; init; }
        public IReadOnlyCollection<string> InvalidMDReferences { get; init; }
    }
}