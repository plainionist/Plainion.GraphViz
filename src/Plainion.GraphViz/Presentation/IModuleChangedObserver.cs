using System;

namespace Plainion.GraphViz.Presentation
{
    public interface IModuleChangedObserver : IDisposable
    {
        event EventHandler ModuleChanged;
        IDisposable Mute();
    }
}
