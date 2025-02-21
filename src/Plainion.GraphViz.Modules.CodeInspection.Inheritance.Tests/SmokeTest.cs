using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Plainion.Diagnostics;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance.Actors;

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
        public async Task AnalyzeInheritance(string targetFramework)
        {
            using (var client = new InheritanceClient())
            {
                client.HideHostWindow = false;

                var assemblyLocation = Path.Combine(myProjectHome, "testData", "DummyProject", "bin", "Debug", targetFramework, "DummyProject.Lib.dll");

                var types = await client.GetAllTypesAsync(assemblyLocation);

                var response = await client.AnalyzeInheritanceAsync(
                    assemblyLocation: assemblyLocation,
                    ignoreDotNetTypes: true,
                    typeToAnalyse: types.Single(x => x.FullName == "DummyProject.Lib.IBuilder"));

                string RemoveId(string nodeId) => nodeId.Split('#')[0];

                var edges = response.Edges
                    .Select(x => $"{RemoveId(x.Item1)} -> {RemoveId(x.Item2)}")
                    .ToList();
                Assert.That(edges, Contains.Item("DummyProject.Builder -> DummyProject.Lib.AbstractBuilder"));
            }
        }
    }
}
