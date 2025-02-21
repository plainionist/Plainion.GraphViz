namespace Plainion.GraphViz.Modules.Markdown.Analyzer.Verifier
{
    internal interface ILinkVerifier
    {
        public VerifiedLink VerifyLink(string link);
    }
}