using System.Collections.Generic;

namespace Plainion.GraphViz.Presentation
{
    public interface IModuleChangedJournal<T> : IModuleChangedObserver
    {
        IEnumerable<T> Entries { get; }

        bool IsEmpty { get; }

        void Clear();
    }
}
