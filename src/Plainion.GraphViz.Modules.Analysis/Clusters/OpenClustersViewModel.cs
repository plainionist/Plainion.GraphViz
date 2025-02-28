using System;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Interactivity.InteractionRequest;
using Prism.Commands;
using Prism.Mvvm;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

class OpenClustersViewModel : BindableBase
{
    private readonly IDomainModel myModel;

    public OpenClustersViewModel(IDomainModel model)
    {
        myModel = model;

        myModel.PresentationChanged += OnPresentationChanged;

        OpenClusterEditorCommand = new DelegateCommand(OnOpenClusterEditor, () => myModel.Presentation != null);
        ClusterEditorRequest = new InteractionRequest<INotification>();
    }

    public DelegateCommand OpenClusterEditorCommand { get; private set; }

    private void OnOpenClusterEditor()
    {
        var notification = new Notification();
        notification.Title = "Clusters";

        ClusterEditorRequest.Raise(notification, c => { });
    }

    public InteractionRequest<INotification> ClusterEditorRequest { get; private set; }

    private void OnPresentationChanged(object sender, EventArgs e)
    {
        OpenClusterEditorCommand.RaiseCanExecuteChanged();
    }
}
