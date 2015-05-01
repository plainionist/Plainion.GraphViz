using System;

namespace Plainion.GraphViz.Modules.Reflection.Services.Framework
{
    public interface IInspectorHandle<T> : IDisposable where T : InspectorBase
    {
        T Value { get; }
    }
}
