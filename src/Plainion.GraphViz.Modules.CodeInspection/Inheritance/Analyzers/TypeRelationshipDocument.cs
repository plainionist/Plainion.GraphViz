using System;
using System.Collections.Generic;
using Plainion.GraphViz.Infrastructure;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Modules.CodeInspection.Core;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance.Analyzers
{
    [Serializable]
    class TypeRelationshipDocument
    {
        private HashSet<Tuple<string, string, ReferenceType>> myEdges;
        private IDictionary<string, TypeDescriptor> myDescriptors;

        public TypeRelationshipDocument()
        {
            myDescriptors = new Dictionary<string, TypeDescriptor>();
            myEdges = new HashSet<Tuple<string, string, ReferenceType>>();
            FailedItems = new List<FailedItem>();
        }

        public IList<FailedItem> FailedItems { get; private set; }

        public IEnumerable<TypeDescriptor> Descriptors
        {
            get { return myDescriptors.Values; }
        }

        public IEnumerable<Tuple<string, string, ReferenceType>> Edges
        {
            get { return myEdges; }
        }

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
    }
}
