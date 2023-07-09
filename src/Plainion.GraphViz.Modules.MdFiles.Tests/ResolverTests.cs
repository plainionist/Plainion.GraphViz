using NUnit.Framework;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Resolver;

namespace Plainion.GraphViz.Modules.MdFiles.Tests
{
    [TestFixture]
    internal class ResolverTests
    {
        private ILinkResolver myResolver;

        [SetUp]
        public void Setup()
        {
            myResolver = new LinkResolver();
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1")]
        public void Website_Is_External_Link(string root, string currentDir)
        {
            var resolvedLink = myResolver.ResolveLink(@"http:\\www.google.de", currentDir, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<ExternalLink>(resolvedLink);
                Assert.AreEqual(resolvedLink.Uri, @"http:\\www.google.de");
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1")]
        public void UncPath_Is_External_Link(string root, string currentDir)
        {
            var resolvedLink = myResolver.ResolveLink(@"\\MyShare\Folder", currentDir, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<ExternalLink>(resolvedLink);
                Assert.AreEqual(resolvedLink.Uri, @"\\MyShare\Folder");
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1")]
        public void Broken_UncPath_Is_External_Link(string root, string currentDir)
        {
            // UNC path are not supported by markdown and therefore wrongly parsed.
            var resolvedLink = myResolver.ResolveLink(@"\MyShare\Folder", currentDir, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<ExternalLink>(resolvedLink);
                Assert.AreEqual(resolvedLink.Uri, @"\\MyShare\Folder");
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1")]
        public void ExplicitPath_Is_External_Link(string root, string currentDir)
        {
            var resolvedLink = myResolver.ResolveLink(@"C:\Project X\Usermanual\introduction.md", currentDir, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<ExternalLink>(resolvedLink);
                Assert.AreEqual(resolvedLink.Uri, @"C:\Project X\Usermanual\introduction.md");
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1")]
        public void RelativePath_Is_InternalLink(string root, string currentDir)
        {
            var resolvedLink = myResolver.ResolveLink(@"introduction.md", currentDir, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<InternalLink>(resolvedLink);
                Assert.AreEqual(resolvedLink.Uri, @$"{currentDir}\introduction.md");
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1")]
        public void RelativePath_To_CurrentDirectory_Is_InternalLink(string root, string currentDir)
        {
            var resolvedLink = myResolver.ResolveLink(@"./introduction.md", currentDir, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<InternalLink>(resolvedLink);
                Assert.AreEqual(resolvedLink.Uri, @$"{currentDir}\introduction.md");
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1")]
        public void RelativePath_To_SubDirectory_Is_InternalLink(string root, string currentDir)
        {
            var resolvedLink = myResolver.ResolveLink(@"/SubChapter/introduction.md", currentDir, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<InternalLink>(resolvedLink);
                Assert.AreEqual(resolvedLink.Uri, @$"{currentDir}\SubChapter\introduction.md");
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1")]
        public void RelativePath_To_RootDirectory_Is_InternalLink(string root, string currentDir)
        {
            var resolvedLink = myResolver.ResolveLink(@"../introduction.md", currentDir, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<InternalLink>(resolvedLink);
                Assert.AreEqual(resolvedLink.Uri, @$"{root}\introduction.md");
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1")]
        public void RelativePath_To_Outside_RootDirectory_Is_ExternalLink(string root, string currentDir)
        {
            var resolvedLink = myResolver.ResolveLink(@"../../introduction.md", currentDir, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<ExternalLink>(resolvedLink);
                Assert.AreEqual(resolvedLink.Uri, @$"C:\Project X\introduction.md");
            });
        }
    }
}