using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Visuals;

namespace Plainion.GraphViz
{
    class ToolTipAdorner : Adorner
    {
        private readonly ToolTipController myToolTipController;
        private readonly IVisualPicking myPicking;
        private readonly IPropertySetModule<ToolTipContent> myToolTipModule;

        public ToolTipAdorner(UIElement owner, IVisualPicking picking, IPropertySetModule<ToolTipContent> toolTipModule)
            : base(owner)
        {
            myPicking = picking;
            myToolTipModule = toolTipModule;

            myToolTipController = new ToolTipController();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            myToolTipController.Hide();

            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            myToolTipController.Hide();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var graphItem = myPicking.PickMousePosition();

            if (graphItem == null)
            {
                myToolTipController.Hide();
                return;
            }

            if (graphItem is not Node)
            {
                return;
            }

            var toolTipState = myToolTipModule.Get(graphItem.Id);
            if (toolTipState == null)
            {
                toolTipState = new ToolTipContent(graphItem.Id, new TextBlock { Text = graphItem.Id });
                myToolTipModule.Add(toolTipState);
            }

            if (toolTipState.Content == null)
            {
                toolTipState.Content = new TextBlock { Text = graphItem.Id };
            }

            myToolTipController.Move(graphItem.Id, toolTipState.Content);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            // without a background the OnMouseMove event would not be fired!
            // Alternative: implement a Canvas as a child of this adorner, like
            // the ConnectionAdorner does.
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        }
    }
}
