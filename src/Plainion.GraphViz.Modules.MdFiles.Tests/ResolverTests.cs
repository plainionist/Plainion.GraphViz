using NUnit.Framework;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Resolver;

namespace Plainion.GraphViz.Modules.MdFiles.Tests
{
    [TestFixture]
    internal class ResolverTests
    {
        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1")]
        public void Website_Is_External_Link(string root, string currentDir)
        {
            var resolver = new LinkResolver();
            var resolvedLink = resolver.ResolveLink(@"http:\\www.google.de", currentDir, root);

            Assert.IsTrue(resolvedLink is ExternalLink);
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1")]
        public void UncPath_Is_External_Link(string root, string currentDir)
        {
            var resolver = new LinkResolver();
            var resolvedLink = resolver.ResolveLink(@"\\MyShare\Folder", currentDir, root);

            Assert.IsTrue(resolvedLink is ExternalLink);
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1")]
        public void ExplicitPath_Is_External_Link(string root, string currentDir)
        {
            var resolver = new LinkResolver();
            var resolvedLink = resolver.ResolveLink(@"C:\Project X\Usermanual\introduction.md", currentDir, root);

            Assert.IsTrue(resolvedLink is ExternalLink);
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1")]
        public void RelativePath_Is_InternalLink(string root, string currentDir)
        {
            var resolver = new LinkResolver();
            var resolvedLink = resolver.ResolveLink(@"introduction.md", currentDir, root);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(resolvedLink is InternalLink);
                Assert.IsTrue(resolvedLink.Url.Equals(@$"{currentDir}\introduction.md"));
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1")]
        public void RelativePath_To_CurrentDirectory_Is_InternalLink(string root, string currentDir)
        {
            var resolver = new LinkResolver();
            var resolvedLink = resolver.ResolveLink(@"./introduction.md", currentDir, root);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(resolvedLink is InternalLink);
                Assert.IsTrue(resolvedLink.Url.Equals(@$"{currentDir}\introduction.md"));
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1")]
        public void RelativePath_To_SubDirectory_Is_InternalLink(string root, string currentDir)
        {
            var resolver = new LinkResolver();
            var resolvedLink = resolver.ResolveLink(@"/SubChapter/introduction.md", currentDir, root);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(resolvedLink is InternalLink);
                Assert.IsTrue(resolvedLink.Url.Equals(@$"{currentDir}\SubChapter\introduction.md"));
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1")]
        public void RelativePath_To_RootDirectory_Is_InternalLink(string root, string currentDir)
        {
            var resolver = new LinkResolver();
            var resolvedLink = resolver.ResolveLink(@"../introduction.md", currentDir, root);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(resolvedLink is InternalLink);
                Assert.IsTrue(resolvedLink.Url.Equals(@$"{root}\introduction.md"));
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1")]
        public void RelativePath_To_Outside_RootDirectory_Is_ExternalLink(string root, string currentDir)
        {
            var resolver = new LinkResolver();
            var resolvedLink = resolver.ResolveLink(@"../../introduction.md", currentDir, root);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(resolvedLink is ExternalLink);
                Assert.IsTrue(resolvedLink.Url.Equals(@$"C:\Project X\introduction.md"));
            });
        }
    }
}