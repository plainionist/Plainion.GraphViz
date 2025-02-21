using System;

namespace Plainion.GraphViz.Modules.Markdown.Analyzer
{
    internal class FailedFile
    {
        public string FullPath { get; init; }
        public Exception Exception { get; init; }
    }
}