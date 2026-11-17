using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using NUnit.Framework;
using Plainion.Diagnostics;
using Plainion.GraphViz.Actors.Client;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance.Actors;

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
        public async Task AnalyzeInheritance(string targetFramework)
        {
            using (var client = new InheritanceClient())
            {
                client.HideHostWindow = true;

                var assemblyLocation = Path.Combine(myProjectHome, "testData", "DummyProject", "bin", "Debug", targetFramework, "DummyProject.Lib.dll");

                var types = await client.GetAllTypesAsync(assemblyLocation).WaitAsync(TimeSpan.FromSeconds(30));

                var response = await client.AnalyzeInheritanceAsync(
                    assemblyLocation: assemblyLocation,
                    ignoreDotNetTypes: true,
                    typeToAnalyse: types.Single(x => x.FullName == "DummyProject.Lib.IBuilder"))
                    .WaitAsync(TimeSpan.FromSeconds(30));

                string RemoveId(string nodeId) => nodeId.Split('#')[0];

                var edges = response.Edges
                    .Select(x => $"{RemoveId(x.Item1)} -> {RemoveId(x.Item2)}")
                    .ToList();
                Assert.That(edges, Contains.Item("DummyProject.Builder -> DummyProject.Lib.AbstractBuilder"));
            }
        }

        [Test]
        public async Task ClientsHaveIndependentHosts()
        {
            var assemblyLocation = Path.Combine(myProjectHome, "testData", "DummyProject", "bin", "Debug", TargetFrameworks[0], "DummyProject.Lib.dll");
            using var first = new InheritanceClient { HideHostWindow = true };
            using var second = new InheritanceClient { HideHostWindow = true };

            var results = await Task.WhenAll(first.GetAllTypesAsync(assemblyLocation), second.GetAllTypesAsync(assemblyLocation))
                .WaitAsync(TimeSpan.FromSeconds(30));
            Assert.That(results.All(types => types.Any(x => x.FullName == "DummyProject.Lib.IBuilder")), Is.True);

            first.Dispose();

            var types = await second.GetAllTypesAsync(assemblyLocation).WaitAsync(TimeSpan.FromSeconds(30));
            Assert.That(types.Any(x => x.FullName == "DummyProject.Lib.IBuilder"), Is.True);
        }

        [Test]
        public void HostExitFailsPendingRequest()
        {
            using var client = new ExitingClient { HideHostWindow = true };

            var error = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await client.ExitAsync().WaitAsync(TimeSpan.FromSeconds(30)));

            Assert.That(error.Message, Does.Contain("exited with code 23"));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task CancellationBeforeDispatchSkipsRequest(bool cancelBeforeStartup)
        {
            var assemblyLocation = Path.Combine(myProjectHome, "testData", "DummyProject", "bin", "Debug", TargetFrameworks[0], "DummyProject.Lib.dll");
            using var client = new InheritanceClient { HideHostWindow = true };
            using var cancellation = new CancellationTokenSource();
            if (cancelBeforeStartup)
            {
                cancellation.Cancel();
            }

            var pending = client.GetAllTypesAsync(assemblyLocation, cancellation.Token);
            if (!cancelBeforeStartup)
            {
                Assert.That(pending.IsCompleted, Is.False);
            }
            cancellation.Cancel();

            Assert.That(await pending.WaitAsync(TimeSpan.FromSeconds(30)), Is.Null);

            var types = await client.GetAllTypesAsync(assemblyLocation).WaitAsync(TimeSpan.FromSeconds(30));
            Assert.That(types.Any(x => x.FullName == "DummyProject.Lib.IBuilder"), Is.True);
        }

        private sealed class ExitingClient : ActorClientBase
        {
            public Task<object> ExitAsync() => ProcessAsync(typeof(ExitingActor), "exit", default);
        }
    }

    public sealed class ExitingActor : ReceiveActor
    {
        public ExitingActor()
        {
            Receive<string>(_ => Environment.Exit(23));
        }
    }
}
