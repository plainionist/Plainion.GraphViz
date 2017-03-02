
namespace Plainion.GraphViz.Presentation
{
    public interface IModuleRepository
    {
        T GetModule<T>();

        IPropertySetModule<T> GetPropertySetFor<T>() where T : AbstractPropertySet;
    }
}
