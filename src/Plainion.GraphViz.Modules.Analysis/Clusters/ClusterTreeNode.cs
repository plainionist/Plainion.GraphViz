using System;
using System.Collections.ObjectModel;
using Plainion.GraphViz.Presentation;
using Plainion.Windows.Interactivity.DragDrop;
using Prism.Mvvm;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

class ClusterTreeNode : BindableBase
{
    private readonly IGraphPresentation myPresentation;
    private ClusterTreeNode myParent;
    private bool myIsSelected;
    private string myCaption;
    private bool myShowId;
    private bool myIsExpanded;
    private bool myIsFilteredOut;

    public ClusterTreeNode(IGraphPresentation presentation)
    {
        myPresentation = presentation;

        Children = [];

        IsDropAllowed = true;
        IsDragAllowed = true;
    }

    public string Id { get; set; }

    public string Caption
    {
        get { return myCaption; }
        set
        {
            if (SetProperty(ref myCaption, value))
            {
                RaisePropertyChanged(nameof(DisplayText));
                myPresentation.GetPropertySetFor<Caption>().Get(Id).DisplayText = myCaption;
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

    public string DisplayText
    {
        get { return ShowId ? Id : Caption; }
    }

    public bool IsSelected
    {
        get { return myIsSelected; }
        set { SetProperty(ref myIsSelected, value); }
    }

    public bool IsDragAllowed { get; set; }

    public bool IsDropAllowed { get; set; }

    public ObservableCollection<ClusterTreeNode> Children { get; }

    public ClusterTreeNode Parent
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

    internal bool IsDropAllowedAt(DropLocation location)
    {
        if (location == DropLocation.InPlace)
        {
            return IsDropAllowed;
        }
        else
        {
            if (Parent != null)
            {
                return Parent.IsDropAllowed;
            }
        }

        return true;
    }
}
