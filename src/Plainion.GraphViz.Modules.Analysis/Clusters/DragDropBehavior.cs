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
public class DragDropBehavior
{
    private NodeWriteAccess myRoot;

    private class ObservableCollection
    {
        private IEnumerable<INode> myCollection;

        public ObservableCollection(IEnumerable<INode> collection)
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
        public NodeWriteAccess(INode node)
        {
            Contract.RequiresNotNull(node, "node");

            Node = node;

            Contract.Requires(Node.GetType().GetProperty("Parent").CanWrite, "Parent is not writable");

            Children = new ObservableCollection(Node.Children);
            Siblings = new ObservableCollection(Node.Parent != null ? Node.Parent.Children : new ObservableCollection<INode>());
        }

        public INode Node { get; private set; }

        public ObservableCollection Children { get; private set; }

        public ObservableCollection Siblings { get; private set; }

        public NodeWriteAccess GetParent()
        {
            return Node.Parent != null ? new NodeWriteAccess(Node.Parent) : null;
        }

        public void SetParent(INode node)
        {
            Node.GetType().GetProperty("Parent").SetValue(Node, node);
        }
    }

    public DragDropBehavior(INode root)
    {
        Contract.RequiresNotNull(root, "root");

        myRoot = new NodeWriteAccess(root);
    }

    public void ApplyDrop(NodeDropRequest request)
    {
        var droppedNode = new NodeWriteAccess(request.DroppedNode);
        var dropTarget = new NodeWriteAccess(request.DropTarget);

        if (request.DropTarget == myRoot)
        {
            ChangeParent(droppedNode, n => myRoot.Children.Add(n), myRoot);
        }
        else if (request.Location == DropLocation.Before || request.Location == DropLocation.After)
        {
            MoveNode(droppedNode, dropTarget, request.Location);
        }
        else
        {
            ChangeParent(droppedNode, n => dropTarget.Children.Add(n), dropTarget);
        }
    }

    private void MoveNode(NodeWriteAccess nodeToMove, NodeWriteAccess targetNode, DropLocation operation)
    {
        var siblings = targetNode.Siblings;
        var dropPos = siblings.IndexOf(targetNode);

        if (operation == DropLocation.After)
        {
            dropPos++;
        }

        if (siblings.Contains(nodeToMove))
        {
            var oldPos = siblings.IndexOf(nodeToMove);
            if (oldPos < dropPos)
            {
                // ObservableCollection first removes the item and then reinserts which invalidates the index
                dropPos--;
            }

            siblings.Move(oldPos, dropPos);
        }
        else
        {
            if (dropPos < siblings.Count)
            {
                ChangeParent(nodeToMove, n => siblings.Insert(dropPos, n), targetNode.GetParent());
            }
            else
            {
                ChangeParent(nodeToMove, n => siblings.Add(n), targetNode.GetParent());
            }
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
