using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    class R
    {
        public static string AssemblyName(Assembly asm)
        {
            return asm.GetName().Name;
        }

        public static string TypeFullName(Type t)
        {
            return t.FullName != null ? t.FullName : $"{t.Namespace}.{t.Name}";
        }
    }

    class AssemblyLoader : IDisposable
    {
        private readonly List<string> myPaths;
        private MetadataLoadContext myContext;

        public AssemblyLoader(IEnumerable<string> assemblies)
        {
            myPaths = new List<string>(Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll"));
            myPaths.AddRange(assemblies);

            var resolver = new PathAssemblyResolver(myPaths);

            myContext = new MetadataLoadContext(resolver);
        }

        public void Dispose()
        {
            myContext?.Dispose();
            myContext = null;
        }

        public Assembly LoadAssembly(AssemblyName name)
        {
            try
            {
                var assembly = TryReflectionOnlyLoadByName(name);

                ForceLoadDependencies(assembly);

                return assembly;
            }
            catch (Exception ex)
            {
                Shell.Warn(ex.Dump());
                return null;
            }
        }

        public Assembly LoadAssemblyFrom(string file)
        {
            try
            {
                var assembly = ReflectionOnlyLoadFrom(file);

                ForceLoadDependencies(assembly);

                return assembly;
            }
            catch (Exception ex)
            {
                Shell.Warn(ex.Dump());
                return null;
            }
        }

        private Assembly ReflectionOnlyLoadFrom(string file)
        {
            try
            {
                return myContext.LoadFromAssemblyPath(file);
            }
            catch (BadImageFormatException)
            {
                Shell.Warn($"Loading skipped for native/mixed mode assembly: {file}");
                return null;
            }
        }

        private string TryGetAssemblyLocation(AssemblyName name)
        {
            var assemblyExtensions = new[] { ".dll", ".exe" };

            return myPaths
                .SelectMany(baseDir => assemblyExtensions.Select(ext => Path.Combine(baseDir, name.Name + ext)))
                .FirstOrDefault(file => File.Exists(file));
        }

        private Assembly TryLoadFromGAC(AssemblyName name)
        {
            try
            {
                // e.g. .NET assemblies, assemblies from GAC
                return myContext.LoadFromAssemblyName(name.FullName);
            }
            catch
            {
                // ignore exception here - e.g. System.Windows.Interactivity - App will work without
                Debug.WriteLine("Failed to load: " + name.ToString());
                return null;
            }
        }

        private Assembly ReflectionOnlyLoad(AssemblyName name)
        {
            var location = TryGetAssemblyLocation(name);
            if (location != null)
            {
                return ReflectionOnlyLoadFrom(location);
            }
            else
            {
                return TryLoadFromGAC(name);
            }
        }

        private Assembly TryReflectionOnlyLoadByName(AssemblyName name)
        {
            var assembly = myContext.GetAssemblies().SingleOrDefault(asm => String.Equals(asm.FullName, name.FullName, StringComparison.OrdinalIgnoreCase));
            if (assembly != null)
            {
                // don't try to load the given assembly because we cannot load the same assembly twice from different locations
                // instead just load the already "linked" assembly (e.g. Fsharp.Core) with reflection only again
                return myContext.LoadFromAssemblyName(assembly.FullName);
            }

            return assembly ?? ReflectionOnlyLoad(name);
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
                var msg = ex.Dump();
                Shell.Warn($"Failed to load assembly: {asm.FullName} ({msg})");
            }
        }
    }
}
