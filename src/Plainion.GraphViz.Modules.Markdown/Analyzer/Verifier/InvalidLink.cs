using System;

namespace Plainion.GraphViz.Modules.Markdown.Analyzer.Verifier
{
    internal record InvalidLink : VerifiedLink
    {
        public InvalidLink(string path, Exception exception) : base(path)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}