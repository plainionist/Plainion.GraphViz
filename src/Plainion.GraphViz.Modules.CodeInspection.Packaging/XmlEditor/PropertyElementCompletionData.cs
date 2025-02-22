using System.Reflection;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.XmlEditor;

class PropertyElementCompletionData : AbstractCompletionData
{
    public PropertyElementCompletionData(PropertyInfo property)
        : base(property.Name, property.PropertyType.Name)
    {
    }
}
