using System;
using System.IO;
using System.Reflection;
using Plainion.Logging;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    class AssemblyLoader 
    {
        private static readonly ILogger myLogger = LoggerFactory.GetLogger(typeof(AssemblyLoader));

        public AssemblyLoader(bool forceLoadDependencies = true)
        {
            ForceLoadDependencies = forceLoadDependencies;
        }

        public bool ForceLoadDependencies { get; }

        public Assembly TryLoadAssembly(AssemblyName name)
        {
            try
            {
                var assembly = Assembly.Load(name);

                ForceLoadDependenciesIfRequested(assembly);

                return assembly;
            }
            catch (Exception ex)
            {
                myLogger.Error($"Failed to load assembly {name}{Environment.NewLine}{ex.Message}");
                return null;
            }
        }

        public Assembly TryLoadAssembly(string path)
        {
            if (!IsAssembly(path))
            {
                return null;
            }

            try
            {
                var assembly = Assembly.LoadFrom(path);

                ForceLoadDependenciesIfRequested(assembly);

                return assembly;
            }
            catch (Exception ex)
            {
                myLogger.Error($"Failed to load assembly {path}{Environment.NewLine}{ex.Message}");
                return null;
            }
        }

        private static bool IsAssembly(string path)
        {
            var ext = Path.GetExtension(path);
            return ".dll".Equals(ext, StringComparison.OrdinalIgnoreCase) || ".exe".Equals(ext, StringComparison.OrdinalIgnoreCase);
        }

        private void ForceLoadDependenciesIfRequested(Assembly asm)
        {
            if (!ForceLoadDependencies)
            {
                return;
            }

            try
            {
                // get all types to ensure that all relevant assemblies are loaded
                asm.GetTypes();
            }
            catch (Exception ex)
            {
                myLogger.Error($"Failed to load assembly {asm.FullName}{Environment.NewLine}{ex.Message}");
            }
        }
    }
}
