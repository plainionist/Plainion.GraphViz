using System;
using System.Collections.ObjectModel;
using Plainion.GraphViz.Presentation;
using Prism.Mvvm;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

class ClusterTreeNode : BindableBase
{
    private readonly IGraphPresentation myPresentation;
    private ClusterTreeNode myParent;
    private bool myIsSelected;
    private string myCaption;
    private bool myShowId;

    public ClusterTreeNode(IGraphPresentation presentation)
    {
        myPresentation = presentation;

        Children = new ObservableCollection<ClusterTreeNode>();

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

    public ObservableCollection<ClusterTreeNode> Children { get; private set; }

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
}
