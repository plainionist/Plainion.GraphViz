using System;
using System.Reflection;

namespace Plainion.GraphViz.CodeInspection.AssemblyLoader;

public interface IAssemblyLoader : IDisposable
{
    Assembly TryLoadAssembly(string path);
    Assembly TryLoadDependency(Assembly requestingAssembly, AssemblyName dependency);
}