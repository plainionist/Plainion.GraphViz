using System;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies
{
    internal class FailedFile
    {
        public string FullPath { get; init; }
        public Exception Exception { get; init; }
    }
}