using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Plainion.GraphViz.Modules.CodeInspection.Batch
{
    public class AssemblyLoader
    {
        private HashSet<string> myBaseDirs;

        public AssemblyLoader()
        {
            myBaseDirs = new HashSet<string>();
            myBaseDirs.Add(AppDomain.CurrentDomain.BaseDirectory);

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomain_ReflectionOnlyAssemblyResolve;
        }

        private Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // do not call "GetTypes()" here because we cannot cause another load of referenced assemblies from within the event handler

            // netstandard.dll references "System 0.0.0.0" 
            // -> therefore calling ApplyPolicy to resolve the actual version number
            // (see also: https://github.com/dotnet/standard/issues/446)

            var name = new AssemblyName(AppDomain.CurrentDomain.ApplyPolicy(args.Name));
            return TryReflectionOnlyLoadByName(name);
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(asm => String.Equals(asm.FullName, args.Name, StringComparison.OrdinalIgnoreCase));
        }

        public static string AssemblyName(Assembly asm)
        {
            return asm.GetName().Name;
        }

        public static string TypeFullName(Type t)
        {
            return t.FullName != null ? t.FullName : $"{t.Namespace}.{t.Name}";
        }

        private Assembly ReflectionOnlyLoadFrom(string file)
        {
            try
            {
                return Assembly.ReflectionOnlyLoadFrom(file);
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

            return myBaseDirs
                .SelectMany(baseDir => assemblyExtensions.Select(ext => Path.Combine(baseDir, name.Name + ext)))
                .FirstOrDefault(file => File.Exists(file));
        }

        private Assembly TryLoadFromGAC(AssemblyName name)
        {
            try
            {
                // e.g. .NET assemblies, assemblies from GAC
                return Assembly.ReflectionOnlyLoad(name.FullName);
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
            var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(asm => String.Equals(asm.FullName, name.FullName, StringComparison.OrdinalIgnoreCase));
            if (assembly != null)
            {
                // don't try to load the given assembly because we cannot load the same assembly twice from different locations
                // instead just load the already "linked" assembly (e.g. Fsharp.Core) with reflection only again
                return Assembly.ReflectionOnlyLoad(assembly.FullName);
            }

            assembly = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies().SingleOrDefault(asm => String.Equals(asm.FullName, name.FullName, StringComparison.OrdinalIgnoreCase));
            return assembly != null ? assembly : ReflectionOnlyLoad(name);
        }

        private Assembly ReflectionOnlyLoadAssemblyFrom(string file)
        {
            myBaseDirs.Add(Path.GetDirectoryName(file));
            return ReflectionOnlyLoadFrom(file);
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
                var assembly = ReflectionOnlyLoadAssemblyFrom(file);

                ForceLoadDependencies(assembly);

                return assembly;
            }
            catch (Exception ex)
            {
                Shell.Warn(ex.Dump());
                return null;
            }
        }
    }
}
