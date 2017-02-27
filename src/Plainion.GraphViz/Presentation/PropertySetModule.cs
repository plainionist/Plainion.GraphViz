using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Plainion.GraphViz.Presentation
{
    public class PropertySetModule<T> : AbstractModule<T>, IPropertySetModule<T> where T : AbstractPropertySet
    {
        private Dictionary<string, T> myItems;

        public PropertySetModule(Func<string, T> defaultProvider)
        {
            DefaultProvider = defaultProvider;

            myItems = new Dictionary<string, T>();
        }

        public Func<string, T> DefaultProvider { get; set; }

        public bool Contains(string id)
        {
            return myItems.ContainsKey(id);
        }

        public T Get(string id)
        {
            if (myItems.ContainsKey(id))
            {
                return myItems[id];
            }

            // do not put "null" into the items to allow defining new default later on

            if (DefaultProvider == null)
            {
                return null;
            }

            var value = DefaultProvider(id);
            if (value == null)
            {
                return null;
            }

            Add(value);

            return value;
        }

        public void Add(T item)
        {
            OnAdding(item);

            myItems.Add(item.OwnerId, item);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        protected virtual void OnAdding(T item)
        {
        }

        public void Remove(string id)
        {
            if (!myItems.ContainsKey(id))
            {
                return;
            }

            var item = myItems[id];
            myItems.Remove(id);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        }

        public void Clear()
        {
            if (!myItems.Any())
            {
                return;
            }

            var removedItems = myItems.Values.ToList();
            myItems.Clear();

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems));
        }

        public override IEnumerable<T> Items
        {
            get
            {
                return myItems.Values;
            }
        }
    }
}
