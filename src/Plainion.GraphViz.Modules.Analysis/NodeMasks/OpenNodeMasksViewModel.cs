using System;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Interactivity.InteractionRequest;
using Prism.Commands;
using Prism.Mvvm;

namespace Plainion.GraphViz.Modules.Analysis.NodeMasks;

class OpenNodeMasksViewModel : BindableBase
{
    private readonly IDomainModel myModel;

    public OpenNodeMasksViewModel(IDomainModel model)
    {
        myModel = model;

        myModel.PresentationChanged += OnPresentationChanged;

        OpenNodeMasksEditorCommand = new DelegateCommand(OnOpenFilterEditor, () => myModel.Presentation != null);
        NodeMasksEditorRequest = new InteractionRequest<INotification>();
    }

    public DelegateCommand OpenNodeMasksEditorCommand { get; private set; }

    private void OnOpenFilterEditor()
    {
        var notification = new Notification();
        notification.Title = "Filters";

        NodeMasksEditorRequest.Raise(notification, c => { });
    }

    public InteractionRequest<INotification> NodeMasksEditorRequest { get; private set; }

    private void OnPresentationChanged(object sender, EventArgs e)
    {
        OpenNodeMasksEditorCommand.RaiseCanExecuteChanged();
    }
}
