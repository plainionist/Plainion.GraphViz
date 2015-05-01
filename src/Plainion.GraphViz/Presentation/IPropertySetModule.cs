using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Plainion.GraphViz.Presentation
{
    public interface IPropertySetModule<T> : IModule<T>
    {
        Func<string, T> DefaultProvider { get; set; }

        T Get( string id );

        void Add( T item );
        void Remove( string id );
        void Clear();
    }
}
