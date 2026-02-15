using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Plainion.Diagnostics;
using Plainion.GraphViz.Modules.CodeInspection.PathFinder.Actors;

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

        [TestCaseSource(nameof(TargetFrameworks))]
        public async Task AnalyzePath(string targetFramework)
        {
            using (var client = new PathFinderClient())
            {
                client.HideHostWindow = true;

                var assemblyLocation = Path.Combine(myProjectHome, "testData", "DummyProject", "bin", "Debug", targetFramework);

                var configFile = Path.Combine(Path.GetTempPath(), "GraphViz.AnalyzeCallTree.json");
                File.WriteAllText(configFile, @"
                    {
                        ""binFolder"": """ + assemblyLocation.Replace('\\', '/') + @""",
                        ""keepInnerAssemblyDependencies"": true,
                        ""keepSourceAssemblyClusters"": true,
                        ""keepTargetAssemblyClusters"": true,
                        ""sources"": [ ""DummyProject.dll"" ],
                        ""targets"": [ ""DummyProject.Lib.dll"" ],
                        ""relevantAssemblies"": [ ""DummyProject*"" ]
                    }");

                var responseFile = await client.AnalyzeAsync(new PathFinderRequest
                {
                    ConfigFile = configFile,
                    AssemblyReferencesOnly = false
                });

                var response = File.ReadAllText(responseFile);

                Assert.That(response, Is.Not.Empty);
                Assert.That(response, Contains.Substring(@"""DummyProject.Component"" -> ""DummyProject.Lib.IBuilder"""));
            }
        }
    }
}
