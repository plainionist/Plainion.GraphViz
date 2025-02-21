namespace Plainion.GraphViz.Modules.Markdown.Dependencies.Analyzer.Verifier
{
    internal record ValidLink : VerifiedLink
    {
        public ValidLink(string path) : base(path)
        {
        }
    }
}