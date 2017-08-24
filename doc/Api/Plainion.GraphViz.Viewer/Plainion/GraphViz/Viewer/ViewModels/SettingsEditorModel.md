
# Plainion.GraphViz.Viewer.ViewModels.SettingsEditorModel

**Namespace:** Plainion.GraphViz.Viewer.ViewModels

**Assembly:** Plainion.GraphViz.Viewer


## Constructors

### Constructor(Plainion.GraphViz.Viewer.Services.ConfigurationService configService)


## Properties

### System.Windows.Input.ICommand MouseDownCommand

### System.Windows.Input.ICommand RemoveConversionStepCommand

### Prism.Commands.DelegateCommand AddConversionCommand

### System.String MatchingText

### System.String ReplacementText

### Plainion.GraphViz.Presentation.ILabelConversionStep SelectedConversionStep

### System.Windows.Input.ICommand CancelCommand

### System.Windows.Input.ICommand OkCommand

### Plainion.GraphViz.Viewer.Configuration.Config Config

### System.Collections.ObjectModel.ObservableCollection`1[[Plainion.GraphViz.Presentation.ILabelConversionStep, Plainion.GraphViz, Version=2.1.0.0, Culture=neutral, PublicKeyToken=null]] ConversionSteps

### System.Collections.ObjectModel.ObservableCollection`1[[Plainion.GraphViz.Viewer.ViewModels.LabelViewModel, Plainion.GraphViz.Viewer, Version=2.1.0.0, Culture=neutral, PublicKeyToken=null]] Labels

### Plainion.GraphViz.Presentation.ILabelConversionStep TemporalConversionStep

### Plainion.GraphViz.Presentation.IGraphPresentation Presentation

### System.Action FinishInteraction

### Prism.Interactivity.InteractionRequest.INotification Notification


## Methods

### void RemoveConversionStep(Plainion.GraphViz.Presentation.ILabelConversionStep step)

### void OnModelPropertyChanged(System.String propertyName)
