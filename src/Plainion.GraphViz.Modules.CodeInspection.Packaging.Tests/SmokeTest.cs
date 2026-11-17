using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Plainion.Diagnostics;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Actors;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Spec;

namespace Plainion.GraphViz.Modules.CodeInspection.Tests
{
    [TestFixture]
    class SmokeTest
    {
        private static readonly string myProjectHome = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(SmokeTest).Assembly.Location), "..", ".."));

        private static readonly string[] TargetFrameworks = ["net10.0", "net48"];

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            using (var writer = new StringWriter())
            {
                var exitCode = Processes.Execute(new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "build DummyProject.sln",
                    WorkingDirectory = Path.Combine(myProjectHome, "testData"),
                    UseShellExecute = true
                }, writer, writer);

                Assert.That(exitCode, Is.EqualTo(0), writer.ToString());
            }
        }

        [TestCase(false, "*Test*.dll", true, true)]
        [TestCase(false, "*Lib.dll", true, false)]
        [TestCase(true, "*Test*.dll", false, false)]
        public async Task ExclusionsMatchOriginalRoot(bool absoluteRoot, string exclusion, bool hasComponent, bool hasBuilder)
        {
            var assemblyLocation = Path.Combine(myProjectHome, "testData", "DummyProject", "bin", "Debug", TargetFrameworks[0]);
            var spec = new SystemPackaging { AssemblyRoot = absoluteRoot ? assemblyLocation : "." };
            var package = new Package { Name = "DummyProject" };
            package.Patterns.Add(new Include { Pattern = "DummyProject*.dll" });
            package.Patterns.Add(new Exclude { Pattern = exclusion });
            spec.Packages.Add(package);
            using var client = new PackageAnalysisClient { HideHostWindow = true };

            var originalDirectory = Environment.CurrentDirectory;
            try
            {
                Environment.CurrentDirectory = assemblyLocation;
                var response = await client.AnalyseAsync(new AnalysisRequest { Spec = SpecUtils.Serialize(spec) })
                    .WaitAsync(TimeSpan.FromSeconds(30));

                Assert.That(response.Nodes.Contains("DummyProject.Component"), Is.EqualTo(hasComponent));
                Assert.That(response.Nodes.Contains("DummyProject.Lib.IBuilder"), Is.EqualTo(hasBuilder));
            }
            finally
            {
                Environment.CurrentDirectory = originalDirectory;
            }
        }

        [TestCaseSource(nameof(TargetFrameworks))]
        public async Task AnalyzePackageDependencies(string targetFramework)
        {
            using (var client = new PackageAnalysisClient())
            {
                client.HideHostWindow = true;

                var spec = SpecUtils.Deserialize(@"
                    <SystemPackaging AssemblyRoot=""."" xmlns=""http://github.com/ronin4net/plainion/GraphViz/Packaging/Spec"">
                        <Package Name=""DummyProject"">
                            <Include Pattern=""DummyProject.dll"" />
                            <Include Pattern=""DummyProject.Lib.dll"" />
                        </Package>
                    </SystemPackaging>");
                spec.NetFramework = targetFramework == "net48";
                var assemblyLocation = Path.Combine(myProjectHome, "testData", "DummyProject", "bin", "Debug", targetFramework);
                spec.AssemblyRoot = assemblyLocation;

                var request = new AnalysisRequest
                {
                    Spec = SpecUtils.Serialize(spec),
                    PackagesToAnalyze = null
                };

                await client.AnalyseAsync(request).WaitAsync(TimeSpan.FromSeconds(30));

                spec.AssemblyRoot = ".";
                request.Spec = SpecUtils.Serialize(spec);

                var originalDirectory = Environment.CurrentDirectory;
                try
                {
                    Environment.CurrentDirectory = assemblyLocation;

                    var response = await client.AnalyseAsync(request).WaitAsync(TimeSpan.FromSeconds(30));

                    var edges = response.Edges
                        .Select(x => $"{x.Item1} -> {x.Item2}")
                        .ToList();
                    Assert.That(edges, Contains.Item("DummyProject.Component -> DummyProject.Lib.IBuilder"));
                }
                finally
                {
                    Environment.CurrentDirectory = originalDirectory;
                }
            }
        }

    }
}
