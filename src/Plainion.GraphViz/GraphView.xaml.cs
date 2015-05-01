using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Visuals;
using Plainion.Prism.Interactivity.InteractionRequest;

namespace Plainion.GraphViz
{
    public partial class GraphView : UserControl, IGraphViewNavigation, IPrintRequestAware
    {
        private Point myZoomStartPoint;
        private AdornerLayer myAdornerLayer;
        private RubberbandAdorner myRubberBand;

        private DateTime myLastRefresh = DateTime.Now;

        public GraphView()
        {
            InitializeComponent();

            ContextMenus = new Dictionary<string, ContextMenu>();

            PreviewMouseRightButtonDown += OnPreviewMouseRightButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;

            Loaded += OnLoaded;
        }

        public IGraphViewNavigation Navigation
        {
            get { return ( IGraphViewNavigation )GetValue( NavigationProperty ); }
            set { throw new NotSupportedException( "Read-only property" ); }
        }

        public static readonly DependencyProperty NavigationProperty = DependencyProperty.Register( "Navigation", typeof( IGraphViewNavigation ),
            typeof( GraphView ) );

        public IGraphItem GraphItemForContextMenu
        {
            get { return ( IGraphItem )GetValue( GraphItemForContextMenuProperty ); }
            set { SetValue( GraphItemForContextMenuProperty, value ); }
        }

        public static readonly DependencyProperty GraphItemForContextMenuProperty = DependencyProperty.Register( "GraphItemForContextMenu", typeof( IGraphItem ),
            typeof( GraphView ) );

        public IGraphPresentation GraphSource
        {
            get { return ( IGraphPresentation )GetValue( GraphSourceProperty ); }
            set { SetValue( GraphSourceProperty, value ); }
        }

