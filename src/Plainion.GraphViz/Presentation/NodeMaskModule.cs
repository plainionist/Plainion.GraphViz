using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Plainion.GraphViz.Presentation
{
    /// <summary>
    /// Masks handled by this module describe a part of the graph.
    /// That means "true" will be interpreted as "visible".
    /// implements a STACK of masks - LIFO
    /// </summary>
    internal class NodeMaskModule : AbstractModule<INodeMask>, INodeMaskModule
    {
        private ObservableCollection<INodeMask> myMasks;
        private INodeMask myAllNodesMask;
        private IModuleChangedObserver mySelfObserver;

        public NodeMaskModule()
        {
            myMasks = new ObservableCollection<INodeMask>();
            myMasks.CollectionChanged += OnCollectionChanged;

            mySelfObserver = CreateObserver();
            mySelfObserver.ModuleChanged += OnModuleChanged;
        }

        private void OnModuleChanged(object sender, EventArgs e)
        {
            HideAllNodesOnDemand();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(e);
        }

        public override IEnumerable<INodeMask> Items
        {
            get { return myMasks; }
        }

        public void Push(INodeMask mask)
        {
            Insert(0, mask);
        }

        public void Insert(int pos, INodeMask mask)
        {
            if (mask is AllNodesMask)
            {
                if (myAllNodesMask != null)
                {
                    myMasks.Remove(myAllNodesMask);
                }

                myMasks.Insert(pos, mask);

                myAllNodesMask = mask;
            }
            else
            {
                myMasks.Insert(pos, mask);
            }
        }

        public void Remove(INodeMask mask)
        {
            if (mask == myAllNodesMask)
            {
                return;
            }

            myMasks.Remove(mask);
        }

        public void Clear()
        {
            myMasks.Clear();

            if (AutoHideAllNodesForShowMasks)
            {
                AutoHideAllNodesForShowMasks = false;
                AutoHideAllNodesForShowMasks = true;
            }
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

        private void HideAllNodesOnDemand()
        {
            if (myAllNodesMask != null && myMasks.Any(s => s.IsShowMask && s.IsApplied))
            {
                myAllNodesMask.IsApplied = true;
            }

            if (myAllNodesMask != null && !myMasks.Any(s => s.IsShowMask && s.IsApplied))
            {
                myAllNodesMask.IsApplied = false;
            }
        }

        /// <summary>
        /// Defines that as soon as there is a single "show" mask in the list of masks and "hide all nodes" mask is 
        /// inserted at the bottom in order to ensure that "show" mask has any visual effect.
        /// </summary>
        public bool AutoHideAllNodesForShowMasks
        {
            get { return myAllNodesMask != null; }
            set
            {
                if (value && myAllNodesMask == null)
                {
                    myAllNodesMask = new AllNodesMask();
                    myAllNodesMask.IsShowMask = false;

                    Insert(0, myAllNodesMask);

                    return;
                }

                if (!value && myAllNodesMask != null)
                {
                    myAllNodesMask = null;
                    Remove(myAllNodesMask);
                }
            }
        }
    }
}
