using System;

namespace Plainion.GraphViz.Modules.Markdown.Dependencies.Analyzer
{
    internal class FailedFile
    {
        public string FullPath { get; init; }
        public Exception Exception { get; init; }
    }
}