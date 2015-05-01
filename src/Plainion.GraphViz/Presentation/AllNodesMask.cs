using System;
using System.Runtime.Serialization;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    [Serializable]
    public class AllNodesMask : AbstractNodeMask
    {
        public AllNodesMask()
        {
            IsApplied = false;
            IsShowMask = true;
            Label = "All nodes";
        }

        public AllNodesMask( SerializationInfo info, StreamingContext context )
            : base( info, context )
        {
        }

        public override bool? IsSet( Node node )
        {
            return IsShowMask;
        }
    }
}
