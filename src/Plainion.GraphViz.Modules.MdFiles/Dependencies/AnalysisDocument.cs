using System.Collections.Generic;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies
{
    internal class AnalysisDocument
    {
        public IList<MDFile> Files { get; } = new List<MDFile>();
        public IList<FailedFile> FailedItems { get; } = new List<FailedFile>();
    }
}