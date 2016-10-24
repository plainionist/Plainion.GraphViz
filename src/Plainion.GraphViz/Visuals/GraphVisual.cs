using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Visuals
{
    public class GraphVisual : FrameworkElement, IVisualPicking
    {
        private IGraphPresentation myPresentation;
        private DrawingVisual myDrawing;
        // indexed access to all visible visuals
        private IDictionary<string, AbstractElementVisual> myDrawingElements;

        private IModuleChangedJournal<Selection> mySelectionJournal;
        private IModuleChangedJournal<INodeMask> myNodeMaskJournal;
        private IModuleChangedJournal<Edge> myEdgeMaskJournal;
        private IModuleChangedJournal<IGraphTransformation> myTransformationsJournal;

        private double myOldScaling;
        private double myCurrentScaling;

        public GraphVisual()
        {
            myDrawing = new DrawingVisual();
            myDrawingElements = new Dictionary<string, AbstractElementVisual>();

            ClipToBounds = true;
            myOldScaling = 1;
            myCurrentScaling = 1;

            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
        }

        public IGraphPresentation Presentation
        {
            get { return myPresentation; }
            set
            {
                if( myPresentation == value )
                {
                    return;
                }

                if( myPresentation != null )
                {
                    mySelectionJournal.Dispose();
                    myNodeMaskJournal.Dispose();
                    myEdgeMaskJournal.Dispose();
                    myTransformationsJournal.Dispose();

                    myDrawingElements.Clear();
                }

                myPresentation = value;

                if( myPresentation != null )
                {
                    mySelectionJournal = myPresentation.GetPropertySetFor<Selection>().CreateJournal();
                    myNodeMaskJournal = myPresentation.GetModule<INodeMaskModule>().CreateJournal();
                    myEdgeMaskJournal = myPresentation.GetModule<IEdgeMaskModule>().CreateJournal();
                    myTransformationsJournal = myPresentation.GetModule<ITransformationModule>().CreateJournal();
                }
            }
        }

        public event EventHandler RenderingFinished;

        public ILayoutEngine LayoutEngine { get; set; }

        public void Refresh()
        {
            if( Presentation == null )
            {
                return;
            }

            var layoutModule = Presentation.GetModule<IGraphLayoutModule>();
            var transformationModule = myPresentation.GetModule<ITransformationModule>();

            // current assumption: it is enough to check the nodes as we would not render edges independent from nodes

            var visibleNodes = transformationModule.Graph.Nodes
                .Where( node => Presentation.Picking.Pick( node ) )
                .ToList();

            if( visibleNodes.Count == 0 && myDrawingElements.Count == 0 )
            {
                // no visible nodes and this also already been reflected in the rendering result (canvas is empty)
                return;
            }

            var reLayout = visibleNodes
                .Any( node => layoutModule.GetLayout( node ) == null )
                || !myTransformationsJournal.IsEmpty;

            if( reLayout )
            {
                Contract.Invariant( LayoutEngine != null, "LayoutEngine not set" );

                Debug.WriteLine( "Relayouting" );
                LayoutEngine.Relayout( Presentation );

                // reLayout requires reRender
                myDrawingElements.Clear();
            }

            this.SnapsToDevicePixels = true;

            if( myDrawingElements.Count == 0 )
            {
                Debug.WriteLine( "Re-Rendering" );

                RemoveVisualChild( myDrawing );

                myDrawing.Children.Clear();

                myDrawing = new DrawingVisual();

                // first draw edges so that in case of overlap with nodes nodes are on top
                foreach( var edge in transformationModule.Graph.Edges )
                {
                    if( !Presentation.Picking.Pick( edge ) )
                    {
                        continue;
                    }

                    var visual = new EdgeVisual( edge, Presentation, myOldScaling );

                    myDrawingElements.Add( edge.Id, visual );

                    var layoutState = layoutModule.GetLayout( edge );
                    visual.Draw( layoutState );

                    myDrawing.Children.Add( visual.Visual );
                }

                foreach( var node in transformationModule.Graph.Nodes )
                {
                    if( !Presentation.Picking.Pick( node ) )
                    {
                        continue;
                    }

                    var visual = new NodeVisual( node, Presentation );

                    myDrawingElements.Add( node.Id, visual );

                    var layoutState = layoutModule.GetLayout( node );
                    visual.Draw( layoutState );

                    myDrawing.Children.Add( visual.Visual );
                }

                foreach( var cluster in transformationModule.Graph.Clusters )
                {
                    if( !cluster.Nodes.Any( n => Presentation.Picking.Pick( n ) ) )
                    {
                        continue;
                    }

                    var visual = new ClusterVisual( cluster, Presentation );

                    myDrawingElements.Add( cluster.Id, visual );

                    visual.Draw( myDrawingElements );

                    myDrawing.Children.Insert( 0, visual.Visual );
                }

                AddVisualChild( myDrawing );

                var selectionModule = Presentation.GetPropertySetFor<Selection>();
                foreach( var e in selectionModule.Items )
                {
                    AbstractElementVisual drawing;
                    if( myDrawingElements.TryGetValue( e.OwnerId, out drawing ) )
                    {
                        drawing.Select( e.IsSelected );
                    }
                }

                // clear journals - to avoid considering out-dated infos on next refresh
                myTransformationsJournal.Clear();
                myEdgeMaskJournal.Clear();
                myNodeMaskJournal.Clear();
                mySelectionJournal.Clear();

                InvalidateMeasure();

                if( RenderingFinished != null )
                {
                    RenderingFinished( this, EventArgs.Empty );
                }
            }
            else
            {
                // first draw edges so that in case of overlap with nodes nodes are on top
                if( !myEdgeMaskJournal.IsEmpty )
                {
                    // EdgeMask module can only hide additional edges so no picking required here
                    foreach( var edge in myEdgeMaskJournal.Entries )
                    {
                        AbstractElementVisual visual;
                        if( myDrawingElements.TryGetValue( edge.Id, out visual ) )
                        {
                            SetVisibility( ( EdgeVisual )visual, false, null );
                        }
                    }

                    myEdgeMaskJournal.Clear();
                    InvalidateVisual();
                }

                if( !myNodeMaskJournal.IsEmpty )
                {
                    foreach( var node in transformationModule.Graph.Nodes )
                    {
                        AbstractElementVisual visual;
                        if( myDrawingElements.TryGetValue( node.Id, out visual ) )
                        {
                            SetVisibility( ( NodeVisual )visual,
                                Presentation.Picking.Pick( node ),
                                v => v.Draw( layoutModule.GetLayout( v.Owner ) ) );
                        }
                    }

                    foreach( var edge in transformationModule.Graph.Edges )
                    {
                        AbstractElementVisual visual;
                        if( myDrawingElements.TryGetValue( edge.Id, out visual ) )
                        {
                            SetVisibility( ( EdgeVisual )visual,
                                Presentation.Picking.Pick( edge ),
                                v => v.Draw( layoutModule.GetLayout( v.Owner ) ) );
                        }
                    }

                    // if node visibility changed we should check whether clusters have to be redrawn
                    foreach( var cluster in transformationModule.Graph.Clusters )
                    {
                        if( cluster.Nodes.Any( n => Presentation.Picking.Pick( n ) ) )
                        {
                            if( !myDrawingElements.ContainsKey( cluster.Id ) )
                            {
                                var visual = new ClusterVisual( cluster, Presentation );

                                myDrawingElements.Add( cluster.Id, visual );

                                visual.Draw( myDrawingElements );

                                myDrawing.Children.Insert( 0, visual.Visual );
                            }
                        }
                        else
                        {
                            AbstractElementVisual visual;
                            if( myDrawingElements.TryGetValue( cluster.Id, out visual ) )
                            {
                                myDrawingElements.Remove( cluster.Id );

                                myDrawing.Children.Remove( visual.Visual );
                            }
                        }
                    }

                    myNodeMaskJournal.Clear();
                    InvalidateVisual();
                }

                if( !mySelectionJournal.IsEmpty )
                {
                    foreach( var e in mySelectionJournal.Entries )
                    {
                        AbstractElementVisual visual;
                        if( myDrawingElements.TryGetValue( e.OwnerId, out visual ) )
                        {
                            visual.Select( e.IsSelected );
                        }
                    }

                    mySelectionJournal.Clear();
                    InvalidateVisual();
                }

                if (Math.Abs(myCurrentScaling - myOldScaling) / myOldScaling > 0.15d)
                {
                    myOldScaling = myCurrentScaling;

                    foreach( var edge in myDrawingElements.Values.OfType<EdgeVisual>() )
                    {
                        edge.ApplyZoomFactor( myCurrentScaling );
                    }

                    InvalidateVisual();
                }
            }
        }

        // when modifying the Children collection we have to consider that we could get here because of IsDirty
        // but IsVisible was already toggled multiple times so that actually nothing has to be done
        private void SetVisibility<T>( T visual, bool isVisible, Action<T> drawAction ) where T : AbstractElementVisual
        {
            if( isVisible )
            {
                if( visual.Visual == null )
                {
                    drawAction( visual );

                    myDrawing.Children.Add( visual.Visual );
                }
                else
                {
                    if( !myDrawing.Children.Contains( visual.Visual ) )
                    {
                        myDrawing.Children.Add( visual.Visual );
                    }
                }
            }
            else
            {
                if( myDrawing.Children.Contains( visual.Visual ) )
                {
                    myDrawing.Children.Remove( visual.Visual );
                }
            }
        }

        // TODO: do we really want to allow direct access?
        internal DrawingVisual Drawing
        {
            get { return myDrawing; }
        }

        // Provide a required override for the VisualChildCount property.
        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild( int index )
        {
            return myDrawing;
        }

        public Size ContentSize
        {
            get
            {
                if( myDrawingElements.Count == 0 )
                {
                    return new Size( 8, 8 );
                }

                var bounds = myDrawing.ContentBounds;
                bounds.Union( myDrawing.DescendantBounds );

                return new Size( bounds.Width * 64, bounds.Height * 64 );
            }
        }

        protected override Size MeasureOverride( Size availableSize )
        {
            Rect bounds = myDrawing.ContentBounds;
            bounds.Union( myDrawing.DescendantBounds );

            if( bounds.IsEmpty )
            {
                // if the graph is empty
                return new Size( 8, 8 );
            }

            // add some extra padding in case the bezier curve is to huge
            bounds.Inflate( bounds.Width * 0.01, bounds.Height * 0.01 );

            var m = new Matrix();
            m.Translate( -bounds.Left, -bounds.Top );
            // TODO: why 64?
            m.Scale( 64, 64 );

            myDrawing.Transform = new MatrixTransform( m );

            return new Size( bounds.Width * 64, bounds.Height * 64 );
        }

        public Rect? GetBoundingBox( IGraphItem item )
        {
            AbstractElementVisual visual;
            if( !myDrawingElements.TryGetValue( item.Id, out visual ) || visual.Visual == null )
            {
                return null;
            }

            var bounds = visual.Visual.ContentBounds;
            bounds.Scale( 64, 64 );
            return bounds;
        }

        public IGraphItem PickMousePosition()
        {
            return Pick( Mouse.GetPosition( this ) );
        }

        public IGraphItem Pick( Point position )
        {
            var result = VisualTreeHelper.HitTest( this, position );
            if( result == null )
            {
                return null;
            }

            var visual = result.VisualHit as DrawingVisual;

            if( visual == null )
            {
                return null;
            }

            return ( IGraphItem )visual.ReadLocalValue( AbstractElementVisual.GraphItemProperty );
        }

        public void SetScaling( double scaleX, double scaleY )
        {
            myCurrentScaling = Math.Min(scaleX, scaleY);
        }
    }
}
