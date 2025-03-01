using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Plainion.GraphViz.Presentation;

/// <summary>
/// Masks handled by this module describe a part of the graph.
/// That means "true" will be interpreted as "visible".
/// implements a STACK of masks - LIFO
/// </summary>
class NodeMaskModule : AbstractModule<INodeMask>, INodeMaskModule
{
    private readonly ObservableCollection<INodeMask> myMasks;

    public NodeMaskModule()
    {
        myMasks = [];
        myMasks.CollectionChanged += OnCollectionChanged;
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        OnCollectionChanged(e);
    }

    public override IEnumerable<INodeMask> Items => myMasks;

    public void Push(INodeMask mask)
    {
        Insert(0, mask);
    }

    public void Insert(int pos, INodeMask mask)
    {
        myMasks.Insert(pos, mask);
    }

    public void Remove(INodeMask mask)
    {
        myMasks.Remove(mask);
    }

    public void Clear()
    {
        myMasks.Clear();
    }

    public void MoveUp(INodeMask mask)
    {
        int pos = myMasks.IndexOf(mask);
        if (pos == 0)
        {
            return;
        }

        myMasks.Move(pos, pos - 1);
    }

    public void MoveDown(INodeMask mask)
    {
        int pos = myMasks.IndexOf(mask);
        if (pos == myMasks.Count - 1)
        {
            return;
        }

        myMasks.Move(pos, pos + 1);
    }
}
