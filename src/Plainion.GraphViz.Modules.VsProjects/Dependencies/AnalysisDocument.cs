using System.Collections.Generic;

namespace Plainion.GraphViz.Modules.VsProjects.Dependencies
{
    class AnalysisDocument
    {
        public IList<VsProject> Projects { get; } = new List<VsProject>();
        public IList<FailedProject> FailedItems { get; } = new List<FailedProject>();
    }
}