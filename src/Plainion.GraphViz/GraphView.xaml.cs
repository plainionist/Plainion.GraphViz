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
using Plainion.Windows.Mvvm;

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
            get { return (IGraphViewNavigation)GetValue(NavigationProperty); }
            set { throw new NotSupportedException("Read-only property"); }
        }

        public static readonly DependencyProperty NavigationProperty = DependencyProperty.Register("Navigation", typeof(IGraphViewNavigation),
            typeof(GraphView));

        public IGraphItem GraphItemForContextMenu
        {
            get { return (IGraphItem)GetValue(GraphItemForContextMenuProperty); }
            set { SetValue(GraphItemForContextMenuProperty, value); }
        }

        public static readonly DependencyProperty GraphItemForContextMenuProperty = DependencyProperty.Register("GraphItemForContextMenu", typeof(IGraphItem),
            typeof(GraphView));

        public IGraphPresentation GraphSource
        {
            get { return (IGraphPresentation)GetValue(GraphSourceProperty); }
            set { SetValue(GraphSourceProperty, value); }
        }

        public static readonly DependencyProperty GraphSourceProperty = DependencyProperty.Register("GraphSource", typeof(IGraphPresentation),
            typeof(GraphView), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnGraphSourceChanged)));

        private static void OnGraphSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graphView = (GraphView)d;

            if (graphView.myGraphVisual.Presentation != null)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(graphView.myGraphVisual);
                var adorners = adornerLayer.GetAdorners(graphView.myGraphVisual);
                if (adorners != null)
                {
                    var toolTipAdorner = adorners.OfType<ToolTipAdorner>().SingleOrDefault();
                    if (toolTipAdorner != null)
                    {
                        adornerLayer.Remove(toolTipAdorner);
                    }
                }
            }

            var presentation = (IGraphPresentation)e.NewValue;
            graphView.myGraphVisual.Presentation = presentation;

            if (graphView.myGraphVisual.Presentation != null)
            {
                var toolTipModule = presentation.GetPropertySetFor<ToolTipContent>();
                if (toolTipModule != null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(graphView.myGraphVisual);
                    var adorner = new ToolTipAdorner(graphView.myGraphVisual, graphView.myGraphVisual, toolTipModule);
                    adornerLayer.Add(adorner);
                }
            }
        }

        public ILayoutEngine LayoutEngine
        {
            get { return (ILayoutEngine)GetValue(LayoutEngineProperty); }
            set { SetValue(LayoutEngineProperty, value); }
        }

        public static readonly DependencyProperty LayoutEngineProperty = DependencyProperty.Register("LayoutEngine", typeof(ILayoutEngine),
            typeof(GraphView), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnLayoutEngineChanged)));

        public bool IsRenderingEnabled
        {
            get { return (bool)GetValue(IsRenderingEnabledProperty); }
            set { SetValue(IsRenderingEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsRenderingEnabledProperty = DependencyProperty.Register("IsRenderingEnabled", typeof(bool),
            typeof(GraphView), new FrameworkPropertyMetadata(true));

        private static void OnLayoutEngineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((GraphView)d).myGraphVisual.LayoutEngine = (ILayoutEngine)e.NewValue;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SetValue(NavigationProperty, this);

            myGraphVisual.RenderingFinished += OnRenderingFinished;

            ComponentDispatcher.ThreadIdle += OnIdle;
        }

        private void OnRenderingFinished(object sender, EventArgs e)
        {
            FitGraphToWindow();

            FocusManager.SetIsFocusScope(this, true);
            Focus();
            Keyboard.Focus(this);
        }

        // http://gaggerostechnicalnotes.blogspot.de/2012/01/onidle-event-in-wpf.html
        private void OnIdle(object sender, EventArgs e)
        {
            if (!IsRenderingEnabled)
            {
                return;
            }

            if (DateTime.Now - myLastRefresh > TimeSpan.FromMilliseconds(50))
            {
                myLastRefresh = DateTime.Now;

                try
                {
                    myGraphVisual.Refresh();
                }
                catch
                {
                    // if exception is thrown during rendering we better disable it because
                    // otherwise we will frequently get the same exceptions over and over again
                    IsRenderingEnabled = false;
                    throw;
                }
            }
        }

        public IVisualPicking Picking
        {
            get { return myGraphVisual; }
        }

        public DocumentPaginator GetPaginator(Size printSize)
        {
            return new GraphPaginator(myGraphVisual.Drawing, printSize);
        }

        public PageOrientation PreferredOrientation
        {
            get
            {
                return (myGraphVisual.ActualWidth > myGraphVisual.ActualHeight) ? PageOrientation.Landscape : PageOrientation.Portrait;
            }
        }

        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            myZoomStartPoint = e.GetPosition(myGraphVisual);

            GraphItemForContextMenu = myGraphVisual.Pick(myZoomStartPoint);

            myAdornerLayer = AdornerLayer.GetAdornerLayer(this);
            myRubberBand = new RubberbandAdorner(this, e.GetPosition(this), args => args.RightButton);
            myRubberBand.Closed += myRubberBand_Closed;
            myAdornerLayer.Add(myRubberBand);
        }

        private void myRubberBand_Closed(object sender, EventArgs e)
        {
            bool handled = ZoomTo(myZoomStartPoint, Mouse.GetPosition(myGraphVisual), false);

            if (handled)
            {
                return;
            }

            var key = GraphItemForContextMenu != null ? GraphItemForContextMenu.GetType().Name : "Default";

            ContextMenu menu = null;
            ContextMenus.TryGetValue(key, out menu);
            ContextMenu = menu;

            if (ContextMenu == null)
            {
                return;
            }

            ContextMenu.PlacementTarget = this;
            ContextMenu.IsOpen = true;
        }

        // relative to rendertarget
        // TODO: unfort. still unclear why navigteto requires scroll to center
        private bool ZoomTo(Point start, Point end, bool scrollToCenter)
        {
            var left = Math.Min(start.X, end.X);
            var top = Math.Min(start.Y, end.Y);
            var width = Math.Abs(start.X - end.X);
            var height = Math.Abs(start.Y - end.Y);

            if (width < 10 || height < 10)
            {
                // tollerance against strange mouse handling
                return false;
            }

            var zoomX = ScrollViewer.ViewportWidth / width;
            var zoomY = ScrollViewer.ViewportHeight / height;
            var zoom = Math.Min(zoomX, zoomY);

            // uses same zoom for x and y to preserve aspect ratio
            Scale(zoom, zoom);

            var scrollTarget = scrollToCenter
                ? new Point(left + width / 2, top + height / 2)
                : new Point(left, top);
            var topLeft = myScaleTransform.Transform(scrollTarget);
            if (double.IsNaN(topLeft.X) || double.IsNaN(topLeft.Y))
            {
                return true;
            }

            ScrollViewer.ScrollToHorizontalOffset(topLeft.X);
            ScrollViewer.ScrollToVerticalOffset(topLeft.Y);

            return true;
        }

        private void FitGraphToWindow()
        {
            var padding = myGraphVisual.Margin.Left + myGraphVisual.Margin.Right;
            var scaleX = (RenderSize.Width - padding) / myGraphVisual.ContentSize.Width;
            var scaleY = (RenderSize.Height - padding) / myGraphVisual.ContentSize.Height;
            var scale = Math.Min(1, Math.Min(scaleX, scaleY));

            Scale(scale, scale);

            UpdateLayout();

            ScrollViewer.ScrollToHorizontalOffset((myGraphVisual.RenderSize.Width - ScrollViewer.ViewportWidth) / 2);
            ScrollViewer.ScrollToVerticalOffset((myGraphVisual.RenderSize.Height - ScrollViewer.ViewportHeight) / 2);
        }

        private void Scale(double scaleX, double scaleY)
        {
            myScaleTransform.ScaleX = scaleX;
            myScaleTransform.ScaleY = scaleY;

            myGraphVisual.SetScaling(myScaleTransform.ScaleX, myScaleTransform.ScaleY);
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            e.Handled = true;

            var absMousePos = Mouse.GetPosition(myGraphVisual);
            var poiBeforeScale = myScaleTransform.Transform(absMousePos);

            Scale(myScaleTransform.ScaleX * (1 + (Math.Sign(e.Delta) * 0.1)), myScaleTransform.ScaleY * (1 + (Math.Sign(e.Delta) * 0.1)));

            var poiAfterScale = myScaleTransform.Transform(absMousePos);
            var xoff = ScrollViewer.HorizontalOffset + (poiAfterScale.X - poiBeforeScale.X);
            var yoff = ScrollViewer.VerticalOffset + (poiAfterScale.Y - poiBeforeScale.Y);

            ScrollViewer.ScrollToHorizontalOffset(xoff);
            ScrollViewer.ScrollToVerticalOffset(yoff);
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                var graphItem = myGraphVisual.PickMousePosition();
                if (graphItem == null)
                {
                    return;
                }

                var selection = myGraphVisual.Presentation.GetPropertySetFor<Selection>().Get(graphItem.Id);
                selection.IsSelected = !selection.IsSelected;
            }
        }

        public void NavigateTo(IGraphItem item)
        {
            var boundingBox = myGraphVisual.GetBoundingBox(item);
            if (boundingBox == null)
            {
                return;
            }

            var rect = boundingBox.Value;

            // lets do some padding
            rect.Inflate(rect.Width, rect.Height);

            ZoomTo(rect.TopLeft, rect.BottomRight, true);
        }

        public void HomeZoomPan()
        {
            FitGraphToWindow();
        }

        /// <summary>
        /// Key: name of the type of the element under cursor - "Default" for none
        /// </summary>
        public Dictionary<string, ContextMenu> ContextMenus { get; set; }
    }
}