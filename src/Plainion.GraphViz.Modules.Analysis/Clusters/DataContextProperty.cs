
namespace Plainion.GraphViz.Modules.Analysis.Clusters;

class DataContextProperty<T>
{
    private string myPropertyName;

    public DataContextProperty(string propertyName)
    {
        myPropertyName = propertyName;
    }

    public T Get(ClusterTreeNode dataContext)
    {
        var prop = dataContext.GetType().GetProperty(myPropertyName);
        return (T)prop.GetValue(dataContext);
    }

    public void Set(ClusterTreeNode dataContext, T value)
    {
        var prop = dataContext.GetType().GetProperty(myPropertyName);
        prop.SetValue(dataContext, value);
    }
}
