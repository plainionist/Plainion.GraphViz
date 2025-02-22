using System;
using System.Reflection;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Plainion.Windows.Editors.Xml
{
    class AttributeCompletionData : AbstractCompletionData
    {
        public AttributeCompletionData(PropertyInfo property)
            : base(property.Name, property.PropertyType.Name)
        {
        }

        public override void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text + "=\"\"");
            textArea.Caret.Offset -= 1;
        }
    }

}
