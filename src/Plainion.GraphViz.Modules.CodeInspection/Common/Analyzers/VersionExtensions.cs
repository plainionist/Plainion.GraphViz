using System;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    internal static class VersionExtensions
    {
        internal static bool Matches(this Version lhs, Version rhs, VersionMatchingStrategy strategy) =>
            lhs != null && rhs != null && strategy switch
            {
                VersionMatchingStrategy.Exact => lhs.Major == rhs.Major && lhs.Minor == rhs.Minor && lhs.Build == rhs.Build,
                VersionMatchingStrategy.SemanticVersion => lhs.Major == rhs.Major && lhs.Minor <= rhs.Minor,
                _ => false,
            };
    }
}
