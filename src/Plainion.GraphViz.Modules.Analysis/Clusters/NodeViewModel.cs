using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Plainion.GraphViz.Presentation;
using Prism.Mvvm;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

enum NodeType
{
    Root,
    Cluster,
    Node
}

/// <summary>
/// In case of "Root" presentation and Id are null.
/// </summary>
[DebuggerDisplay("{Id}")]
class NodeViewModel : BindableBase
{
    private readonly IGraphPresentation myPresentation;
    private NodeViewModel myParent;
    private bool myIsSelected;
    private string myCaption;
    private bool myShowId;
    private bool myIsExpanded;
    private bool myIsFilteredOut;

    public NodeViewModel(IGraphPresentation presentation, string id, NodeType type)
    {
        myPresentation = presentation;
        Type = type;
        Id = id;
    }

    public NodeType Type { get; }
    public string Id { get; }

    public string Caption
    {
        get { return myCaption; }
        set
        {
            if (SetProperty(ref myCaption, value))
            {
                RaisePropertyChanged(nameof(DisplayText));
                if (myPresentation != null)
                {
                    myPresentation.GetPropertySetFor<Caption>().Get(Id).DisplayText = myCaption;
                }
            }
        }
    }

    public bool ShowId
    {
        get { return myShowId; }
        set
        {
            if (SetProperty(ref myShowId, value))
            {
                RaisePropertyChanged(nameof(DisplayText));
            }
        }
    }

    public string DisplayText => ShowId ? Id : Caption;

    public bool IsSelected
    {
        get { return myIsSelected; }
        set { SetProperty(ref myIsSelected, value); }
    }

    public ObservableCollection<NodeViewModel> Children { get; } = [];

    public NodeViewModel Parent
    {
        get { return myParent; }
        set { SetProperty(ref myParent, value); }
    }

    public bool Matches(string pattern)
    {
        if (pattern == "*")
        {
            return Caption != null;
        }

        return Caption != null && Caption.Contains(pattern, StringComparison.OrdinalIgnoreCase)
            || Id != null && Id.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }

    public bool IsExpanded
    {
        get { return myIsExpanded; }
        set { SetProperty(ref myIsExpanded, value); }
    }

    public bool IsFilteredOut
    {
        get { return myIsFilteredOut; }
        set { SetProperty(ref myIsFilteredOut, value); }
    }

    public void ApplyFilter(string filter)
    {
        string[] tokens = null;

        if (filter == null)
        {
            IsFilteredOut = false;
        }
        else
        {
            // if this has no parent it is Root - no need to filter root
            if (Parent != null)
            {
                tokens = filter.Split('/');
                var levelFilter = tokens.Length == 1 ? filter : tokens[GetDepth()];
                if (string.IsNullOrWhiteSpace(levelFilter))
                {
                    IsFilteredOut = false;
                }
                else
                {
                    IsFilteredOut = !Matches(levelFilter);
                }
            }
        }

        foreach (var child in Children)
        {
            child.ApplyFilter(filter);

            if (!child.IsFilteredOut && tokens != null && tokens.Length == 1)
            {
                IsFilteredOut = false;
            }
        }
    }

    private int GetDepth()
    {
        int depth = 0;

        var parent = Parent;
        while (parent != null)
        {
            parent = parent.Parent;
            depth++;
        }

        // ignore invisible root
        return depth - 1;
    }

    public void ExpandAll()
    {
        IsExpanded = true;

        foreach (var child in Children)
        {
            child.ExpandAll();
        }
    }

    public void CollapseAll()
    {
        IsExpanded = false;

        foreach (var child in Children)
        {
            child.CollapseAll();
        }
    }
}
