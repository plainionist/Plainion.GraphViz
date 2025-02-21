using System;
using System.Reflection;

namespace Plainion.GraphViz.Modules.CodeInspection.Reflection
{
    public interface IAssemblyLoader : IDisposable
    {
        Assembly TryLoadAssembly(string path);
        Assembly TryLoadDependency(Assembly requestingAssembly, AssemblyName dependency);
    }
}