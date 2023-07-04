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
        private CancellationTokenSource myCts;
        private MockFileSystem myFileSystem;
        private IMarkdownParser myMarkdownParser;
        private ILinkResolver myLinkResolver;
        private ILinkVerifier myLinkVerifier;
        private MarkdownAnalyzer myAnalyzer;

        [SetUp]
        public void SetUp()
        {
            myFileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { });
            myMarkdownParser = new MarkdigParser(myFileSystem);
            myLinkResolver = new LinkResolver();
            myLinkVerifier = new LinkVerifier(myFileSystem);
            myAnalyzer = new MarkdownAnalyzer(myFileSystem, myMarkdownParser, myLinkResolver, myLinkVerifier);
            myCts = new CancellationTokenSource();
        }

        [TearDown]
        public void TearDown()
        {
            myCts?.Dispose();
        }

        [Test]
        public async Task Analyze_ExistingDirectory()
        {
            var usermanual = new StringBuilder();
            usermanual.AppendLine("[](Introduction.md)");
            usermanual.AppendLine("[](Chapter1)");
            usermanual.AppendLine("[](Chapter1)");
            usermanual.AppendLine(@"[](http:\\www.google.de)");
            usermanual.AppendLine(@"[](\\NetworkShare\folderA)");
            usermanual.AppendLine("[](Appendix/AppendixA.md)");
            usermanual.AppendLine("![](Appendix/chart.jpg)");

            myFileSystem.AddFile(@"C:\Project X\Documentation\Usermanual.md", new MockFileData(usermanual.ToString()));
            myFileSystem.AddFile(@"C:\Project X\Documentation\Introduction.md", new MockFileData(""));
            myFileSystem.AddFile(@"C:\Project X\Documentation\Chapter1.md", new MockFileData("[](Chapter2.md)"));
            myFileSystem.AddFile(@"C:\Project X\Documentation\Chapter2.md", new MockFileData("[](../Readme.md)"));
            myFileSystem.AddFile(@"C:\Project X\Readme.md", new MockFileData(""));
            myFileSystem.AddFile(@"C:\Project X\Documentation\Appendix\chart.jpg)", new MockFileData(""));

            var doc = await myAnalyzer.AnalyzeAsync(@"C:\Project X\Documentation\", myCts.Token);

            Assert.Multiple(() =>
            {
                Assert.IsEmpty(doc.FailedItems);
                Assert.IsNotEmpty(doc.Files);
                Assert.AreEqual(doc.Files.Count, 4);

                Assert.IsNotEmpty(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Usermanual.md").ValidMDReferences);
                Assert.AreEqual(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Usermanual.md").ValidMDReferences.Count, 2);
                Assert.IsNotEmpty(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Usermanual.md").InvalidMDReferences);
                Assert.AreEqual(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Usermanual.md").InvalidMDReferences.Count, 1);

                Assert.IsEmpty(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Introduction.md").ValidMDReferences);
                Assert.IsEmpty(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Introduction.md").InvalidMDReferences);

                Assert.IsNotEmpty(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Chapter1.md").ValidMDReferences);
                Assert.AreEqual(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Chapter1.md").ValidMDReferences.Count, 1);
                Assert.IsEmpty(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Chapter1.md").InvalidMDReferences);

                Assert.IsEmpty(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Chapter2.md").ValidMDReferences);
                Assert.IsEmpty(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Chapter2.md").InvalidMDReferences);
            });
        }

        [Test]
        public async Task Analyze_NonExistingDirectory()
        {
            var doc = await myAnalyzer.AnalyzeAsync(@"C:\MissingDirectory\", myCts.Token);

            Assert.Multiple(() =>
            {
                Assert.IsNotEmpty(doc.FailedItems);
                Assert.IsInstanceOf<DirectoryNotFoundException>(doc.FailedItems.First().Exception);
            });
        }

        [Test]
        public async Task Analyze_EmptyDirectory()
        {
            var doc = await myAnalyzer.AnalyzeAsync(@"C:\Temp\", myCts.Token);

            Assert.Multiple(() =>
            {
                Assert.IsEmpty(doc.Files);
                Assert.IsEmpty(doc.FailedItems);
            });
        }
    }
}