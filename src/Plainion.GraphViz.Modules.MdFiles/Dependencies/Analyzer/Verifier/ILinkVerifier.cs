namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Verifier
{
    internal interface ILinkVerifier
    {
        public VerifiedLink VerifyInternalLink(string url);
    }
}