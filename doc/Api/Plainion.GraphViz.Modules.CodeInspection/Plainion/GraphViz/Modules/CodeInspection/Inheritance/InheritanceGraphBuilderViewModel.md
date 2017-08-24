
# Plainion.GraphViz.Modules.CodeInspection.Inheritance.InheritanceGraphBuilderViewModel

**Namespace:** Plainion.GraphViz.Modules.CodeInspection.Inheritance

**Assembly:** Plainion.GraphViz.Modules.CodeInspection


## Constructors

### Constructor()


## Properties

### System.Boolean IsReady

### Plainion.GraphViz.Infrastructure.Services.IPresentationCreationService PresentationCreationService

### Plainion.GraphViz.Infrastructure.Services.IStatusMessageService StatusMessageService

### Plainion.GraphViz.Modules.CodeInspection.Inheritance.Services.AssemblyInspectionService InspectionService

### Prism.Commands.DelegateCommand CreateGraphCommand

### Prism.Commands.DelegateCommand AddToGraphCommand

### Prism.Commands.DelegateCommand CancelCommand

### Prism.Commands.DelegateCommand BrowseAssemblyCommand

### Prism.Commands.DelegateCommand ClosedCommand

### Prism.Interactivity.InteractionRequest.InteractionRequest`1[[Plainion.Prism.Interactivity.InteractionRequest.OpenFileDialogNotification, Plainion.Prism, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null]] OpenFileRequest

### System.Windows.Controls.AutoCompleteFilterPredicate`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] TypeFilter

### System.String AssemblyToAnalyseLocation

### System.Collections.ObjectModel.ObservableCollection`1[[Plainion.GraphViz.Modules.CodeInspection.Inheritance.Services.Framework.TypeDescriptor, Plainion.GraphViz.Modules.CodeInspection, Version=2.1.0.0, Culture=neutral, PublicKeyToken=null]] Types

### Plainion.GraphViz.Modules.CodeInspection.Inheritance.Services.Framework.TypeDescriptor TypeToAnalyse

### System.Boolean IgnoreDotNetTypes

### System.Int32 ProgressValue


## Methods

### void OnModelPropertyChanged(System.String propertyName)
