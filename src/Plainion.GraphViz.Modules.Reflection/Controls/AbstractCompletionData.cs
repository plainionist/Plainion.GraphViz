using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace Plainion.GraphViz.Modules.Reflection.Controls
{
    public abstract class AbstractCompletionData : ICompletionData
    {
        protected AbstractCompletionData(string name, string description)
        {
            Text = name;
            Description = description;
        }

        public ImageSource Image { get { return null; } }

        public string Text { get; private set; }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content { get { return Text; } }

        public object Description { get; private set; }

        public virtual void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }

        public double Priority { get { return 0; } }
    }
}
