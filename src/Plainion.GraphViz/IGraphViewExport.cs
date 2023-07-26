using System.IO;

namespace Plainion.GraphViz
{
    public interface IGraphViewExport
    {
        void ExportAsPng(Stream stream);
    }
}