using System;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Interactivity.InteractionRequest;
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

        OpenPropertiesCommand = new DelegateCommand(OnOpenProperties);//, () => myModel.Presentation != null);
        PropertiesRequest = new InteractionRequest<INotification>();
    }

    private void OnOpenProperties()
    {
        PropertiesRequest.Raise(new Notification
        {
            Title = "Properties"
        }, c => { });
    }

    private void OnPresentationChanged(object sender, EventArgs e)
    {
        OpenPropertiesCommand.RaiseCanExecuteChanged();
    }

    public DelegateCommand OpenPropertiesCommand { get; }
    public InteractionRequest<INotification> PropertiesRequest { get; private set; }
}
