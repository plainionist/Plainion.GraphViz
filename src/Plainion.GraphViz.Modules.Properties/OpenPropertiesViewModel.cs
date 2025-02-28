using System;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Prism.Commands;
using Prism.Mvvm;

namespace Plainion.GraphViz.Modules.Properties;

class OpenPropertiesViewModel : BindableBase
{
    private readonly IDomainModel myModel;

    public OpenPropertiesViewModel(IDomainModel model)
    {
        myModel = model;

        myModel.PresentationChanged += OnPresentationChanged;

        OpenPropertiesCommand = new DelegateCommand(OnOpenProperties, () => myModel.Presentation != null);
    }

    private void OnOpenProperties()
    {
        throw new NotImplementedException();
    }

    private void OnPresentationChanged(object sender, System.EventArgs e)
    {
        OpenPropertiesCommand.RaiseCanExecuteChanged();
    }

    public DelegateCommand OpenPropertiesCommand { get; }

}
