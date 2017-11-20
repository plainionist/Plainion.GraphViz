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
            foreach (var n in myPresentation.Graph.Nodes)
            {
                selection.Get(n.Id).IsSelected = select;
            }
            foreach (var e in myPresentation.Graph.Edges)
            {
                selection.Get(e.Id).IsSelected = select;
            }
        }
    }
}
