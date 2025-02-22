using System.Collections.Generic;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

/// <summary>
/// used to store additional state to the actual INode model.
/// we cannot store state in NodeItem directly as those instances lifecylced ItemContainerGenerator.
/// esp. with virtualization enabled those items might be created on demand and destroyed frequently.
/// </summary>
class StateContainer
{
    private readonly Dictionary<object, NodeState> myStates;
    private INode myDataContext;

    public StateContainer()
    {
        myStates = new Dictionary<object, NodeState>();
    }

    public INode DataContext
    {
        get { return myDataContext; }
        set
        {
            if (myDataContext != value)
            {
                myDataContext = value;
                myStates.Clear();
            }
        }
    }

    public NodeState GetRoot()
    {
        return GetOrCreate(DataContext);
    }

    public NodeState GetOrCreate(INode dataContext)
    {
        NodeState state;
        if (!myStates.TryGetValue(dataContext, out state))
        {
            state = new NodeState(dataContext, this);
            myStates[dataContext] = state;
        }
        return state;
    }

    // used to avoid updates from NodeItem to NodeState while recursively updating NodeStates
    public bool IsCheckedPropagationRunning { get; set; }

    public DataContextProperty<bool?> IsCheckedProperty { get; set; }

    public bool ShowChildrenCount { get; set; }
}
