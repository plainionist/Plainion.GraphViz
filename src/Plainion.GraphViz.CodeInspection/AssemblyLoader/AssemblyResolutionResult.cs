using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Plainion.GraphViz.CodeInspection.AssemblyLoader;

[DebuggerDisplay("{AssemblyName}")]
class AssemblyResolutionResult
{
    public AssemblyResolutionResult(FileInfo file)
    {
        Contract.RequiresNotNull(file, nameof(file));

        File = file;
        AssemblyName = AssemblyName.GetAssemblyName(file.FullName);
    }

    public FileInfo File { get; }
    public AssemblyName AssemblyName { get; private set; }
}
