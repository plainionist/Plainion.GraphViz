/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2016. All rights reserved
   ------------------------------------------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;

namespace Plainion.GraphViz.Pioneer.Packaging
{
    class AssemblyLoader
    {
        private List<Assembly> myAssemblies = new List<Assembly>();
        private Dictionary<string, AssemblyDefinition> myMonoCache = new Dictionary<string, AssemblyDefinition>();

        internal Assembly Load(string path)
        {
            try
            {
                Console.WriteLine("Loading {0}", path);

                var asm = Assembly.LoadFrom(path);
                myAssemblies.Add(asm);
                return asm;
            }
            catch
            {
                Console.WriteLine("ERROR: failed to load assembly {0}", path);
                return null;
            }
        }

        public Type FindTypeByName(string fullName)
        {
            return myAssemblies
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => fullName.StartsWith(t.FullName));
        }

        internal AssemblyDefinition MonoLoad(Assembly assembly)
        {
            if (!myMonoCache.ContainsKey(assembly.Location))
            {
                myMonoCache[assembly.Location] = AssemblyDefinition.ReadAssembly(assembly.Location);
            }
            return myMonoCache[assembly.Location];
        }
    }
}
