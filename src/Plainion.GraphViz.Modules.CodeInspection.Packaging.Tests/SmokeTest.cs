using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Plainion.Diagnostics;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Actors;

namespace Plainion.GraphViz.Modules.CodeInspection.Tests
{
    [TestFixture]
    class SmokeTest
    {
        private static readonly string myProjectHome = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(SmokeTest).Assembly.Location), "..", ".."));

        private static readonly string[] TargetFrameworks = ["net8.0", "net48"];

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

        [TestCaseSource(nameof(TargetFrameworks))]
        public async Task AnalyzePackageDependencies(string targetFramework)
        {
            using (var client = new PackageAnalysisClient())
            {
                client.HideHostWindow = true;

                var spec = @"
                    <SystemPackaging AssemblyRoot=""."" xmlns=""http://github.com/ronin4net/plainion/GraphViz/Packaging/Spec"">
                        <Package Name=""DummyProject"">
                            <Include Pattern=""DummyProject.dll"" />
                            <Include Pattern=""DummyProject.Lib.dll"" />
                        </Package>
                    </SystemPackaging>";

                var request = new AnalysisRequest
                {
                    Spec = spec,
                    PackagesToAnalyze = null
                };

                // "AssemblyRoot" uses relative path
                Environment.CurrentDirectory = Path.Combine(myProjectHome, "testData", "DummyProject", "bin", "Debug", targetFramework);

                var response = await client.AnalyseAsync(request);

                var edges = response.Edges
                    .Select(x => $"{x.Item1} -> {x.Item2}")
                    .ToList();
                Assert.That(edges, Contains.Item("DummyProject.Component -> DummyProject.Lib.IBuilder"));
            }
        }

    }
}
