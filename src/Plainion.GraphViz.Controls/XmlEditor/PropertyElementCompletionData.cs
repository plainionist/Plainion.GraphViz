using System.Reflection;

namespace Plainion.GraphViz.Controls.XmlEditor;

class PropertyElementCompletionData : AbstractCompletionData
{
    public PropertyElementCompletionData(PropertyInfo property)
        : base(property.Name, property.PropertyType.Name)
    {
    }
}
