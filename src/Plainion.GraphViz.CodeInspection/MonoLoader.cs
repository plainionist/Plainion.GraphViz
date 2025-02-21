﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;

namespace Plainion.GraphViz.CodeInspection
{
    public class MonoLoader
    {
        private readonly Dictionary<string, AssemblyDefinition> myMonoCache;
        private readonly HashSet<string> mySkippedAssemblies;

        public MonoLoader(IEnumerable<Assembly> assemblies)
        {
            Assemblies = assemblies.Distinct().ToList();
            myMonoCache = new Dictionary<string, AssemblyDefinition>();
            mySkippedAssemblies = new HashSet<string>();
        }

        public IReadOnlyCollection<Assembly> Assemblies { get; }

        public IReadOnlyCollection<string> SkippedAssemblies => mySkippedAssemblies;

        public AssemblyDefinition MonoLoad(Assembly assembly)
        {
            lock (myMonoCache)
            {
                if (!myMonoCache.ContainsKey(assembly.Location))
                {
                    myMonoCache[assembly.Location] = AssemblyDefinition.ReadAssembly(assembly.Location);
                }
                return myMonoCache[assembly.Location];
            }
        }

        public Type FindTypeByName(TypeReference typeRef)
        {
            // seems to be always the callers module
            //{
            //    var type = FindTypeByName(typeRef.Module.Assembly.FullName, typeRef);
            //    if (type != null)
            //    {
            //        return type;
            //    }
            //}

            var anr = typeRef.Scope as AssemblyNameReference;
            if (anr != null)
            {
                var type = FindTypeByName(anr.FullName, typeRef);
                if (type != null)
                {
                    return type;
                }
            }

            var md = typeRef.Scope as ModuleDefinition;
            if (md != null)
            {
                var type = FindTypeByName(md.Assembly.FullName, typeRef);
                if (type != null)
                {
                    return type;
                }
            }


            return null;
        }

        private Type FindTypeByName(string assemblyFullName, TypeReference typeRef)
        {
            if (typeRef.FullName == "<Module>")
            {
                return null;
            }

            var asm = Assemblies.SingleOrDefault(x => x.FullName.Equals(assemblyFullName, StringComparison.OrdinalIgnoreCase));

            if (asm == null)
            {
                // assuming the assembly does not belong to any package
                // -> skip it
                lock (mySkippedAssemblies)
                {
                    mySkippedAssemblies.Add(assemblyFullName);
                }
                return null;
            }

            var typeFullName = typeRef.FullName;

            if (typeRef.IsGenericInstance)
            {
                var idx = typeFullName.IndexOf('<');

                // compiler generated classes start with <> 
                // -> keep
                if (idx + 1 < typeFullName.Length && typeFullName[idx + 1] == '>')
                {
                    idx = typeFullName.IndexOf('<', idx + 1);
                }

                typeFullName = typeFullName.Substring(0, idx);
            }

            if (typeRef.IsNested)
            {
                typeFullName = typeFullName.Replace('/', '+');
            }

            // ',' in fullname has to be escaped
            typeFullName = typeFullName.Replace(",", "\\,");

            // sometimes there is '&' at the end??
            typeFullName = typeFullName.Replace("&", "");

            if (string.IsNullOrEmpty(typeFullName))
            {
                // TODO: log ignorance of these types
                return null;
            }

            var type = asm.GetType(typeFullName);

            if (type != null)
            {
                return type;
            }

            //
            // D I A G N O S T I C S
            //

            //var ns = typeRef.Namespace;
            //if (string.IsNullOrEmpty(ns))
            //{
            //    var idx = typeFullName.LastIndexOf('.');
            //    ns = typeFullName.Substring(0, idx);
            //}

            //Console.WriteLine();
            //Console.WriteLine(typeFullName);
            //Console.WriteLine("  [Asm] " + asm.FullName);
            //Console.WriteLine("  [NS]  " + ns);


            //foreach (var t in asm.GetTypes().Where(x => x.Namespace == ns))
            //{
            //    Console.WriteLine("  + " + t.FullName);
            //}

            //Environment.Exit(2);

            return null;
        }
    }
}
