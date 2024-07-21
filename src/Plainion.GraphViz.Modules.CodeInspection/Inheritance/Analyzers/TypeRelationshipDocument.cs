using System;
using System.Collections.Generic;
using MessagePack;
using Plainion.GraphViz.Infrastructure;
using Plainion.GraphViz.Modules.CodeInspection.Core;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance.Analyzers
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class TypeRelationshipDocument
    {
        private readonly HashSet<Tuple<string, string, ReferenceType>> myEdges;
        private readonly Dictionary<string, TypeDescriptor> myDescriptors;
        private readonly List<FailedItem> myFailedItems;

        public TypeRelationshipDocument()
        {
            myDescriptors = new Dictionary<string, TypeDescriptor>();
            myEdges = new HashSet<Tuple<string, string, ReferenceType>>();
            myFailedItems = new List<FailedItem>();
        }

        public IEnumerable<FailedItem> FailedItems { get { return myFailedItems; } }

        public IEnumerable<TypeDescriptor> Descriptors { get { return myDescriptors.Values; } }

        public IEnumerable<Tuple<string, string, ReferenceType>> Edges { get { return myEdges; } }

        public void AddEdge(TypeDescriptor source, TypeDescriptor target, ReferenceType refType)
        {
            if (!myDescriptors.ContainsKey(source.Id))
            {
                myDescriptors.Add(source.Id, source);
            }

            if (!myDescriptors.ContainsKey(target.Id))
            {
                myDescriptors.Add(target.Id, target);
            }

            myEdges.Add(Tuple.Create(source.Id, target.Id, refType));
        }

        public void AddFailedItem(FailedItem item)
        {
            myFailedItems.Add(item);
        }
    }
}
