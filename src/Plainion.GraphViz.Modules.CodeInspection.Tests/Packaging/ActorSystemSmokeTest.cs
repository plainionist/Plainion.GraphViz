using System;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Actors;

namespace Plainion.GraphViz.Modules.CodeInspection.Tests.Packaging
{
    [TestFixture]
    class ActorSystemSmokeTest
    {
        [Test]
        public void VerifyAkkaCommunicationWorks()
        {
            using (var client = new PackageAnalysisClient())
            {
                client.HideHostWindow = true;

                var home = Path.GetDirectoryName(GetType().Assembly.Location);
                // Packaging spec uses "." as "AssemblyRoot" so set current working directory to
                // current execution folder
                Environment.CurrentDirectory = home;

                var spec = Path.Combine(home, "TestData", "Packaging.xaml");
                Assert.That(File.Exists(spec), Is.True, "Spec file not found");

                var request = new AnalysisRequest
                {
                    Spec = File.ReadAllText(spec),
                    PackagesToAnalyze = null,
                    UsedTypesOnly = false,
                    CreateClustersForNamespaces = false
                };

                var response = client.AnalyseAsync(request, CancellationToken.None).Result;

                // With this test we basically just verify that nothing crashed so we just check for 
                // some result other than exception
                Assert.That(response.Nodes.Count(), Is.Not.EqualTo(0));
            }
        }
    }
}
