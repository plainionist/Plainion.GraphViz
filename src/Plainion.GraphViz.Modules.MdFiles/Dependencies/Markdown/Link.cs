namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Markdown
{
    internal abstract class Link
    {
        protected Link(string url, string label)
        {
            Url = url;
            Label = label;
        }

        public string Url { get; }
        public string Label { get; }
    }
}