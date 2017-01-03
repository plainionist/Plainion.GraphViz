
# Plainion.GraphViz.Visuals.GraphVisual

**Namespace:** Plainion.GraphViz.Visuals

**Assembly:** Plainion.GraphViz


## Constructors

### Constructor()


## Properties

### Plainion.GraphViz.Presentation.IGraphPresentation Presentation

### Plainion.GraphViz.ILayoutEngine LayoutEngine

### System.Int32 VisualChildrenCount

### System.Windows.Size ContentSize


## Events

### System.EventHandler RenderingFinished


## Methods

### void Refresh()

### System.Windows.Media.Visual GetVisualChild(System.Int32 index)

### System.Windows.Size MeasureOverride(System.Windows.Size availableSize)

### System.Nullable`1[[System.Windows.Rect, WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35]] GetBoundingBox(Plainion.GraphViz.Model.IGraphItem item)

### Plainion.GraphViz.Model.IGraphItem PickMousePosition()

### Plainion.GraphViz.Model.IGraphItem Pick(System.Windows.Point position)

### void SetScaling(System.Double scaleX,System.Double scaleY)
