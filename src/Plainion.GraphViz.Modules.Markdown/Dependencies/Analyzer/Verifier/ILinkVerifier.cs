namespace Plainion.GraphViz.Modules.Markdown.Dependencies.Analyzer.Verifier
{
    internal interface ILinkVerifier
    {
        public VerifiedLink VerifyLink(string link);
    }
}