using System;
using System.Collections.Generic;
using System.Linq;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    static class DotNetFrameworkVersionExtensions
    {
        private static readonly Dictionary<DotNetFrameworkVersion, Version> myFrameworkVersionToStandardMap = new()
        {
            { new DotNetFrameworkVersion(DotNetFrameworkType.Framework, new Version(1, 1)), null },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Framework, new Version(2, 0)), null },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Framework, new Version(3, 5)), null },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Framework, new Version(4, 0)), null },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Framework, new Version(4, 5)), new Version(1, 1) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Framework, new Version(4, 5, 1)), new Version(1, 2) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Framework, new Version(4, 5, 2)), new Version(1, 2) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Framework, new Version(4, 6)), new Version(1, 3) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Framework, new Version(4, 6, 1)), new Version(2, 0) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Framework, new Version(4, 6, 2)), new Version(2, 0) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Framework, new Version(4, 7)), new Version(2, 0) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Framework, new Version(4, 7, 1)), new Version(2, 0) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Framework, new Version(4, 7, 2)), new Version(2, 0) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Framework, new Version(4, 8)), new Version(2, 0) },

            { new DotNetFrameworkVersion(DotNetFrameworkType.Core, new Version(1, 0)), new Version(1, 6) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Core, new Version(1, 1)), new Version(1, 6) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Core, new Version(2, 0)), new Version(2, 0) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Core, new Version(2, 1)), new Version(2, 0) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Core, new Version(2, 2)), new Version(2, 0) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Core, new Version(3, 0)), new Version(2, 1) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Core, new Version(3, 1)), new Version(2, 1) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Core, new Version(5, 0)), new Version(2, 1) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Core, new Version(6, 0)), new Version(2, 1) },

            { new DotNetFrameworkVersion(DotNetFrameworkType.Standard, new Version(1, 0)), new Version(1, 0) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Standard, new Version(1, 1)), new Version(1, 1) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Standard, new Version(1, 2)), new Version(1, 2) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Standard, new Version(1, 3)), new Version(1, 3) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Standard, new Version(1, 4)), new Version(1, 4) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Standard, new Version(1, 5)), new Version(1, 5) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Standard, new Version(1, 6)), new Version(1, 6) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Standard, new Version(2, 0)), new Version(2, 0) },
            { new DotNetFrameworkVersion(DotNetFrameworkType.Standard, new Version(2, 1)), new Version(2, 1) },
        };

        public static Version TryGetNetStandardVersion(this DotNetFrameworkVersion self)
        {
            Contract.RequiresNotNull(self, nameof(self));

            return myFrameworkVersionToStandardMap.TryGetValue(self, out Version version) ? version : null;
        }

        public static IReadOnlyCollection<DotNetFrameworkVersion> GetCompatibleFrameworkVersions(this DotNetFrameworkVersion self)
        {
            Contract.RequiresNotNull(self, nameof(self));

            var standardVersion = self.TryGetNetStandardVersion();

            return myFrameworkVersionToStandardMap.Keys
                .Where(x => (x.Framework == self.Framework && x.Version <= self.Version)
                    || (standardVersion != null && x.Framework == DotNetFrameworkType.Standard && x.Version <= standardVersion))
                .ToList();
        }
    }
}
