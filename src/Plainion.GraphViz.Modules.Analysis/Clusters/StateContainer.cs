namespace Plainion.GraphViz.Modules.Analysis.Clusters;

class StateContainer
{
    private NodeViewModel myDataContext;

    public NodeViewModel DataContext
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
}
