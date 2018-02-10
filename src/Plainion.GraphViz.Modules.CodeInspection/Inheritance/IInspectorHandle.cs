using System;

namespace Plainion.GraphViz.Modules.CodeInspection.Inheritance.Services.Framework
{
    public interface IInspectorHandle<T> : IDisposable 
    {
        T Value { get; }
    }
}
