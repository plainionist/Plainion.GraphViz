﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Plainion.Diagnostics;
using Plainion.GraphViz.Modules.CodeInspection.CallTree.Actors;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance.Actors;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Actors;
using Plainion.GraphViz.Modules.CodeInspection.PathFinder.Actors;

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

        [TestCaseSource(nameof(TargetFrameworks))]
        public async Task AnalyzeCallTree(string targetFramework)
        {
            using (var client = new CallTreeClient())
            {
                client.HideHostWindow = true;

                var assemblyLocation = Path.Combine(myProjectHome, "testData", "DummyProject", "bin", "Debug", targetFramework);

                var configFile = Path.Combine(Path.GetTempPath(), "GraphViz.AnalyzeCallTree.json");
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

                var responseFile = await client.AnalyzeAsync(new CallTreeRequest
                {
                    ConfigFile = configFile,
                    AssemblyReferencesOnly = false,
                    StrictCallsOnly = true
                });

                var response = File.ReadAllText(responseFile);

                Assert.That(response, Is.Not.Empty);
                Assert.That(response, Contains.Substring(@"""DummyProject.Component.Init"" -> ""DummyProject.Lib.IBuilder.Build"""));
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
