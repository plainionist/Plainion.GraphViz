using System.Collections.Generic;

namespace Plainion.GraphViz.Modules.VsProjects
{
    class VsProject
    {
        /// <summary>
        /// Without file extension
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Relative path to the project file from root of analyzed code base
        /// </summary>
        public string RelativePath { get; init; }

        /// <summary>
        /// Without file extension
        /// </summary>
        public string Assembly { get; init; }

        /// <summary>
        /// Name of the referenced assembly without version and public key.
        /// </summary>
        public IReadOnlyCollection<string> References { get; init; }

        /// <summary>
        /// Full path to the project
        /// </summary>
        public IReadOnlyCollection<string> ProjectReferences { get; init; }

        /// <summary>
        /// Package name without version
        /// </summary>
        public IReadOnlyCollection<string> PackageReferences { get; init; }
    }
}