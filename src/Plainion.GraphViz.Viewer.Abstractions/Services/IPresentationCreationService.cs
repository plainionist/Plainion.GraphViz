using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Viewer.Abstractions.Services
{
    public interface IPresentationCreationService
    {
        IGraphPresentation CreatePresentation(string dataRoot);
    }
}
