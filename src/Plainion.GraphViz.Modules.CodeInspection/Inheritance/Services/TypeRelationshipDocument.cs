using System;
using System.Collections.Generic;
using Plainion.GraphViz.Infrastructure;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Modules.CodeInspection.Core;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance.Services
{
    [Serializable]
    class TypeRelationshipDocument
    {
        private readonly RelaxedGraphBuilder myGraphBuilder;
        private readonly IDictionary<string, TypeDescriptor> myDescriptors;

        public TypeRelationshipDocument()
        {
            myGraphBuilder = new RelaxedGraphBuilder();
            myDescriptors = new Dictionary<string, TypeDescriptor>();
            FailedItems = new List<FailedItem>();
            EdgeTypes = new Dictionary<string, ReferenceType>();
        }

        public IList<FailedItem> FailedItems { get; private set; }

        public IGraph Graph
        {
            get { return myGraphBuilder.Graph; }
        }

        public IEnumerable<TypeDescriptor> Descriptors
        {
            get { return myDescriptors.Values; }
        }

        public void AddEdge(TypeDescriptor source, TypeDescriptor target)
        {
            if (!myDescriptors.ContainsKey(source.Id))
            {
                myDescriptors.Add(source.Id, source);
            }

            if (!myDescriptors.ContainsKey(target.Id))
            {
                myDescriptors.Add(target.Id, target);
            }

            myGraphBuilder.TryAddEdge(source.Id, target.Id);
        }

        public void AddNode(TypeDescriptor node)
        {
            if (!myDescriptors.ContainsKey(node.Id))
            {
                myDescriptors.Add(node.Id, node);
            }

            myGraphBuilder.TryAddNode(node.Id);
        }

        public IDictionary<string, ReferenceType> EdgeTypes { get; private set; }
    }
}
