using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Algorithms
{
    public class SelectAll
    {
        private readonly IGraphPresentation myPresentation;

        public SelectAll(IGraphPresentation presentation)
        {
            Contract.RequiresNotNull(presentation, "presentation");

            myPresentation = presentation;
        }

        public void Execute(bool select)
        {
            var selection = myPresentation.GetPropertySetFor<Selection>();
            foreach (var item in selection.Items)
            {
                item.IsSelected = select;
            }
        }
    }
}
