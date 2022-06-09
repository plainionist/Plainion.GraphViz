using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

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

    class AssemblyLoader
    {
        private Assembly ReflectionOnlyLoadFrom(MetadataLoadContext ctx, string file)
        {
            try
            {
                return ctx.LoadFromAssemblyPath(file);
            }
            catch (BadImageFormatException)
            {
                Shell.Warn($"Loading skipped for native/mixed mode assembly: {file}");
                return null;
            }
        }

        private string TryGetAssemblyLocation(IEnumerable<string> paths, AssemblyName name)
        {
            var assemblyExtensions = new[] { ".dll", ".exe" };

            return paths
                .SelectMany(baseDir => assemblyExtensions.Select(ext => Path.Combine(baseDir, name.Name + ext)))
                .FirstOrDefault(file => File.Exists(file));
        }

        private Assembly TryLoadFromGAC(MetadataLoadContext ctx, AssemblyName name)
        {
            try
            {
                // e.g. .NET assemblies, assemblies from GAC
                return ctx.LoadFromAssemblyName(name.FullName);
            }
            catch
            {
                // ignore exception here - e.g. System.Windows.Interactivity - App will work without
                Debug.WriteLine("Failed to load: " + name.ToString());
                return null;
            }
        }

        private Assembly ReflectionOnlyLoad(IEnumerable<string> paths, MetadataLoadContext ctx, AssemblyName name)
        {
            var location = TryGetAssemblyLocation(paths, name);
            if (location != null)
            {
                return ReflectionOnlyLoadFrom(ctx, location);
            }
            else
            {
                return TryLoadFromGAC(ctx, name);
            }
        }

        private Assembly TryReflectionOnlyLoadByName(IEnumerable<string> paths, MetadataLoadContext ctx, AssemblyName name)
        {
            var assembly = ctx.GetAssemblies().SingleOrDefault(asm => String.Equals(asm.FullName, name.FullName, StringComparison.OrdinalIgnoreCase));
            if (assembly != null)
            {
                // don't try to load the given assembly because we cannot load the same assembly twice from different locations
                // instead just load the already "linked" assembly (e.g. Fsharp.Core) with reflection only again
                return ctx.LoadFromAssemblyName(assembly.FullName);
            }

            return assembly ?? ReflectionOnlyLoad(paths, ctx, name);
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

        public Assembly LoadAssembly(AssemblyName name)
        {
            try
            {
                var paths = new[] { AppDomain.CurrentDomain.BaseDirectory };
                var resolver = new PathAssemblyResolver(paths);
                using (var ctx = new MetadataLoadContext(resolver))
                {
                    var assembly = TryReflectionOnlyLoadByName(paths, ctx, name);

                    ForceLoadDependencies(assembly);

                    return assembly;
                }
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
                var resolver = new PathAssemblyResolver(new[] { AppDomain.CurrentDomain.BaseDirectory, Path.GetDirectoryName(file) });
                using (var ctx = new MetadataLoadContext(resolver))
                {
                    var assembly = ReflectionOnlyLoadFrom(ctx, file);

                    ForceLoadDependencies(assembly);

                    return assembly;
                }
            }
            catch (Exception ex)
            {
                Shell.Warn(ex.Dump());
                return null;
            }
        }
    }
}
