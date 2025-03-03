using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Plainion.Graphs;
using Plainion.Graphs.Projections;
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
        private IModuleChangedJournal<IGraphTransformation> myTransformationsJournal;
        private IModuleChangedJournal<Caption> myCaptionJournal;
        private IModuleChangedJournal<GraphAttribute> myGraphAttributesJournal;

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
                if (myPresentation == value)
                {
                    return;
                }

                if (myPresentation != null)
                {
                    mySelectionJournal.Dispose();
                    myNodeMaskJournal.Dispose();
                    myTransformationsJournal.Dispose();
                    myCaptionJournal.Dispose();
                    myGraphAttributesJournal.Dispose();

                    myDrawingElements.Clear();
                }

                myPresentation = value;

                if (myPresentation != null)
                {
                    mySelectionJournal = myPresentation.GetPropertySetFor<Selection>().CreateJournal();
                    myNodeMaskJournal = myPresentation.GetModule<INodeMaskModule>().CreateJournal();
                    myTransformationsJournal = myPresentation.GetModule<ITransformationModule>().CreateJournal();
                    myCaptionJournal = myPresentation.GetPropertySetFor<Caption>().CreateJournal();
                    myGraphAttributesJournal = myPresentation.GetModule<IGraphAttributesModule>().CreateJournal();
                }
            }
        }

        private void MyGraphAttributesObserver_ModuleChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public event EventHandler RenderingFinished;

        public ILayoutEngine LayoutEngine { get; set; }

        public void Refresh()
        {
            if (Presentation == null)
            {
                return;
            }

            var layoutModule = Presentation.GetModule<IGraphLayoutModule>();
            var transformationModule = myPresentation.GetModule<ITransformationModule>();

            // current assumption: it is enough to check the nodes as we would not render edges independent from nodes

            var visibleNodes = transformationModule.Graph.Nodes
                .Where(Presentation.Picking.Pick)
                .ToList();

            if (visibleNodes.Count == 0 && myDrawingElements.Count == 0)
            {
                // no visible nodes and this also already been reflected in the rendering result (canvas is empty)
                return;
            }

            // updated attributes are automatically considered during rendering in "DotWriter"
            var reLayout = visibleNodes
                .Any(node => layoutModule.GetLayout(node) == null)
                || !myTransformationsJournal.IsEmpty
                || !myGraphAttributesJournal.IsEmpty;

            if (reLayout)
            {
                Contract.Invariant(LayoutEngine != null, "LayoutEngine not set");

                Debug.WriteLine("Relayouting");
                LayoutEngine.Relayout(Presentation);

                // reLayout requires reRender
                myDrawingElements.Clear();
            }

            this.SnapsToDevicePixels = true;

            if (myDrawingElements.Count == 0)
            {
                Debug.WriteLine("Re-Rendering");

                RemoveVisualChild(myDrawing);

                myDrawing.Children.Clear();

                myDrawing = new DrawingVisual();

                // first draw edges so that in case of overlap with nodes nodes are on top
                foreach (var edge in transformationModule.Graph.Edges)
                {
                    if (!Presentation.Picking.Pick(edge))
                    {
                        continue;
                    }

                    var visual = new EdgeVisual(edge, Presentation, myOldScaling);

                    myDrawingElements.Add(edge.Id, visual);

                    var layoutState = layoutModule.GetLayout(edge);
                    visual.Draw(layoutState);

                    myDrawing.Children.Add(visual.Visual);
                }

                foreach (var node in transformationModule.Graph.Nodes)
                {
                    if (!Presentation.Picking.Pick(node))
                    {
                        continue;
                    }

                    var visual = new NodeVisual(node, Presentation);

                    myDrawingElements.Add(node.Id, visual);

                    var layoutState = layoutModule.GetLayout(node);
                    visual.Draw(layoutState);

                    myDrawing.Children.Add(visual.Visual);
                }

                foreach (var cluster in transformationModule.Graph.Clusters)
                {
                    if (!cluster.Nodes.Any(n => Presentation.Picking.Pick(n)))
                    {
                        continue;
                    }

                    var visual = new ClusterVisual(cluster, Presentation);

                    myDrawingElements.Add(cluster.Id, visual);

                    visual.Draw(myDrawingElements);

                    myDrawing.Children.Insert(0, visual.Visual);
                }

                AddVisualChild(myDrawing);

                var selectionModule = Presentation.GetPropertySetFor<Selection>();
                foreach (var e in selectionModule.Items)
                {
                    AbstractElementVisual drawing;
                    if (myDrawingElements.TryGetValue(e.OwnerId, out drawing))
                    {
                        drawing.Select(e.IsSelected);
                    }
                }

                // clear journals - to avoid considering out-dated infos on next refresh
                myTransformationsJournal.Clear();
                myNodeMaskJournal.Clear();
                mySelectionJournal.Clear();
                myCaptionJournal.Clear();
                myGraphAttributesJournal.Clear();

                InvalidateMeasure();

                RenderingFinished?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                if (!myNodeMaskJournal.IsEmpty)
                {
                    foreach (var node in transformationModule.Graph.Nodes)
                    {
                        if (myDrawingElements.TryGetValue(node.Id, out var visual))
                        {
                            SetVisibility((NodeVisual)visual,
                                Presentation.Picking.Pick(node),
                                v => v.Draw(layoutModule.GetLayout(v.Owner)));
                        }
                    }

                    foreach (var edge in transformationModule.Graph.Edges)
                    {
                        if (myDrawingElements.TryGetValue(edge.Id, out var visual))
                        {
                            SetVisibility((EdgeVisual)visual,
                                Presentation.Picking.Pick(edge),
                                v => v.Draw(layoutModule.GetLayout(v.Owner)));
                        }
                    }

                    // if node visibility changed we should check whether clusters have to be redrawn
                    foreach (var cluster in transformationModule.Graph.Clusters)
                    {
                        if (cluster.Nodes.Any(n => Presentation.Picking.Pick(n)))
                        {
                            if (!myDrawingElements.ContainsKey(cluster.Id))
                            {
                                var visual = new ClusterVisual(cluster, Presentation);

                                myDrawingElements.Add(cluster.Id, visual);

                                visual.Draw(myDrawingElements);

                                myDrawing.Children.Insert(0, visual.Visual);
                            }
                        }
                        else
                        {
                            if (myDrawingElements.TryGetValue(cluster.Id, out var visual))
                            {
                                myDrawingElements.Remove(cluster.Id);

                                myDrawing.Children.Remove(visual.Visual);
                            }
                        }
                    }

                    myNodeMaskJournal.Clear();
                    InvalidateVisual();
                }

                if (!mySelectionJournal.IsEmpty)
                {
                    var selectionModule = Presentation.GetPropertySetFor<Selection>();
                    foreach (var e in mySelectionJournal.Entries)
                    {
                        if (myDrawingElements.TryGetValue(e.OwnerId, out var visual))
                        {
                            // for each change apply new status
                            visual.Select(selectionModule.TryGet(e.OwnerId)?.IsSelected ?? false);
                        }
                    }

                    mySelectionJournal.Clear();
                    InvalidateVisual();
                }

                if (!myCaptionJournal.IsEmpty)
                {
                    foreach (var e in myCaptionJournal.Entries)
                    {
                        if (myDrawingElements.TryGetValue(e.OwnerId, out var visual))
                        {
                            // node captions indirectly change if a cluster is folded then the "cluster node" caption
                            // will change when the cluster gets renamed

                            if (visual is NodeVisual nodeVisual)
                            {
                                // only need to handle visual part of the canvas 
                                if (nodeVisual.Visual != null && myDrawing.Children.Contains(nodeVisual.Visual))
                                {
                                    myDrawing.Children.Remove(nodeVisual.Visual);

                                    nodeVisual.Draw(layoutModule.GetLayout(nodeVisual.Owner));

                                    myDrawing.Children.Add(nodeVisual.Visual);
                                }
                            }
                            else if (visual is ClusterVisual clusterVisual)
                            {
                                // only need to handle visual part of the canvas 
                                if (visual.Visual != null && myDrawing.Children.Contains(visual.Visual))
                                {
                                    myDrawing.Children.Remove(visual.Visual);

                                    clusterVisual.Draw(myDrawingElements);

                                    myDrawing.Children.Add(visual.Visual);
                                }
                            }
                        }
                    }

                    myCaptionJournal.Clear();
                    InvalidateVisual();
                }

                if (Math.Abs(myCurrentScaling - myOldScaling) / myOldScaling > 0.15d)
                {
                    myOldScaling = myCurrentScaling;

                    foreach (var edge in myDrawingElements.Values.OfType<EdgeVisual>())
                    {
                        edge.ApplyZoomFactor(myCurrentScaling);
                    }

                    InvalidateVisual();
                }
            }
        }

        // when modifying the Children collection we have to consider that we could get here because of IsDirty
        // but IsVisible was already toggled multiple times so that actually nothing has to be done
        private void SetVisibility<T>(T visual, bool isVisible, Action<T> drawAction) where T : AbstractElementVisual
        {
            if (isVisible)
            {
                if (visual.Visual == null)
                {
                    drawAction(visual);

                    myDrawing.Children.Add(visual.Visual);
                }
                else
                {
                    if (!myDrawing.Children.Contains(visual.Visual))
                    {
                        myDrawing.Children.Add(visual.Visual);
                    }
                }
            }
            else
            {
                if (myDrawing.Children.Contains(visual.Visual))
                {
                    myDrawing.Children.Remove(visual.Visual);
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
        protected override Visual GetVisualChild(int index)
        {
            return myDrawing;
        }

        public Size ContentSize
        {
            get
            {
                if (myDrawingElements.Count == 0)
                {
                    return new Size(8, 8);
                }

                var bounds = myDrawing.ContentBounds;
                bounds.Union(myDrawing.DescendantBounds);

                return new Size(bounds.Width * 64, bounds.Height * 64);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Rect bounds = myDrawing.ContentBounds;
            bounds.Union(myDrawing.DescendantBounds);

            if (bounds.IsEmpty)
            {
                // if the graph is empty
                return new Size(8, 8);
            }

            // add some extra padding in case the bezier curve is to huge
            bounds.Inflate(bounds.Width * 0.01, bounds.Height * 0.01);

            var m = new Matrix();
            m.Translate(-bounds.Left, -bounds.Top);
            // TODO: why 64?
            m.Scale(64, 64);

            myDrawing.Transform = new MatrixTransform(m);

            return new Size(bounds.Width * 64, bounds.Height * 64);
        }

        public Rect? GetBoundingBox(IGraphItem item)
        {
            AbstractElementVisual visual;
            if (!myDrawingElements.TryGetValue(item.Id, out visual) || visual.Visual == null)
            {
                return null;
            }

            var bounds = visual.Visual.ContentBounds;
            bounds.Scale(64, 64);
            return bounds;
        }

        public IGraphItem PickMousePosition()
        {
            return Pick(Mouse.GetPosition(this));
        }

        public IGraphItem Pick(Point position)
        {
            var result = VisualTreeHelper.HitTest(this, position);
            if (result == null)
            {
                return null;
            }

            var visual = result.VisualHit as DrawingVisual;

            if (visual == null)
            {
                return null;
            }

            return (IGraphItem)visual.ReadLocalValue(AbstractElementVisual.GraphItemProperty);
        }

        public void SetScaling(double scaleX, double scaleY)
        {
            myCurrentScaling = Math.Min(scaleX, scaleY);
        }
    }
}
