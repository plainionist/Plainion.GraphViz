using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Plainion.GraphViz.CodeInspection.AssemblyLoader;
using Plainion.GraphViz.Presentation;
using Plainion.Text;

namespace Plainion.GraphViz.Modules.CodeInspection;

public class AssemblyDependencyAnalyzer
{
    public record AssemblyReference(Assembly Assembly, Assembly Dependency);

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
    public IReadOnlyCollection<AssemblyReference> GetRecursiveDependencies(IEnumerable<Assembly> sources) =>
        sources.Aggregate(new List<AssemblyReference>(), (acc, asm) => Analyze(acc, asm, false));

    private List<AssemblyReference> Analyze(List<AssemblyReference> analyzed, Assembly asm, bool isSource)
    {
        var pad = isSource ? "  " : "    ";
        Console.WriteLine($"{pad}{asm.FullName}");

        var dependencies = asm.GetReferencedAssemblies()
            .Select(x => myLoader.TryLoadDependency(asm, x))
            .Where(x => x != null && FollowAssembly(x))
            .ToList();

        var analyzedAssemblies = analyzed
            .Select(x => x.Assembly)
            .Distinct()
            .ToList();

        var initialAcc = analyzed.ToList();
        initialAcc.Add(new AssemblyReference(asm, asm));

        var indirectDeps = dependencies
            .Where(x => analyzedAssemblies.Any(a => a.FullName != x.FullName))
            .Where(FollowAssembly)
            .Aggregate(initialAcc, (acc, asm) => Analyze(acc, asm, false));

        return dependencies
            .Select(x => new AssemblyReference(asm, x))
            .Concat(indirectDeps)
            .ToList();
    }

    private bool FollowAssembly(Assembly asm)
    {
        return myRelevantAssemblies.Any(p => p.IsMatch(asm.GetName().Name));
    }
}
