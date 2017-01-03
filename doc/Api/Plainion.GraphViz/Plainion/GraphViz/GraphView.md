
# Plainion.GraphViz.GraphView

**Namespace:** Plainion.GraphViz

**Assembly:** Plainion.GraphViz

GraphView


## Fields

### System.Windows.DependencyProperty NavigationProperty

### System.Windows.DependencyProperty GraphItemForContextMenuProperty

### System.Windows.DependencyProperty GraphSourceProperty

### System.Windows.DependencyProperty LayoutEngineProperty

### System.Windows.DependencyProperty IsRenderingEnabledProperty


## Constructors

### Constructor()


## Properties

### Plainion.GraphViz.IGraphViewNavigation Navigation

### Plainion.GraphViz.Model.IGraphItem GraphItemForContextMenu

### Plainion.GraphViz.Presentation.IGraphPresentation GraphSource

### Plainion.GraphViz.ILayoutEngine LayoutEngine

### System.Boolean IsRenderingEnabled

### Plainion.GraphViz.Visuals.IVisualPicking Picking

### System.Printing.PageOrientation PreferredOrientation

### System.Collections.Generic.Dictionary`2[[System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.Windows.Controls.ContextMenu, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35]] ContextMenus

Key: name of the type of the element under cursor - "Default" for none


## Methods

### System.Windows.Documents.DocumentPaginator GetPaginator(System.Windows.Size printSize)

### void OnPreviewMouseWheel(System.Windows.Input.MouseWheelEventArgs e)

### void NavigateTo(Plainion.GraphViz.Model.IGraphItem item)

### void InitializeComponent()

InitializeComponent
