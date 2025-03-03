using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    public ObservableCollection<KeyValuePair<string, string>> KeyValuePairs { get; } =
           new ObservableCollection<KeyValuePair<string, string>>
           {
            new KeyValuePair<string, string>("Name", "Alice"),
            new KeyValuePair<string, string>("Age", "30"),
            new KeyValuePair<string, string>("City", "Berlin")
           };
}

