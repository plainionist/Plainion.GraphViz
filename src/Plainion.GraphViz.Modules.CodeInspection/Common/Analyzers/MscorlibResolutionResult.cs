using System.IO;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    internal class MscorlibResolutionResult : AssemblyResolutionResult
    {
        public MscorlibResolutionResult(FileInfo file, string referenceAssemblies) 
            : base(file)
        {
            ReferenceAssemblies = new DirectoryInfo(referenceAssemblies);

            Contract.Requires(ReferenceAssemblies.Exists, $"'{referenceAssemblies}' does not exist");
        }

        public DirectoryInfo ReferenceAssemblies { get; }
    }
}