namespace Plainion.GraphViz.Modules.Markdown.Analyzer.Verifier
{
    internal record ValidLink : VerifiedLink
    {
        public ValidLink(string path) : base(path)
        {
        }
    }
}