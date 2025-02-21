using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Plainion.GraphViz.CodeInspection;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Modules.CodeInspection.Common;
using Plainion.Logging;

namespace Plainion.GraphViz.Modules.CodeInspection.CallTree.Analyzers
{
    class InterfaceImplementationsMap
    {
        private static readonly ILogger myLogger = LoggerFactory.GetLogger(typeof(InterfaceImplementationsMap));

        private readonly IReadOnlyCollection<Assembly> myAssemblies;
        private readonly Regex myTypeParameterPattern;
        private readonly Dictionary<string, IReadOnlyCollection<Type>> myMap;
        private List<Type> myTargetTypes;

        public InterfaceImplementationsMap(IEnumerable<Assembly> assemblies)
        {
            myAssemblies = assemblies.ToList();
            myTypeParameterPattern = new Regex(@"\[\[.*\]\]");
            myMap = new Dictionary<string, IReadOnlyCollection<Type>>();
        }

        public void Build(IReadOnlyCollection<Node> relevantNodes, IEnumerable<Type> targetTypes)
        {
            myTargetTypes = targetTypes.ToList();

            var interfaceImplMap = relevantNodes
                .SelectMany(n => myAssemblies.Single(x => R.AssemblyName(x) == n.Id).GetTypes())
                .SelectMany(t => t.GetInterfaces().Select(GetInterfaceId).Where(iface => iface != null).Select(iface => (iface, t)))
                .ToList();

            // if we find more than that amount of implementations the interface is too generic and
            // it doesn't make sense to replace it
            var implCountThreshold = 10;

            foreach(var g in interfaceImplMap.GroupBy(x => x.Item1))
            {
                if (g.Count() < implCountThreshold)
                {
                    myMap.Add(g.Key, g.Select(x=>x.Item2).ToList());
                }
            }
        }

        public IEnumerable<MethodCall> ResolveInterface(MethodCall call)
        {
            if (call.To.DeclaringType.IsInterface && (!myTargetTypes.Any(t => t == call.To.DeclaringType)))
            {
                if(myMap.TryGetValue(call.To.DeclaringType.AssemblyQualifiedName, out var impls))
                {
                    Console.Write("#");
                    return impls.Select(t => new MethodCall(call.From, new Method(t, call.To.Name)));
                }
                else
                {
                    myLogger.Warning($"Implementation not found for: {call.To.DeclaringType.FullName}");
                    return new[] { call };
                }
            }
            else
            {
                return new[] { call };
            }
        }

        private string GetInterfaceId(Type iface)
        {
            if (iface.AssemblyQualifiedName == null)
            {
                return null;
            }
            else if (iface.AssemblyQualifiedName.StartsWith("System."))
            {
                return null;
            }
            else if (iface.AssemblyQualifiedName.Contains("[["))
            {
                return myTypeParameterPattern.Replace(iface.AssemblyQualifiedName, "");
            }
            else
            {
                return iface.AssemblyQualifiedName;
            }
        }
    }
}
