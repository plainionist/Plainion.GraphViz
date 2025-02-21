using System;

namespace Plainion.GraphViz.Viewer.Abstractions
{
    [Serializable]
    public class FailedItem
    {
        public FailedItem( string item, string reason )
        {
            Item = item;
            FailureReason = reason;
        }

        public string Item { get; private set; }

        public string FailureReason { get; private set; }
    }
}
