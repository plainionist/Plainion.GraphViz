﻿namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Markdown
{
    internal abstract record Link
    {
        protected Link(string url, string label)
        {
            Contract.RequiresNotNullNotEmpty(url);

            Url = url;
            Label = label;
        }

        public string Url { get; }
        public string Label { get; }
    }
}