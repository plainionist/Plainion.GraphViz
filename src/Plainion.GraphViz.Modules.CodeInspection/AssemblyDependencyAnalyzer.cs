using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Plainion.GraphViz.Algorithms;
using Plainion.GraphViz.CodeInspection;
using Plainion.GraphViz.CodeInspection.AssemblyLoader;
using Plainion.GraphViz.Presentation;
using Plainion.Text;

namespace Plainion.GraphViz.Modules.CodeInspection;

public class AssemblyDependencyAnalyzer
{
    private class AssemblyReference
    {
        public Assembly myAssembly;
        public Assembly myDependency;
    }

    private readonly IAssemblyLoader myLoader;
    private readonly IEnumerable<Wildcard> myRelevantAssemblies;

    public AssemblyDependencyAnalyzer(IAssemblyLoader loader, IEnumerable<string> relevantAssemblies)
    {
        myLoader = loader;
        myRelevantAssemblies = relevantAssemblies.Select(p => new Wildcard(p)).ToList();
    }

    /// <summary>
    /// Creates a graph of all source and target assemblies and all assemblies directly and indirectly referenced
    /// by source assemblies which cause an indirect dependency to target assemblies
    /// </summary>
    public GraphPresentation CreateAssemblyGraph(IEnumerable<Assembly> sources, IEnumerable<Assembly> targets)
    {
        var deps = sources.Aggregate(new List<AssemblyReference>(), (acc, asm) => Analyze(acc, asm, false));

        return new SpecialGraphBuilder()
            .CreateGraphOfReachables(
                sources.Select(R.AssemblyName),
                targets.Select(R.AssemblyName),
                deps.Select(r => (R.AssemblyName(r.myAssembly), R.AssemblyName(r.myDependency))));
    }

    private List<AssemblyReference> Analyze(List<AssemblyReference> analyzed, Assembly asm, bool isSource)
    {
        var pad = isSource ? "  " : "    ";
        Console.WriteLine($"{pad}{asm.FullName}");

        var dependencies = asm.GetReferencedAssemblies()
            .Select(x => myLoader.TryLoadDependency(asm, x))
            .Where(x => x != null && FollowAssembly(x))
            .ToList();

        var analyzedAssemblies = analyzed
            .Select(x => x.myAssembly)
            .Distinct()
            .ToList();

        var initialAcc = analyzed.ToList();
        initialAcc.Add(new AssemblyReference { myAssembly = asm, myDependency = asm });

        var indirectDeps = dependencies
            .Where(x => analyzedAssemblies.Any(a => a.FullName != x.FullName))
            .Where(FollowAssembly)
            .Aggregate(initialAcc, (acc, asm) => Analyze(acc, asm, false));

        return dependencies
            .Select(x => new AssemblyReference { myAssembly = asm, myDependency = x })
            .Concat(indirectDeps)
            .ToList();
    }

    private bool FollowAssembly(Assembly asm)
    {
        return myRelevantAssemblies.Any(p => p.IsMatch(asm.GetName().Name));
    }
}
