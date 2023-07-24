namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Verifier
{
    internal record ValidLink : VerifiedLink
    {
        public ValidLink(string path) : base(path)
        {
        }
    }
}