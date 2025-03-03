﻿using System.Collections.Generic;
using System.Collections.Specialized;
using Plainion.Graphs;

namespace Plainion.GraphViz.Presentation
{
    public abstract class AbstractModule<T> : NotifyPropertyChangedBase, IModule<T>
    {
        public abstract IEnumerable<T> Items { get; }

        public IModuleChangedObserver CreateObserver()
        {
            return new GenericModuleChangedObserver<T>(this);
        }

        public IModuleChangedJournal<T> CreateJournal()
        {
            return new GenericModuleChangedJournal<T>(this, CreateEqualityComparer());
        }

        private IEqualityComparer<T> CreateEqualityComparer()
        {
            if (typeof(AbstractPropertySet).IsAssignableFrom(typeof(T)))
            {
                return (IEqualityComparer<T>)new PropertySetComparer();
            }
            else
            {
                return EqualityComparer<T>.Default;
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }
    }
}
