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

            Assert.IsInstanceOf<ExternalLink>(resolvedLink);
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1")]
        public void UncPath_Is_External_Link(string root, string currentDir)
        {
            var resolvedLink = myResolver.ResolveLink(@"\\MyShare\Folder", currentDir, root);

            Assert.IsInstanceOf<ExternalLink>(resolvedLink);
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1")]
        public void ExplicitPath_Is_External_Link(string root, string currentDir)
        {
            var resolvedLink = myResolver.ResolveLink(@"C:\Project X\Usermanual\introduction.md", currentDir, root);

            Assert.IsInstanceOf<ExternalLink>(resolvedLink);
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1")]
        public void RelativePath_Is_InternalLink(string root, string currentDir)
        {
            var resolvedLink = myResolver.ResolveLink(@"introduction.md", currentDir, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<InternalLink>(resolvedLink);
                Assert.AreEqual(resolvedLink.Url, @$"{currentDir}\introduction.md");
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
                Assert.AreEqual(resolvedLink.Url, @$"{currentDir}\introduction.md");
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
                Assert.AreEqual(resolvedLink.Url, @$"{currentDir}\SubChapter\introduction.md");
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
                Assert.AreEqual(resolvedLink.Url, @$"{root}\introduction.md");
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
                Assert.AreEqual(resolvedLink.Url, @$"C:\Project X\introduction.md");
            });
        }
    }
}