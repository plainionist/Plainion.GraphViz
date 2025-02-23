namespace Plainion.GraphViz.Modules.Analysis.Clusters;

/// <summary>
/// used to store additional state to the actual INode model.
/// we cannot store state in NodeItem directly as those instances lifecylced ItemContainerGenerator.
/// esp. with virtualization enabled those items might be created on demand and destroyed frequently.
/// </summary>
class StateContainer
{
    private ClusterTreeNode myDataContext;

    public ClusterTreeNode DataContext
    {
        get { return myDataContext; }
        set
        {
            if (myDataContext != value)
            {
                myDataContext = value;
            }
        }
    }

    public ClusterTreeNode GetRoot() => DataContext;

    public ClusterTreeNode GetOrCreate(ClusterTreeNode dataContext) => dataContext;
}
