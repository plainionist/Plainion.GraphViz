namespace Plainion.GraphViz.Modules.MdFiles.Dependencies.Verifier
{
    internal abstract class VerifiedLink
    {
        protected VerifiedLink(string url)
        {
            Url = url;
        }

        public string Url { get; }
    }
}