using System.Linq;
using Plainion.Graphs;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Viewer.Abstractions;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Windows.Mvvm;
using Prism.Events;

namespace Plainion.GraphViz.Modules.Metrics;

class HighlightViewModel : ViewModelBase
{
    private readonly IEventAggregator myEventAggregator;

    public HighlightViewModel(IDomainModel model, IEventAggregator eventAggregator)
         : base(model)
    {
        myEventAggregator = eventAggregator;

        HighlightCommand = new DelegateCommand<object>(OnHighlight);
    }

    private void OnHighlight(object item)
    {
        if (item is NodeDegreesVM nodeDegreesVM)
        {
            var selection = Model.Presentation.GetPropertySetFor<Selection>();
            selection.Clear();
            selection.Get(nodeDegreesVM.Model.Id).IsSelected = true;

            myEventAggregator.GetEvent<NodeFocusedEvent>().Publish(nodeDegreesVM.Model);
        }
        else if (item is CycleVM cycleVM)
        {
            var selection = Model.Presentation.GetPropertySetFor<Selection>();
            selection.Clear();
            foreach (var edge in cycleVM.Model.Edges)
            {
                selection.Select(edge);
            }

            myEventAggregator.GetEvent<NodeFocusedEvent>().Publish(cycleVM.Model.Start);
        }
        else if (item is GraphItemMeasurementVM itemMeasurementVM)
        {
            var selection = Model.Presentation.GetPropertySetFor<Selection>();
            selection.Clear();

            if (itemMeasurementVM.Model is Node node)
            {
                selection.Get(node.Id).IsSelected = true;
                myEventAggregator.GetEvent<NodeFocusedEvent>().Publish(node);
            }
            else if (itemMeasurementVM.Model is Graphs.Undirected.Node uNode)
            {
                selection.Get(uNode.Id).IsSelected = true;
                myEventAggregator.GetEvent<NodeFocusedEvent>().Publish(uNode);
            }
            else if (itemMeasurementVM.Model is Edge edge)
            {
                selection.Select(edge);

                myEventAggregator.GetEvent<NodeFocusedEvent>().Publish(edge.Source);
            }
            else
            {
                // intentionally ignore
            }
        }
    }

    public DelegateCommand<object> HighlightCommand { get; set; }
}
