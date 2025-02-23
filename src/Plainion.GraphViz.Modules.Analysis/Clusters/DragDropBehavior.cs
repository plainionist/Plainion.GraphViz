using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Plainion.Collections;
using Plainion.Windows.Interactivity.DragDrop;

namespace Plainion.GraphViz.Modules.Analysis.Clusters;

/// <summary>
/// Implements DragDrop in the tree. Can be used by users of the TreeEditor to handle 
/// Drag and Drop commands.
/// <para>
/// Pre-conditions:
/// - INode.Parent must be writable
/// - INode.Children must be of type ObservableCollection{T}
/// </para>
/// </summary>
class DragDropBehavior
{
    private NodeWriteAccess myRoot;

    private class ObservableCollection
    {
        private IEnumerable<NodeViewModel> myCollection;

        public ObservableCollection(IEnumerable<NodeViewModel> collection)
        {
            Contract.Requires(collection.GetType().IsGenericType
                && collection.GetType().GetGenericTypeDefinition() == typeof(ObservableCollection<>), "Collection is not of type ObservableCollection<T>");

            myCollection = collection;
        }

        public void Add(NodeWriteAccess node)
        {
            ((IList)myCollection).Add(node.Node);
        }

        public void Insert(int pos, NodeWriteAccess node)
        {
            ((IList)myCollection).Insert(pos, node.Node);
        }

        public void Move(int oldPos, int newPos)
        {
            myCollection.GetType().GetMethod("Move").Invoke(myCollection, new object[] { oldPos, newPos });
        }

        public void Remove(NodeWriteAccess node)
        {
            ((IList)myCollection).Remove(node.Node);
        }

        public int IndexOf(NodeWriteAccess node)
        {
            return myCollection.IndexOf(node.Node);
        }

        public bool Contains(NodeWriteAccess node)
        {
            return myCollection.Contains(node.Node);
        }

        public int Count { get { return myCollection.Count(); } }
    }

    private class NodeWriteAccess
    {
        public NodeWriteAccess(NodeViewModel node)
        {
            Contract.RequiresNotNull(node, "node");

            Node = node;

            Contract.Requires(Node.GetType().GetProperty(nameof(Node.Parent)).CanWrite, "Parent is not writable");

            Children = new ObservableCollection(Node.Children);
        }

        public NodeViewModel Node { get; private set; }

        public ObservableCollection Children { get; private set; }

        public NodeWriteAccess GetParent()
        {
            return Node.Parent != null ? new NodeWriteAccess(Node.Parent) : null;
        }

        public void SetParent(NodeViewModel node)
        {
            Node.GetType().GetProperty(nameof(Node.Parent)).SetValue(Node, node);
        }
    }

    public DragDropBehavior(NodeViewModel root)
    {
        Contract.RequiresNotNull(root, "root");

        myRoot = new NodeWriteAccess(root);
    }

    public void ApplyDrop(NodeDropRequest request)
    {
        var droppedNode = new NodeWriteAccess(request.DroppedNode);
        var dropTarget = new NodeWriteAccess(request.DropTarget);

        if (request.DropTarget == myRoot.Node)
        {
            ChangeParent(droppedNode, myRoot.Children.Add, myRoot);
        }
        else
        {
            ChangeParent(droppedNode, dropTarget.Children.Add, dropTarget);
        }
    }

    private void ChangeParent(NodeWriteAccess nodeToMove, Action<NodeWriteAccess> insertionOperation, NodeWriteAccess newParent)
    {
        var oldParent = nodeToMove.GetParent();

        oldParent.Children.Remove(nodeToMove);

        insertionOperation(nodeToMove);

        nodeToMove.SetParent(newParent.Node);
    }
}
