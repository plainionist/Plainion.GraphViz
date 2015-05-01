using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Visuals;

namespace Plainion.GraphViz
{
    internal class ToolTipAdorner : Adorner
    {
        private ToolTipController myToolTipController;
        private IVisualPicking myPicking;
        private IPropertySetModule<ToolTipContent> myToolTipModule;

        public ToolTipAdorner( UIElement owner, IVisualPicking picking, IPropertySetModule<ToolTipContent> toolTipModule )
            : base( owner )
        {
            myPicking = picking;
            myToolTipModule = toolTipModule;

            myToolTipController = new ToolTipController();
        }

        protected override void OnMouseDown( MouseButtonEventArgs e )
        {
            myToolTipController.Hide();

            base.OnMouseDown( e );
        }

        protected override void OnMouseLeave( MouseEventArgs e )
        {
            base.OnMouseLeave( e );

            myToolTipController.Hide();
        }

        protected override void OnMouseMove( MouseEventArgs e )
        {
            var graphItem = myPicking.PickMousePosition();

            if( graphItem == null )
            {
                myToolTipController.Hide();
                return;
            }

            if( !( graphItem is Node ) )
            {
                return;
            }

            var toolTipState = myToolTipModule.Get( graphItem.Id );
            if( toolTipState != null )
            {
                myToolTipController.Move( graphItem.Id, toolTipState.Content );
            }
        }

        protected override void OnRender( DrawingContext dc )
        {
            base.OnRender( dc );

            // without a background the OnMouseMove event would not be fired!
            // Alternative: implement a Canvas as a child of this adorner, like
            // the ConnectionAdorner does.
            dc.DrawRectangle( Brushes.Transparent, null, new Rect( RenderSize ) );
        }
    }
}
