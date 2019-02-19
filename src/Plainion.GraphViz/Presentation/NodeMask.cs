using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    // DESIGN-HINT: masks are "delta" mask by design, means: you have to set nodes explicitly from outside.
    // For every "unknown"node the mask returns "don't know". This design works well with "add"/"remove" nodes to/from
    // graph and especially well with folding!
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

        public NodeMask(IEnumerable<string> nodes)
        {
            myValues = new HashSet<string>(nodes);
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

            OnPropertyChanged(nameof(Values));
        }

        public void Unset(Node node)
        {
            myValues.Remove(node.Id);

            OnPropertyChanged(nameof(Values));
        }

        public void Set(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                myValues.Add(node.Id);
            }

            OnPropertyChanged(nameof(Values));
        }

        public override void Invert(IGraphPresentation presentation)
        {
            var graph = presentation.GetModule<ITransformationModule>().Graph;

            var inversion = graph.Nodes
                .Where(n => IsShowMask ? !presentation.Picking.Pick(n) : presentation.Picking.Pick(n))
                .Where(n => !myValues.Contains(n.Id))
                .ToList();

            myValues.Clear();

            foreach (var n in inversion)
            {
                myValues.Add(n.Id);
            }

            OnPropertyChanged(nameof(Values));

            Label = $"All but '{Label}'";
        }
    }
}
