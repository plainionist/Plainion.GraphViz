using System;

namespace Plainion.GraphViz.Modules.VsProjects.Dependencies
{
    class FailedProject
    {
        public string FullPath { get; init; }
        public Exception Exception { get; init; }
    }
}