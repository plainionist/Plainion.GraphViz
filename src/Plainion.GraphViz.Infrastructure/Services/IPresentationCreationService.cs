using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Infrastructure.Services
{
    public interface IPresentationCreationService
    {
        IGraphPresentation CreatePresentation( string dataRoot );
    }
}
