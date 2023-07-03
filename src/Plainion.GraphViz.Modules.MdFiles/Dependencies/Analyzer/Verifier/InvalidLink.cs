using System;

namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Verifier
{
    internal class InvalidLink : VerifiedLink
    {
        public InvalidLink(string url, Exception exception) : base(url)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}