﻿namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Verifier
{
    internal abstract record VerifiedLink
    {
        protected VerifiedLink(string path)
        {
            Contract.RequiresNotNullNotEmpty(path);

            Path = path;
        }

        public string Path { get; }
    }
}