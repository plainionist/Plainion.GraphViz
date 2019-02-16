using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class AbstractAlgorithm
    {
        public AbstractAlgorithm(IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, nameof(presentation));

            Presentation = presentation;
        }

        protected IGraphPresentation Presentation { get; }
    }
}
