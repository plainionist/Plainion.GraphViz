namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Verifier
{
    internal interface ILinkVerifier
    {
        public VerifiedLink VerifyInternalLink(string url);
    }
}