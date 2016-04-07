using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    [Serializable]
    public class NodeMask : AbstractNodeMask
    {
        private readonly HashSet<string> myValues;

        public NodeMask()
        {
            myValues = new HashSet<string>();
            IsApplied = true;
            IsShowMask = true;
        }

        public NodeMask(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            var count = (int)info.GetValue("ValueCount", typeof(int));

            myValues = new HashSet<string>();

            for (int i = 0; i < count; ++i)
            {
                myValues.Add((string)info.GetValue("Value" + i, typeof(string)));
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("ValueCount", myValues.Count);

            int i = 0;
            foreach (var value in myValues)
            {
                info.AddValue("Value" + i, value);
                ++i;
            }
        }

        public IEnumerable<string> Values
        {
            get { return myValues; }
        }

        public override bool? IsSet(Node node)
        {
            if (myValues.Contains(node.Id))
            {
                return IsShowMask;
            }
            else
            {
                return null;
            }
        }

        public void Set(Node node)
        {
            if (myValues.Contains(node.Id))
            {
                return;
            }

            myValues.Add(node.Id);

            RaisePropertyChanged("Values");
        }

        public void Unset(Node node)
        {
            myValues.Remove(node.Id);

            RaisePropertyChanged("Values");
        }

        public void Set(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                if (!myValues.Contains(node.Id))
                {
                    myValues.Add(node.Id);
                }
            }

            RaisePropertyChanged("Values");
        }
    }
}
