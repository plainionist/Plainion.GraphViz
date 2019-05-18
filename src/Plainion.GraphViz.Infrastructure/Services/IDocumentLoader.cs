
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Infrastructure.Services
{
    public interface IDocumentLoader
    {
        bool CanLoad(string filename);

        void Load(string filename);

        IGraphPresentation Read(string filename);
    }
}
