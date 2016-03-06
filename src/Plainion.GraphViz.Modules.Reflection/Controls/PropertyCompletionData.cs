using System;
using System.Reflection;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Plainion.GraphViz.Modules.Reflection.Controls
{
    class PropertyCompletionData : AbstractCompletionData
    {
        public PropertyCompletionData(PropertyInfo property)
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
