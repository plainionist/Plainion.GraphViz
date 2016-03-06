using System;

namespace Plainion.GraphViz.Modules.Reflection.Controls
{
    public class KeywordCompletionData : AbstractCompletionData
    {
        public KeywordCompletionData(Type type)
            : base(type.Name, type.Name)
        {
            Type = type;
        }

        public Type Type { get; private set; }
    }
}
