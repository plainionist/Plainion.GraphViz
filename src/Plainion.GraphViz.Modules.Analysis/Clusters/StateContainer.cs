using System.Collections.Generic;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

/// <summary>
/// used to store additional state to the actual INode model.
/// we cannot store state in NodeItem directly as those instances lifecylced ItemContainerGenerator.
/// esp. with virtualization enabled those items might be created on demand and destroyed frequently.
/// </summary>
class StateContainer
{
    private readonly Dictionary<object, NodeState> myStates = [];
    private ClusterTreeNode myDataContext;

    public ClusterTreeNode DataContext
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

    public NodeState GetOrCreate(ClusterTreeNode dataContext)
    {
        if (!myStates.TryGetValue(dataContext, out var state))
        {
            state = new NodeState(dataContext, this);
            myStates[dataContext] = state;
        }
        return state;
    }
}
