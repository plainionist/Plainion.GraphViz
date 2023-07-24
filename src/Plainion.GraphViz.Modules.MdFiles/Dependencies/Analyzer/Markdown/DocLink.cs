namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Markdown
{
    internal record DocLink : Link
    {
        public DocLink(string url, string label) : base(url, label)
        {
        }
    }
}