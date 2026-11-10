using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Threading;
using Plainion.GraphViz.Presentation;
using Plainion.GraphViz.Viewer.Abstractions.ViewModel;
using Plainion.Prism.Mvvm;
using Prism.Commands;

namespace Plainion.GraphViz.Modules.Analysis.NodeSelection;

class NodeSelectionEditorModel : ViewModelBase
{
    private readonly Dispatcher myDispatcher = Dispatcher.CurrentDispatcher;
    private IGraphPresentation myPresentation;
    private IModuleChangedObserver myCaptionsObserver;
    private IModuleChangedObserver mySelectionObserver;
    private IReadOnlyList<NodeWithCaption> myMatchedNodes = [];
    private string myPattern;
    private bool myMatchOnIdentifier;
    private bool myShowSelectedNodesOnly;
    private bool myPreviewRefreshPending;
    private NodeWithCaption mySelectedPreviewItem;

    public NodeSelectionEditorModel(IDomainModel model)
        : base(model)
    {
        SelectCommand = new DelegateCommand(() => SetSelection(true), CanApplyPattern);
        DeselectCommand = new DelegateCommand(() => SetSelection(false), CanApplyPattern);
        MouseDownCommand = new DelegateCommand<MouseButtonEventArgs>(OnMouseDown);

        OnPresentationChanged();
    }

    public string Pattern
    {
        get { return myPattern; }
        set
        {
            if (SetProperty(ref myPattern, value))
            {
                RefreshPreview();
            }
        }
    }

    public bool MatchOnIdentifier
    {
        get { return myMatchOnIdentifier; }
        set
        {
            if (SetProperty(ref myMatchOnIdentifier, value))
            {
                RefreshPreview();
            }
        }
    }

    public bool ShowSelectedNodesOnly
    {
        get { return myShowSelectedNodesOnly; }
        set
        {
            if (SetProperty(ref myShowSelectedNodesOnly, value))
            {
                RefreshPreview();
            }
        }
    }

    public IReadOnlyList<NodeWithCaption> PreviewNodes { get; private set; } = [];

    public string ValidationError { get; private set; }

    public DelegateCommand SelectCommand { get; }

    public DelegateCommand DeselectCommand { get; }

    public DelegateCommand<MouseButtonEventArgs> MouseDownCommand { get; }

    public NodeWithCaption SelectedPreviewItem
    {
        get { return mySelectedPreviewItem; }
        set { SetProperty(ref mySelectedPreviewItem, value); }
    }

    protected override void OnPresentationChanged()
    {
        if (myPresentation == Model.Presentation)
        {
            return;
        }

        if (myPresentation != null)
        {
            myPresentation.GraphVisibilityChanged -= OnGraphChanged;
            myCaptionsObserver.ModuleChanged -= OnGraphChanged;
            myCaptionsObserver.Dispose();
            myCaptionsObserver = null;
            mySelectionObserver.ModuleChanged -= OnSelectionChanged;
            mySelectionObserver.Dispose();
            mySelectionObserver = null;
        }

        myPresentation = Model.Presentation;

        if (myPresentation != null)
        {
            myPresentation.GraphVisibilityChanged += OnGraphChanged;
            myCaptionsObserver = myPresentation.GetPropertySetFor<Caption>().CreateObserver();
            myCaptionsObserver.ModuleChanged += OnGraphChanged;
            mySelectionObserver = myPresentation.GetPropertySetFor<Selection>().CreateObserver();
            mySelectionObserver.ModuleChanged += OnSelectionChanged;
        }

        myPattern = null;
        RaisePropertyChanged(nameof(Pattern));
        RefreshPreview();
    }

    private void OnGraphChanged(object sender, EventArgs e)
    {
        if (myPreviewRefreshPending)
        {
            return;
        }

        myPreviewRefreshPending = true;
        myDispatcher.BeginInvoke(new Action(() =>
        {
            myPreviewRefreshPending = false;
            RefreshPreview();
        }));
    }

    private void OnSelectionChanged(object sender, EventArgs e)
    {
        if (ShowSelectedNodesOnly)
        {
            OnGraphChanged(sender, e);
        }
    }

    private void RefreshPreview()
    {
        ClearErrors(nameof(Pattern));
        ValidationError = null;
        myMatchedNodes = [];
        PreviewNodes = [];

        try
        {
            var regex = string.IsNullOrEmpty(Pattern)
                ? null
                : new Regex(Pattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

            if (myPresentation != null)
            {
                var captions = myPresentation.GetPropertySetFor<Caption>();
                myMatchedNodes = myPresentation.TransformedGraph.Nodes
                    .Where(myPresentation.Picking.Pick)
                    .Select(n => new NodeWithCaption(n, MatchOnIdentifier ? n.Id : captions.Get(n.Id).DisplayText))
                    .Where(n => regex == null || regex.IsMatch(n.DisplayText))
                    .OrderBy(n => n.DisplayText)
                    .ToList();

                var selections = myPresentation.GetPropertySetFor<Selection>();
                PreviewNodes = ShowSelectedNodesOnly
                    ? myMatchedNodes.Where(n => selections.TryGet(n.Node.Id)?.IsSelected == true).ToList()
                    : myMatchedNodes;
            }
        }
        catch (RegexParseException ex)
        {
            SetPatternError(ex.Message);
        }
        catch (RegexMatchTimeoutException)
        {
            SetPatternError("Pattern matching timed out. Please simplify the regular expression.");
        }

        SelectedPreviewItem = null;
        RaisePropertyChanged(nameof(PreviewNodes));
        RaisePropertyChanged(nameof(ValidationError));
        SelectCommand.RaiseCanExecuteChanged();
        DeselectCommand.RaiseCanExecuteChanged();
    }

    private void SetPatternError(string message)
    {
        ValidationError = message;
        SetError(ValidationFailure.Error, nameof(Pattern));
    }

    private bool CanApplyPattern()
    {
        return myPresentation != null
            && !string.IsNullOrEmpty(Pattern)
            && ValidationError == null
            && myMatchedNodes.Count > 0;
    }

    private void SetSelection(bool isSelected)
    {
        RefreshPreview();

        if (!CanApplyPattern())
        {
            return;
        }

        var selections = myPresentation.GetPropertySetFor<Selection>();
        foreach (var node in myMatchedNodes)
        {
            var selection = isSelected ? selections.Get(node.Node.Id) : selections.TryGet(node.Node.Id);
            if (selection != null)
            {
                selection.IsSelected = isSelected;
            }
        }

        Pattern = null;
    }

    private void OnMouseDown(MouseButtonEventArgs args)
    {
        if (args.ClickCount == 2 && SelectedPreviewItem?.DisplayText is string text)
        {
            Pattern = Regex.Escape(text);
        }
    }
}
