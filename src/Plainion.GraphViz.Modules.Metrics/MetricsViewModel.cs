using System;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Interactivity.InteractionRequest;

namespace Plainion.GraphViz.Modules.Metrics;

class MetricsViewModel : ViewModelBase, IInteractionRequestAware
{
    public MetricsViewModel(IDomainModel model)
         : base(model)
    {
    }

    protected override void OnPresentationChanged()
    {
    }

    public Action FinishInteraction { get; set; }

    public INotification Notification { get; set; }
}

