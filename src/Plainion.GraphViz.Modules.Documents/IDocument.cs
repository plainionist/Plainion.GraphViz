namespace Plainion.GraphViz.Modules.Documents
{
    public interface IDocument
    {
        string Filename { get; }

        void Load( string path );
    }
}
