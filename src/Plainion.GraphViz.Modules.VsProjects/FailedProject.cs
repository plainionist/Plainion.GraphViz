using System;

namespace Plainion.GraphViz.Modules.VsProjects
{
    class FailedProject
    {
        public string FullPath { get; init; }
        public Exception Exception { get; init; }
    }
}