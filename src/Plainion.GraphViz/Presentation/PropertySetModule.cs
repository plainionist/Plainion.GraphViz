using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Plainion.Graphs.Projections;

namespace Plainion.GraphViz.Presentation
{
    public class PropertySetModule<T> : AbstractModule<T>, IPropertySetModule<T> where T : AbstractPropertySet
    {
        private readonly Dictionary<string, T> myItems;

        public PropertySetModule(Func<string, T> defaultProvider)
        {
            Contract.RequiresNotNull(defaultProvider, nameof(defaultProvider));

            DefaultProvider = defaultProvider;

            myItems = [];
        }

        public Func<string, T> DefaultProvider { get; }

        public bool Contains(string id)
        {
            return myItems.ContainsKey(id);
        }

        public T TryGet(string id)
        {
            T value = null;
            if (myItems.TryGetValue(id, out value))
            {
                return value;
            }

            return null;
        }

        public T Get(string id)
        {
            T value = null;
            if (myItems.TryGetValue(id, out value))
            {
                return value;
            }

            // do not put "null" into the items to allow defining new default later on

            if (DefaultProvider == null)
            {
                return null;
            }

            value = DefaultProvider(id);
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

            // we need to support override as we pre-fill the captions module for performance reasons
            // see setter of the graph to GraphPresentation
            myItems.Remove(item.OwnerId);

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
            get { return myItems.Values; }
        }
    }
}
