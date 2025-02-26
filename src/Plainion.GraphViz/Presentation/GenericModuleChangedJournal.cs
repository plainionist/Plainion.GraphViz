using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Plainion.GraphViz.Presentation
{
    class GenericModuleChangedJournal<T> : AbstractModuleChangedObserver<T>, IModuleChangedJournal<T>
    {
        private readonly List<T> myEntries;
        private readonly IEqualityComparer<T> myComparer;

        public GenericModuleChangedJournal(IModule<T> module, IEqualityComparer<T> comparer)
            :base(module)
        {
            myComparer = comparer;

            myEntries = new List<T>();
        }

        protected override void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems.Cast<T>())
                {
                    RecordItem(item);
                }
                foreach (var item in e.NewItems.OfType<INotifyPropertyChanged>())
                {
                    item.PropertyChanged += OnPropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems.Cast<T>())
                {
                    RecordItem(item);
                }
                foreach (var item in e.OldItems.OfType<INotifyPropertyChanged>())
                {
                    item.PropertyChanged -= OnPropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var item in Module.Items)
                {
                    RecordItem(item);
                }
                foreach (var item in Module.Items.OfType<INotifyPropertyChanged>())
                {
                    item.PropertyChanged -= OnPropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                foreach (var item in e.NewItems.Cast<T>())
                {
                    RecordItem(item);
                }
            }
            else
            {
                throw new NotSupportedException(e.Action.ToString());
            }
        }

        private void RecordItem(T item)
        {
            int i = 0;
            for (; i < myEntries.Count; ++i)
            {
                if (myComparer.Equals(myEntries[i], item))
                {
                    myEntries[i] = item;
                    break;
                }
            }

            if (i == myEntries.Count)
            {
                myEntries.Add(item);
            }

            RaiseModuleChanged();
        }

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var item = (T)sender;
            RecordItem(item);
        }

        public IEnumerable<T> Entries
        {
            get
            {
                return myEntries;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return myEntries.Count == 0;
            }
        }

        public void Clear()
        {
            myEntries.Clear();
        }
    }
}
