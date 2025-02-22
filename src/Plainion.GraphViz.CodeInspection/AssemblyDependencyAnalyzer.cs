using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Plainion.GraphViz.CodeInspection.AssemblyLoader;

namespace Plainion.GraphViz.CodeInspection;

public class AssemblyDependencyAnalyzer(IAssemblyLoader loader, IEnumerable<string> relevantAssemblies)
{
    public record AssemblyReference(Assembly Assembly, Assembly Dependency);

    private readonly IAssemblyLoader myLoader = loader;
    private readonly IEnumerable<Wildcard> myRelevantAssemblies = relevantAssemblies.Select(p => new Wildcard(p)).ToList();

    /// <summary>
    /// Returns the dependencies of the given assemblies recursively. Only follows the relevant assemblies passed to the constructor.
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

    private bool FollowAssembly(Assembly asm) =>
        myRelevantAssemblies.Any(p => p.IsMatch(asm.GetName().Name));
}