        public static readonly DependencyProperty GraphSourceProperty = DependencyProperty.Register( "GraphSource", typeof( IGraphPresentation ),
            typeof( GraphView ), new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnGraphSourceChanged ) ) );

        private static void OnGraphSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            var graphView = ( GraphView )d;

            if( graphView.myGraphVisual.Presentation != null )
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer( graphView.myGraphVisual );
                var adorners = adornerLayer.GetAdorners( graphView.myGraphVisual );
                if( adorners != null )
                {
                    var toolTipAdorner = adorners.OfType<ToolTipAdorner>().SingleOrDefault();
                    if( toolTipAdorner != null )
                    {
                        adornerLayer.Remove( toolTipAdorner );
                    }
                }
            }

            var presentation = ( IGraphPresentation )e.NewValue;
            graphView.myGraphVisual.Presentation = presentation;

            if( graphView.myGraphVisual.Presentation != null )
            {
                var toolTipModule = presentation.GetPropertySetFor<ToolTipContent>();
                if( toolTipModule != null )
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer( graphView.myGraphVisual );
                    var adorner = new ToolTipAdorner( graphView.myGraphVisual, graphView.myGraphVisual, toolTipModule );
                    adornerLayer.Add( adorner );
                }
            }
        }

        public ILayoutEngine LayoutEngine
        {
            get { return ( ILayoutEngine )GetValue( LayoutEngineProperty ); }
            set { SetValue( LayoutEngineProperty, value ); }
        }

        public static readonly DependencyProperty LayoutEngineProperty = DependencyProperty.Register( "LayoutEngine", typeof( ILayoutEngine ),
            typeof( GraphView ), new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnLayoutEngineChanged ) ) );

        public bool IsRenderingEnabled
        {
            get { return ( bool )GetValue( IsRenderingEnabledProperty ); }
            set { SetValue( IsRenderingEnabledProperty, value ); }
        }

        public static readonly DependencyProperty IsRenderingEnabledProperty = DependencyProperty.Register( "IsRenderingEnabled", typeof( bool ),
            typeof( GraphView ), new FrameworkPropertyMetadata( true ) );

        private static void OnLayoutEngineChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ( ( GraphView )d ).myGraphVisual.LayoutEngine = ( ILayoutEngine )e.NewValue;
        }

        private void OnLoaded( object sender, RoutedEventArgs e )
        {
            SetValue( NavigationProperty, this );

            myGraphVisual.RenderingFinished += OnRenderingFinished;

            ComponentDispatcher.ThreadIdle += OnIdle;
        }

        // http://gaggerostechnicalnotes.blogspot.de/2012/01/onidle-event-in-wpf.html
        private void OnIdle( object sender, EventArgs e )
        {
            if( !IsRenderingEnabled )
            {
                return;
            }

            if( DateTime.Now - myLastRefresh > TimeSpan.FromMilliseconds( 50 ) )
            {
                myLastRefresh = DateTime.Now;
                myGraphVisual.Refresh();
            }
        }

        public IVisualPicking Picking
        {
            get { return myGraphVisual; }
        }

        private void OnRenderingFinished( object sender, EventArgs e )
        {
            FitGraphToWindow();

            FocusManager.SetIsFocusScope( this, true );
            Focus();
            Keyboard.Focus( this );
        }

        private void FitGraphToWindow()
        {
            var padding = myGraphVisual.Margin.Left + myGraphVisual.Margin.Right;
            var scaleX = ( RenderSize.Width - padding ) / myGraphVisual.ContentSize.Width;
            var scaleY = ( RenderSize.Height - padding ) / myGraphVisual.ContentSize.Height;
            var scale = Math.Min( 1, Math.Min( scaleX, scaleY ) );

            myScaleTransform.ScaleX = scale;
            myScaleTransform.ScaleY = scale;

            UpdateLayout();

            ScrollViewer.ScrollToHorizontalOffset( ( myGraphVisual.RenderSize.Width - ScrollViewer.ViewportWidth ) / 2 );
            ScrollViewer.ScrollToVerticalOffset( ( myGraphVisual.RenderSize.Height - ScrollViewer.ViewportHeight ) / 2 );
        }

        public DocumentPaginator GetPaginator( Size printSize )
        {
            return new GraphPaginator( myGraphVisual.Drawing, printSize );
        }

        public PageOrientation PreferredOrientation
        {
            get
            {
                return ( myGraphVisual.ActualWidth > myGraphVisual.ActualHeight ) ? PageOrientation.Landscape : PageOrientation.Portrait;
            }
        }

        private void OnPreviewMouseRightButtonDown( object sender, MouseButtonEventArgs e )
        {
            myZoomStartPoint = e.GetPosition( myGraphVisual );

            GraphItemForContextMenu = myGraphVisual.Pick( myZoomStartPoint );

            myAdornerLayer = AdornerLayer.GetAdornerLayer( this );
            myRubberBand = new RubberbandAdorner( this, e.GetPosition( this ), args => args.RightButton );
            myRubberBand.Closed += myRubberBand_Closed;
            myAdornerLayer.Add( myRubberBand );
        }

        private void myRubberBand_Closed( object sender, EventArgs e )
        {
            bool handled = ZoomTo( myZoomStartPoint, Mouse.GetPosition( myGraphVisual ) );

            if( handled )
            {
                return;
            }

            var key = GraphItemForContextMenu != null ? GraphItemForContextMenu.GetType().Name : "Default";

            ContextMenu menu = null;
            ContextMenus.TryGetValue( key, out menu );
            ContextMenu = menu;

            if( ContextMenu == null )
            {
                return;
            }

            ContextMenu.PlacementTarget = this;
            ContextMenu.IsOpen = true;
        }

        // relative to rendertarget
        private bool ZoomTo( Point start, Point end )
        {
            var x = Math.Min( start.X, end.X );
            var y = Math.Min( start.Y, end.Y );
            var width = Math.Abs( start.X - end.X );
            var height = Math.Abs( start.Y - end.Y );

            if( width < 10 || height < 10 )
            {
                // tollerance against strange mouse handling
                return false;
            }

            double zoom;
            if( width > height )
            {
                zoom = ScrollViewer.ViewportWidth / width;
            }
            else
            {
                zoom = ScrollViewer.ViewportHeight / height;
            }

            // uses same zoom for x and y to preserve aspect ratio
            myScaleTransform.ScaleX = zoom;
            myScaleTransform.ScaleY = zoom;

            var newXY = myScaleTransform.Transform( new Point( x, y ) );
            if( double.IsNaN( newXY.X ) || double.IsNaN( newXY.Y ) )
            {
                return true;
            }

            if( width > height )
            {
                // keep x but center y
                newXY.Y -= ( ( ScrollViewer.ViewportHeight - height * zoom ) / 2 );
            }
            else
            {
                // keep y but center x
                newXY.X -= ( ( ScrollViewer.ViewportWidth - width * zoom ) / 2 );
            }

            ScrollViewer.ScrollToHorizontalOffset( newXY.X );
            ScrollViewer.ScrollToVerticalOffset( newXY.Y );

            return true;
        }

        protected override void OnPreviewMouseWheel( MouseWheelEventArgs e )
        {
            e.Handled = true;

            var absMousePos = Mouse.GetPosition( myGraphVisual );
            var poiBeforeScale = myScaleTransform.Transform( absMousePos );

            myScaleTransform.ScaleX *= 1 + ( Math.Sign( e.Delta ) * 0.1 );
            myScaleTransform.ScaleY *= 1 + ( Math.Sign( e.Delta ) * 0.1 );

            var poiAfterScale = myScaleTransform.Transform( absMousePos );
            var xoff = ScrollViewer.HorizontalOffset + ( poiAfterScale.X - poiBeforeScale.X );
            var yoff = ScrollViewer.VerticalOffset + ( poiAfterScale.Y - poiBeforeScale.Y );

            ScrollViewer.ScrollToHorizontalOffset( xoff );
            ScrollViewer.ScrollToVerticalOffset( yoff );
        }

        private void OnMouseLeftButtonUp( object sender, MouseButtonEventArgs e )
        {
            if( Keyboard.IsKeyDown( Key.LeftCtrl ) || Keyboard.IsKeyDown( Key.RightCtrl ) )
            {
                var graphItem = myGraphVisual.PickMousePosition();
                if( graphItem == null )
                {
                    return;
                }

                var selection = myGraphVisual.Presentation.GetPropertySetFor<Selection>().Get( graphItem.Id );
                selection.IsSelected = !selection.IsSelected;
            }
        }

        public void NavigateTo( IGraphItem item )
        {
            var boundingBox = myGraphVisual.GetBoundingBox( item );
            if( boundingBox == null )
            {
                return;
            }

            var rect = boundingBox.Value;

            // lets do some padding
            rect.Inflate( rect.Width, rect.Height );

            ZoomTo( rect.TopLeft, rect.BottomRight );
        }

        /// <summary>
        /// Key: name of the type of the element under cursor - "Default" for none
        /// </summary>
        public Dictionary<string, ContextMenu> ContextMenus
        {
            get;
            set;
        }
    }
}