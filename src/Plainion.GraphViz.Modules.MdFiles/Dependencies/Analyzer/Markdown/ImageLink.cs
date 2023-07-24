namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Markdown
{
    internal record ImageLink : Link
    {
        public ImageLink(string url, string label) : base(url, label)
        {
        }
    }
}