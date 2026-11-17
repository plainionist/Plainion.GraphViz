using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Plainion.Diagnostics;
using Plainion.GraphViz.Modules.CodeInspection.CallTree.Actors;

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
        public async Task AnalyzeCallTree(string targetFramework)
        {
            using (var client = new CallTreeClient())
            {
                client.HideHostWindow = true;

                var assemblyLocation = Path.Combine(myProjectHome, "testData", "DummyProject", "bin", "Debug", targetFramework);

                var configFile = Path.GetTempFileName();
                string responseFile = null;
                try
                {
                    File.WriteAllText(configFile, @"
                    {
                        ""binFolder"": """ + assemblyLocation.Replace('\\', '/') + @""",
                        ""sources"": [ ""DummyProject.dll"" ],
                        ""targets"": [
                            {
                                ""assembly"": ""DummyProject.Lib.dll"",
                                ""type"": ""DummyProject.Lib.IBuilder"",
                                ""method"": ""Build""
                            }
                        ],
                        ""relevantAssemblies"": [ ""DummyProject*"" ]
                    }");

                    responseFile = await client.AnalyzeAsync(new CallTreeRequest
                    {
                        ConfigFile = configFile,
                        AssemblyReferencesOnly = false,
                        StrictCallsOnly = true
                    }).WaitAsync(TimeSpan.FromSeconds(30));

                    var response = File.ReadAllText(responseFile);

                    Assert.That(response, Is.Not.Empty);
                    Assert.That(response, Contains.Substring(@"""DummyProject.Component.Init"" -> ""DummyProject.Lib.IBuilder.Build"""));
                }
                finally
                {
                    File.Delete(configFile);
                    if (responseFile != null)
                    {
                        File.Delete(responseFile);
                    }
                }
            }
        }
    }
}
