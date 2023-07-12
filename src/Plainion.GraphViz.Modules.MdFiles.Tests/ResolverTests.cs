using System.IO;
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
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1\chapter.md")]
        public void Website_Is_External_Link(string root, string file)
        {
            var resolvedLink = myResolver.ResolveLink(@"http://www.google.de", file, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<ExternalLink>(resolvedLink);
                Assert.AreEqual(@"http://www.google.de/", resolvedLink.Uri.AbsoluteUri);
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1\chapter.md")]
        public void UncPath_Is_External_Link(string root, string file)
        {
            var resolvedLink = myResolver.ResolveLink(@"\\MyShare\Folder", file, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<ExternalLink>(resolvedLink);
                Assert.AreEqual(@"\\myshare\Folder", resolvedLink.Uri.LocalPath);
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1\chapter.md")]
        public void Broken_UncPath_Is_External_Link(string root, string file)
        {
            // UNC path are not supported by markdown and therefore wrongly parsed.
            var resolvedLink = myResolver.ResolveLink(@"\MyShare\Folder", file, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<ExternalLink>(resolvedLink);
                Assert.AreEqual(@"\\myshare\Folder", resolvedLink.Uri.LocalPath);
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1\chapter.md")]
        public void ExplicitPath_Is_External_Link(string root, string file)
        {
            var resolvedLink = myResolver.ResolveLink(@"C:\Project X\Usermanual\introduction.md", file, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<ExternalLink>(resolvedLink);
                Assert.AreEqual(@"C:\Project X\Usermanual\introduction.md", resolvedLink.Uri.LocalPath);
            });
        }

        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1\chapter.md")]
        public void Anchor_Is_InternalLink(string root, string file)
        {
            var resolvedLink = myResolver.ResolveLink(@"#note", file, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<InternalLink>(resolvedLink);
                Assert.AreEqual(@$"{file}#note", resolvedLink.Uri.LocalPath);
            });
        }

        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1\chapter.md")]
        public void RelativePath_With_Anchor_Is_InternalLink2(string root, string file)
        {
            var resolvedLink = myResolver.ResolveLink(@"../Chapter 2/chapter#note", file, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<InternalLink>(resolvedLink);
                Assert.AreEqual(@"C:\Project X\Usermanual\Chapter 2\chapter#note", resolvedLink.Uri.LocalPath);
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1\chapter.md")]
        public void RelativePath_Is_InternalLink(string root, string file)
        {
            var currentDir = Path.GetDirectoryName(file);
            var resolvedLink = myResolver.ResolveLink(@"introduction.md", file, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<InternalLink>(resolvedLink);
                Assert.AreEqual(@$"{currentDir}\introduction.md", resolvedLink.Uri.LocalPath);
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1\chapter.md")]
        public void RelativePath_To_CurrentDirectory_Is_InternalLink(string root, string file)
        {
            var currentDir = Path.GetDirectoryName(file);
            var resolvedLink = myResolver.ResolveLink(@"./introduction.md", file, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<InternalLink>(resolvedLink);
                Assert.AreEqual(@$"{currentDir}\introduction.md", resolvedLink.Uri.LocalPath);
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1\chapter.md")]
        public void RelativePath_To_SubDirectory_Is_InternalLink(string root, string file)
        {
            var currentDir = Path.GetDirectoryName(file);
            var resolvedLink = myResolver.ResolveLink(@"/SubChapter/introduction.md", file, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<InternalLink>(resolvedLink);
                Assert.AreEqual(@$"{currentDir}\SubChapter\introduction.md", resolvedLink.Uri.LocalPath);
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1\chapter.md")]
        public void RelativePath_To_RootDirectory_Is_InternalLink(string root, string file)
        {
            var resolvedLink = myResolver.ResolveLink(@"../introduction.md", file, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<InternalLink>(resolvedLink);
                Assert.AreEqual(@$"{root}\introduction.md", resolvedLink.Uri.LocalPath);
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1\chapter.md")]
        public void RelativePath_To_Outside_RootDirectory_Is_ExternalLink(string root, string file)
        {
            var resolvedLink = myResolver.ResolveLink(@"../../introduction.md", file, root);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<ExternalLink>(resolvedLink);
                Assert.AreEqual(@$"C:\Project X\introduction.md", resolvedLink.Uri.LocalPath);
            });
        }
    }
}