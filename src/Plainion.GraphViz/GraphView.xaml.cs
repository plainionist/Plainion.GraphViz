using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Visuals;
using Plainion.Windows.Mvvm;

namespace Plainion.GraphViz
{
    public partial class GraphView : UserControl, IGraphViewNavigation, IPrintRequestAware, IGraphViewExport
    {
        private Point myZoomStartPoint;
        private AdornerLayer myAdornerLayer;
        private RubberbandAdorner myRubberBand;
        private DateTime myLastRefresh = DateTime.Now;
        private bool myZoomGuard;

        public GraphView()
        {
            InitializeComponent();

            ContextMenus = new Dictionary<string, ContextMenu>();

            PreviewMouseRightButtonDown += OnPreviewMouseRightButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;

            Loaded += OnLoaded;

            myZoomSlider.Value = 50;
            myZoomSlider.IsEnabled = false;

            // ensure to get key events
            myZoomSlider.Focusable = false;
            Focusable = true;

            Themes.Naked.PropertyChanged += Naked_PropertyChanged;
        }

        private void Naked_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ScrollViewer.HorizontalScrollBarVisibility = Themes.Naked.IsEnabled ? ScrollBarVisibility.Hidden : ScrollBarVisibility.Auto;
            ScrollViewer.VerticalScrollBarVisibility = Themes.Naked.IsEnabled ? ScrollBarVisibility.Hidden : ScrollBarVisibility.Auto;
            myZoomSlider.Visibility = Themes.Naked.IsEnabled ? Visibility.Hidden : Visibility.Visible;
        }

        public IGraphViewNavigation Navigation
        {
            get { return (IGraphViewNavigation)GetValue(NavigationProperty); }
            set { throw new NotSupportedException("Read-only property"); }
        }

        public static readonly DependencyProperty NavigationProperty = DependencyProperty.Register("Navigation", typeof(IGraphViewNavigation),
            typeof(GraphView));

        public IGraphViewExport Export
        {
            get { return (IGraphViewExport)GetValue(ExportProperty); }
            set { throw new NotSupportedException("Read-only property"); }
        }

        public static readonly DependencyProperty ExportProperty = DependencyProperty.Register("Export", typeof(IGraphViewExport),
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
            ((GraphView)d).OnGraphSourceChanged();
        }

        private void OnGraphSourceChanged()
        {
            if (myGraphVisual.Presentation != null)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(myGraphVisual);
                var adorners = adornerLayer.GetAdorners(myGraphVisual);
                if (adorners != null)
                {
                    var toolTipAdorner = adorners.OfType<ToolTipAdorner>().SingleOrDefault();
                    if (toolTipAdorner != null)
                    {
                        adornerLayer.Remove(toolTipAdorner);
                    }
                }
            }

            myGraphVisual.Presentation = GraphSource;

            if (myGraphVisual.Presentation != null)
            {
                var toolTipModule = GraphSource.GetPropertySetFor<ToolTipContent>();
                if (toolTipModule != null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(myGraphVisual);
                    var adorner = new ToolTipAdorner(myGraphVisual, myGraphVisual, toolTipModule);
                    adornerLayer.Add(adorner);
                }
            }

            myZoomSlider.IsEnabled = myGraphVisual.Presentation != null;
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
            SetValue(ExportProperty, this);

            myGraphVisual.RenderingFinished += OnRenderingFinished;

            ComponentDispatcher.ThreadIdle += OnIdle;
        }

        private void OnRenderingFinished(object sender, EventArgs e)
        {
            FitGraphToWindow();

            // if we do this here we loose focus in e.g. NodeEditor or other UI elements
            // which influence graph
            //FocusManager.SetIsFocusScope(this, true);
            //Focus();
            //Keyboard.Focus(this);
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

            // prepare for key events
            Focus();
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

            Scale(zoom);

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

            Scale(scale);

            UpdateLayout();

            ScrollViewer.ScrollToHorizontalOffset((myGraphVisual.RenderSize.Width - ScrollViewer.ViewportWidth) / 2);
            ScrollViewer.ScrollToVerticalOffset((myGraphVisual.RenderSize.Height - ScrollViewer.ViewportHeight) / 2);
        }

        // uses same zoom for x and y to preserve aspect ratio
        private void Scale(double scale)
        {
            myScaleTransform.ScaleX = scale;
            myScaleTransform.ScaleY = scale;

            myGraphVisual.SetScaling(myScaleTransform.ScaleX, myScaleTransform.ScaleY);
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            e.Handled = true;

            var absMousePos = Mouse.GetPosition(myGraphVisual);
            var delta = 1 + (Math.Sign(e.Delta) * 0.1);
            ZoomAtPoint(absMousePos, delta);
        }

        private void ZoomAtPoint(Point pos, double delta)
        {
            var poiBeforeScale = myScaleTransform.Transform(pos);

            Scale(myScaleTransform.ScaleX * delta);

            var poiAfterScale = myScaleTransform.Transform(pos);
            var xoff = ScrollViewer.HorizontalOffset + (poiAfterScale.X - poiBeforeScale.X);
            var yoff = ScrollViewer.VerticalOffset + (poiAfterScale.Y - poiBeforeScale.Y);

            ScrollViewer.ScrollToHorizontalOffset(xoff);
            ScrollViewer.ScrollToVerticalOffset(yoff);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                var absMousePos = Mouse.GetPosition(myGraphVisual);
                var delta = 1 + ((e.Key == Key.Up ? 1 : -1) * 0.1);
                ZoomAtPoint(absMousePos, delta);

                e.Handled = true;
            }

            base.OnPreviewKeyUp(e);
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

            // prepare for key events
            Focus();
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

        private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var slider = (Slider)sender;

            myZoomGuard = true;
            slider.Value = slider.Maximum / 2;
            myZoomGuard = false;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (myZoomGuard || GraphSource == null)
            {
                return;
            }

            var slider = (Slider)sender;

            var delta = (1 + (Math.Sign(e.NewValue - e.OldValue) * 0.025));
            if (delta == 0)
            {
                return;
            }

            Scale(myScaleTransform.ScaleX * delta);

            // makes the scrolling smoooth
            UpdateLayout();

            ScrollViewer.ScrollToHorizontalOffset(ScrollViewer.ScrollableWidth / 2);
            ScrollViewer.ScrollToVerticalOffset(ScrollViewer.ScrollableHeight / 2);
        }

        public void ExportAsPng(Stream stream)
        {
            var bounds = myGraphVisual.Drawing.DescendantBounds;
            bounds.Union(myGraphVisual.Drawing.ContentBounds);

            double scaleFactor = 2.0;
            double dpiScale = 300.0 * scaleFactor / 96;

            int width = Math.Min(10 * 1024, Convert.ToInt32(bounds.Width * dpiScale * scaleFactor));
            int height = Convert.ToInt32(bounds.Height * dpiScale * scaleFactor);

            var target = new DrawingVisual();
            using (var dc = target.RenderOpen())
            {
                // Apply a ScaleTransform to render at higher resolution
                dc.PushTransform(new ScaleTransform(scaleFactor, scaleFactor));
                foreach (DrawingVisual child in myGraphVisual.Drawing.Children)
                {
                    dc.DrawDrawing(child.Drawing);
                }
                dc.Pop();
            }

            var bmp = new RenderTargetBitmap(width, height, 600, 600, PixelFormats.Pbgra32);
            bmp.Render(target);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));

            encoder.Save(stream);
        }
    }
}