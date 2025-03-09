using System;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Interactivity.InteractionRequest;

namespace Plainion.GraphViz.Modules.Metrics;

class MetricsViewModel : ViewModelBase, IInteractionRequestAware
{
    private Action myFinishAction;
    private IModuleChangedObserver myNodeMaskObserver;
    private IModuleChangedObserver myTransformationsObserver;

    public MetricsViewModel(IDomainModel model, HighlightViewModel highlightViewModel)
         : base(model)
    {
        Highlighting = highlightViewModel;
        Metrics = new MetricsComputationViewModel();
    }

    public HighlightViewModel Highlighting { get; set; }

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

