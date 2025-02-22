using System;

namespace Plainion.GraphViz.Controls.XmlEditor;

public class ElementCompletionData : AbstractCompletionData
{
    public ElementCompletionData(Type type)
        : base(type.Name, type.Name)
    {
        Type = type;
    }

    public Type Type { get; private set; }
}
