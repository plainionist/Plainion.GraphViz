using System;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer
{
    internal class FailedFile
    {
        public string FullPath { get; init; }
        public Exception Exception { get; init; }
    }
}