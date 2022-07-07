using System;
using System.Reflection;
using Plainion.Logging;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    class AssemblyLoader 
    {
        private static readonly ILogger myLogger = LoggerFactory.GetLogger(typeof(AssemblyLoader));

        public Assembly TryLoadAssembly(AssemblyName name)
        {
            try
            {
                var assembly = Assembly.Load(name);

                ForceLoadDependencies(assembly);

                return assembly;
            }
            catch (Exception ex)
            {
                myLogger.Error($"Failed to load assembly {name}{Environment.NewLine}{ex.Message}");
                return null;
            }
        }

        public Assembly TryLoadAssembly(string file)
        {
            try
            {
                var assembly = Assembly.LoadFrom(file);

                ForceLoadDependencies(assembly);

                return assembly;
            }
            catch (Exception ex)
            {
                myLogger.Error($"Failed to load assembly {file}{Environment.NewLine}{ex.Message}");
                return null;
            }
        }

        private void ForceLoadDependencies(Assembly asm)
        {
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
