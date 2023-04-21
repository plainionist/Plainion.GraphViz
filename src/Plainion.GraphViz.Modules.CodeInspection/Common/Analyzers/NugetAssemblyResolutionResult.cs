using System.IO;
using System.Reflection;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    public class NugetAssemblyResolutionResult : AssemblyResolutionResult
    {
        public NugetAssemblyResolutionResult(FileInfo file, DotNetFrameworkVersion fwVersion, SemanticVersion packageVersion)
            : base(file)
        {
            Contract.RequiresNotNull(fwVersion, nameof(fwVersion));
            Contract.RequiresNotNull(packageVersion, nameof(packageVersion));

            DotNetFrameworkVersion = fwVersion;
            PackageVersion = packageVersion;
        }

        public DotNetFrameworkVersion DotNetFrameworkVersion { get; }

        public SemanticVersion PackageVersion { get; }

        public static NugetAssemblyResolutionResult TryCreate(FileInfo file)
        {
            var fwVersion = DotNetFrameworkVersion.TryParse(file.Directory.Name);
            if (fwVersion == null)
            {
                return null;
            }

            var libDir = file.Directory.Parent;
            if (libDir.Name != "lib")
            {
                libDir = libDir.Parent;
            }
            if (libDir.Name != "lib")
            {
                return null;
            }

            var packageVersion = SemanticVersion.TryParse(libDir.Parent.Name);
            if (packageVersion == null)
            {
                return null;
            }

            var packageName = libDir.Parent.Parent.Name;
            var assemblyName = AssemblyName.GetAssemblyName(file.FullName);

            return packageName == assemblyName.Name
                ? new NugetAssemblyResolutionResult(file, fwVersion, packageVersion)
                : null;
        }
    }
}
