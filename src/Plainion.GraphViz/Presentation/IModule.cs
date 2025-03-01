using System.Collections.Generic;
using System.Collections.Specialized;

namespace Plainion.GraphViz.Presentation;

public interface IModule<T> : INotifyCollectionChanged
{
    IEnumerable<T> Items { get; }

    IModuleChangedObserver CreateObserver();
    IModuleChangedJournal<T> CreateJournal();
}
