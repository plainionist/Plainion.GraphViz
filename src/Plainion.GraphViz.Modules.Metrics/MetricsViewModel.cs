using System;
using System.Windows.Input;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Interactivity.InteractionRequest;
using Prism.Commands;

namespace Plainion.GraphViz.Modules.Metrics;

class MetricsViewModel : ViewModelBase, IInteractionRequestAware
{
    public MetricsViewModel(IDomainModel model)
         : base(model)
    {
        OkCommand = new DelegateCommand(OnOk);
    }

    public ICommand OkCommand { get; }

    private void OnOk()
    {
        FinishInteraction();
    }

    protected override void OnPresentationChanged()
    {
    }

    public Action FinishInteraction { get; set; }

    public INotification Notification { get; set; }
}

