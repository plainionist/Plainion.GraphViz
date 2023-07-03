using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Parser;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Resolver;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Verifier;

namespace Plainion.GraphViz.Modules.MdFiles.Tests
{
    [TestFixture]
    internal class AnalyzerTests
    {
        [Test]
        public async Task Test_Analyzer()
        {
            var usermanual = new StringBuilder();
            usermanual.AppendLine("[](Introduction.md)");
            usermanual.AppendLine("[](Chapter1)");
            usermanual.AppendLine("[](Chapter1)");
            usermanual.AppendLine(@"[](http:\\www.google.de)");
            usermanual.AppendLine(@"[](\\NetworkShare\folderA)");
            usermanual.AppendLine("[](Appendix/AppendixA.md)");
            usermanual.AppendLine("![](Appendix/chart.jpg)");

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"C:\Project X\Documentation\Usermanual.md", new MockFileData(usermanual.ToString()) },
                { @"C:\Project X\Documentation\Introduction.md", new MockFileData("") },
                { @"C:\Project X\Documentation\Chapter1.md", new MockFileData("[](Chapter2.md)") },
                { @"C:\Project X\Documentation\Chapter2.md", new MockFileData("[](../Readme.md)") },
                { @"C:\Project X\Readme.md", new MockFileData("") },
                { @"C:\Project X\Documentation\Appendix\chart.jpg)", new MockFileData("") },
            });

            var parser = new MarkdigParser(fileSystem);
            var resolver = new LinkResolver();
            var verifier = new LinkVerifier(fileSystem);
            var analyzer = new MarkdownAnalyzer(fileSystem, parser, resolver, verifier);

            var cts = new CancellationTokenSource();
            var doc = await analyzer.AnalyzeAsync(@"C:\Project X\Documentation\", cts.Token);
            cts.Dispose();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(doc.Files.Any());
                Assert.IsTrue(!doc.FailedItems.Any());
                Assert.IsTrue(doc.Files.Count() == 4);
                Assert.IsTrue(doc.Files.First(f
                    => f.FullPath == @"C:\Project X\Documentation\Usermanual.md").ValidMDReferences.Any());
                Assert.IsTrue(doc.Files.First(f
                    => f.FullPath == @"C:\Project X\Documentation\Usermanual.md").ValidMDReferences.Count() == 2);
                Assert.IsTrue(doc.Files.First(f
                    => f.FullPath == @"C:\Project X\Documentation\Usermanual.md").InvalidMDReferences.Any());
                Assert.IsTrue(doc.Files.First(f
                    => f.FullPath == @"C:\Project X\Documentation\Usermanual.md").InvalidMDReferences.Count() == 1);

                Assert.IsTrue(!doc.Files.First(f
                    => f.FullPath == @"C:\Project X\Documentation\Introduction.md").ValidMDReferences.Any());
                Assert.IsTrue(!doc.Files.First(f
                    => f.FullPath == @"C:\Project X\Documentation\Introduction.md").InvalidMDReferences.Any());

                Assert.IsTrue(doc.Files.First(f
                    => f.FullPath == @"C:\Project X\Documentation\Chapter1.md").ValidMDReferences.Any());
                Assert.IsTrue(doc.Files.First(f
                    => f.FullPath == @"C:\Project X\Documentation\Chapter1.md").ValidMDReferences.Count() == 1);
                Assert.IsTrue(!doc.Files.First(f
                    => f.FullPath == @"C:\Project X\Documentation\Chapter1.md").InvalidMDReferences.Any());

                Assert.IsTrue(!doc.Files.First(f
                    => f.FullPath == @"C:\Project X\Documentation\Chapter2.md").ValidMDReferences.Any());
                Assert.IsTrue(!doc.Files.First(f
                    => f.FullPath == @"C:\Project X\Documentation\Chapter2.md").InvalidMDReferences.Any());
            });
        }

        [Test]
        public async Task Test_Analyzer_With_InvalidDirectory()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { });
            var parser = new MarkdigParser(fileSystem);
            var resolver = new LinkResolver();
            var verifier = new LinkVerifier(fileSystem);
            var analyzer = new MarkdownAnalyzer(fileSystem, parser, resolver, verifier);

            var cts = new CancellationTokenSource();
            var doc = await analyzer.AnalyzeAsync(@"C:\Project X\Documentation\", cts.Token);
            cts.Dispose();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(doc.FailedItems.Any());
                Assert.IsTrue(doc.FailedItems.First().Exception is DirectoryNotFoundException);
            });
        }
    }
}