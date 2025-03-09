using System;
using System.Linq;
using Plainion.Graphs;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Viewer.Abstractions;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Interactivity.InteractionRequest;
using Plainion.Windows.Mvvm;
using Prism.Events;

namespace Plainion.GraphViz.Modules.Metrics;

class MetricsViewModel : ViewModelBase, IInteractionRequestAware
{
    private readonly IEventAggregator myEventAggregator;
    private Action myFinishAction;
    private IModuleChangedObserver myNodeMaskObserver;
    private IModuleChangedObserver myTransformationsObserver;

    public MetricsViewModel(IDomainModel model, IEventAggregator eventAggregator)
         : base(model)
    {
        myEventAggregator = eventAggregator;

        Metrics = new MetricsComputationViewModel();
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

    public MetricsComputationViewModel Metrics { get; }

    protected override void OnPresentationChanged()
    {
        if (myNodeMaskObserver != null)
        {
            myNodeMaskObserver.ModuleChanged -= OnNodeMasksChanged;
            myNodeMaskObserver.Dispose();
            myNodeMaskObserver = null;
        }

        if (myTransformationsObserver != null)
        {
            myTransformationsObserver.ModuleChanged -= OnTransformationsChanged;
            myTransformationsObserver.Dispose();
            myTransformationsObserver = null;
        }

        if (Model.Presentation == null)
        {
            return;
        }

        myNodeMaskObserver = Model.Presentation.GetModule<INodeMaskModule>().CreateObserver();
        myNodeMaskObserver.ModuleChanged += OnNodeMasksChanged;

        myTransformationsObserver = Model.Presentation.GetModule<ITransformationModule>().CreateObserver();
        myTransformationsObserver.ModuleChanged += OnTransformationsChanged;

        RestartAnalysis();
    }

    private void RestartAnalysis()
    {
        if (myFinishAction == null)
        {
            // dialog not open
            return;
        }

        Metrics.RestartAnalysis(Model.Presentation);
    }

    private void OnTransformationsChanged(object sender, EventArgs e)
    {
        RestartAnalysis();
    }

    private void OnNodeMasksChanged(object sender, EventArgs e)
    {
        RestartAnalysis();
    }

    public INotification Notification { get; set; }

    public Action FinishInteraction
    {
        get { return myFinishAction; }
        set
        {
            if (SetProperty(ref myFinishAction, value))
            {
                TriggerAnalysis();
            }
        }
    }

    private void TriggerAnalysis()
    {
        if (myFinishAction == null)
        {
            // dialog not open
            return;
        }

        Metrics.TriggerAnalysis(Model.Presentation);
    }

    internal void Closed()
    {
        myFinishAction = null;
    }
}

