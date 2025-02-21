using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Plainion.GraphViz.Modules.Markdown.Dependencies.Analyzer;
using Plainion.GraphViz.Modules.Markdown.Dependencies.Analyzer.Parser;
using Plainion.GraphViz.Modules.Markdown.Dependencies.Analyzer.Resolver;
using Plainion.GraphViz.Modules.Markdown.Dependencies.Analyzer.Verifier;

namespace Plainion.GraphViz.Modules.Markdown.Tests
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
            usermanual.AppendLine(@"[](http://www.google.de)");
            usermanual.AppendLine(@"[](\\NetworkShare\folderA)");
            usermanual.AppendLine("[](Appendix/AppendixA.md)");
            usermanual.AppendLine("![](Appendix/chart.jpg)");

            myFileSystem.AddFile(@"C:\Project X\Documentation\Usermanual.md", new MockFileData(usermanual.ToString()));
            myFileSystem.AddFile(@"C:\Project X\Documentation\Introduction.md", new MockFileData("[](../Changelog)"));
            myFileSystem.AddFile(@"C:\Project X\Documentation\Chapter1.md", new MockFileData("[](Chapter2#note)"));
            myFileSystem.AddFile(@"C:\Project X\Documentation\Chapter2.md", new MockFileData("[](../Readme.md)"));
            myFileSystem.AddFile(@"C:\Project X\Readme.md", new MockFileData(""));
            myFileSystem.AddFile(@"C:\Project X\Documentation\Appendix\chart.jpg)", new MockFileData(""));

            var doc = await myAnalyzer.AnalyzeAsync(@"C:\Project X\Documentation\", myCts.Token);

            Assert.Multiple(() =>
            {
                Assert.That(doc.FailedItems, Is.Empty);
                Assert.That(doc.Files, Is.Not.Empty);
                Assert.That(4, Is.EqualTo(doc.Files.Count));

                Assert.That(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Usermanual.md").ValidInternalMDRefs, Is.Not.Empty);
                Assert.That(2, Is.EqualTo(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Usermanual.md").ValidInternalMDRefs.Count));
                Assert.That(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Usermanual.md").InvalidInternalMDRefs, Is.Not.Empty);
                Assert.That(1, Is.EqualTo(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Usermanual.md").InvalidInternalMDRefs.Count));
                Assert.That(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Usermanual.md").ValidExternalMDRefs, Is.Empty);
                Assert.That(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Usermanual.md").InvalidExternalMDRefs, Is.Not.Empty);

                Assert.That(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Introduction.md").ValidInternalMDRefs, Is.Empty);
                Assert.That(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Introduction.md").InvalidInternalMDRefs, Is.Empty);
                Assert.That(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Introduction.md").ValidExternalMDRefs, Is.Empty);
                Assert.That(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Introduction.md").InvalidExternalMDRefs, Is.Not.Empty);
                Assert.That(1, Is.EqualTo(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Introduction.md").InvalidExternalMDRefs.Count));

                Assert.That(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Chapter1.md").ValidInternalMDRefs, Is.Not.Empty);
                Assert.That(1, Is.EqualTo(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Chapter1.md").ValidInternalMDRefs.Count));
                Assert.That(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Chapter1.md").InvalidInternalMDRefs, Is.Empty);
                Assert.That(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Chapter1.md").ValidExternalMDRefs, Is.Empty);
                Assert.That(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Chapter1.md").InvalidExternalMDRefs, Is.Empty);

                Assert.That(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Chapter2.md").ValidInternalMDRefs, Is.Empty);
                Assert.That(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Chapter2.md").InvalidInternalMDRefs, Is.Empty);
                Assert.That(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Chapter2.md").ValidExternalMDRefs, Is.Not.Empty);
                Assert.That(1, Is.EqualTo(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Chapter2.md").ValidExternalMDRefs.Count));
                Assert.That(doc.Files.First(f => f.FullPath == @"C:\Project X\Documentation\Chapter2.md").InvalidExternalMDRefs, Is.Empty);
            });
        }

        [Test]
        public async Task Analyze_NonExistingDirectory()
        {
            var doc = await myAnalyzer.AnalyzeAsync(@"C:\MissingDirectory\", myCts.Token);

            Assert.Multiple(() =>
            {
                Assert.That(doc.FailedItems, Is.Not.Empty);
                Assert.That(doc.FailedItems.First().Exception, Is.InstanceOf<DirectoryNotFoundException>());
            });
        }

        [Test]
        public async Task Analyze_EmptyDirectory()
        {
            var doc = await myAnalyzer.AnalyzeAsync(@"C:\Temp\", myCts.Token);

            Assert.Multiple(() =>
            {
                Assert.That(doc.Files, Is.Empty);
                Assert.That(doc.FailedItems, Is.Empty);
            });
        }
    }
}