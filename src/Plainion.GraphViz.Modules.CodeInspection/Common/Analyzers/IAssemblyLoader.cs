using System;
using System.Reflection;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    internal interface IAssemblyLoader : IDisposable
    {
        Assembly TryLoadAssembly(string path);
        Assembly TryLoadDependency(Assembly requestingAssembly, AssemblyName dependency);
    }
}