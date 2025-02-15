using System;

namespace Plainion.GraphViz.Presentation
{
    public interface IPropertySetModule<T> : IModule<T>
    {
        bool Contains(string id);
        T TryGet(string id);
        T Get(string id);

        void Add(T item);
        void Remove(string id);
        void Clear();
    }
}
