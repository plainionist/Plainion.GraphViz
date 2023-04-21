using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using Plainion;

namespace Plainion.GraphViz.Modules.CodeInspection.Common.Analyzers
{
    public enum DotNetFrameworkType
    {
        Unknown,
        Standard,
        Framework,
        Core,
    }

    public record DotNetFrameworkVersion : IComparable<DotNetFrameworkVersion>
    {
        public DotNetFrameworkVersion(DotNetFrameworkType framework, Version version)
        {
            Contract.RequiresNotNull(framework, nameof(framework));
            Contract.RequiresNotNull(version, nameof(version));

            Framework = framework;
            Version = version;
        }

        public DotNetFrameworkType Framework { get; }

        public Version Version { get; }

        public override string ToString() => $"{Framework} {Version}";

        // Format expected comparable to NuGet version folder name
        public static DotNetFrameworkVersion TryParse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            value = value.Trim().ToLower();

            var fwTypePart = string.Concat(value.TakeWhile(char.IsLetter));
            var versionPart = value.Substring(fwTypePart.Length);

            var fwType = ParseFrameworkType(fwTypePart, versionPart);
            var version = TryParseVersion(fwType, versionPart);

            return fwType != DotNetFrameworkType.Unknown && version != null ? new DotNetFrameworkVersion(fwType, version) : null;

            static DotNetFrameworkType ParseFrameworkType(string fwName, string version) =>
                 fwName switch
                 {
                     "netstandard" => DotNetFrameworkType.Standard,
                     "netcoreapp" => DotNetFrameworkType.Core,
                     "net" => version.Contains('.') ? DotNetFrameworkType.Core : DotNetFrameworkType.Framework,
                     _ => DotNetFrameworkType.Unknown,
                 };
        }

        private static Version TryParseVersion(DotNetFrameworkType identifier, string versionPart)
        {
            if (identifier == DotNetFrameworkType.Unknown)
            {
                return null;
            }

            if (!versionPart.All(c => char.IsDigit(c) || c == '.'))
            {
                return null;
            }

            Version version = null;
            if (!Version.TryParse(versionPart, out version))
            {
                return new Version(string.Join(".", versionPart.Select(c => c.ToString())));
            }

            return version;
        }

        public int CompareTo(DotNetFrameworkVersion other)
        {
            if (other == null)
            {
                return 1;
            }

            if (Equals(other))
            {
                return 0;
            }

            if (Framework == DotNetFrameworkType.Unknown || Framework == DotNetFrameworkType.Unknown)
            {
                return 0;
            }

            if (Framework.CompareTo(other.Framework) != 0)
            {
                if (Framework == DotNetFrameworkType.Standard)
                {
                    var netStandardVersion = other.TryGetNetStandardVersion();
                    return netStandardVersion != null && netStandardVersion.CompareTo(Version) >= 0 ? 1 : -1;
                }

                if (other.Framework == DotNetFrameworkType.Standard)
                {
                    var netStandardVersion = this.TryGetNetStandardVersion();
                    return netStandardVersion != null && netStandardVersion.CompareTo(other.Version) >= 0 ? 1 : -1;
                }
            }

            return Version.CompareTo(other.Version);
        }

        /// <summary>
        /// Read from TargetFrameworkAttribute.
        /// </summary>
        // ".NETStandard,Version=v2.0"
        // ".NETFramework,Version=v4.5"
        public static DotNetFrameworkVersion TryParse(Assembly assembly)
        {
            var attr = assembly.GetCustomAttribute<TargetFrameworkAttribute>();
            if (attr == null)
            {
                return null;
            }

            var parts = attr.FrameworkName.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                return null;
            }

            var fwType = ParseFrameworkType(parts[0]);
            if (fwType != DotNetFrameworkType.Unknown)
            {
                return null;
            }

            var version = new Version(parts[1].Substring("Version=v".Length));

            return new DotNetFrameworkVersion(fwType, version);

            static DotNetFrameworkType ParseFrameworkType(string name) =>
                 name.ToLower() switch
                 {
                     ".netstandard" => DotNetFrameworkType.Standard,
                     ".netcoreapp" => DotNetFrameworkType.Core,
                     ".netframework" => DotNetFrameworkType.Framework,
                     _ => DotNetFrameworkType.Unknown,
                 };
        }
    }
}
