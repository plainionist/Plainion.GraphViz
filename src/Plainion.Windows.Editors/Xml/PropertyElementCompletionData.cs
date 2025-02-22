using System;
using System.Reflection;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Plainion.Windows.Editors.Xml
{
    class PropertyElementCompletionData : AbstractCompletionData
    {
        public PropertyElementCompletionData( PropertyInfo property )
            : base(property.Name, property.PropertyType.Name)
        {
        }
    }
}
