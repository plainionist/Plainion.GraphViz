using System.IO;
using NUnit.Framework;
using Plainion.GraphViz.Modules.Markdown.Analyzer.Resolver;

namespace Plainion.GraphViz.Modules.Markdown.Tests
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
                Assert.That(resolvedLink, Is.InstanceOf<ExternalLink>());
                Assert.That(@"http://www.google.de/", Is.EqualTo(resolvedLink.Uri.AbsoluteUri));
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1\chapter.md")]
        public void UncPath_Is_External_Link(string root, string file)
        {
            var resolvedLink = myResolver.ResolveLink(@"\\MyShare\Folder", file, root);

            Assert.Multiple(() =>
            {
                Assert.That(resolvedLink, Is.InstanceOf<ExternalLink>());
                Assert.That(@"\\myshare\Folder", Is.EqualTo(resolvedLink.Uri.LocalPath));
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
                Assert.That(resolvedLink, Is.InstanceOf<ExternalLink>());
                Assert.That(@"\\myshare\Folder", Is.EqualTo(resolvedLink.Uri.LocalPath));
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1\chapter.md")]
        public void ExplicitPath_Is_External_Link(string root, string file)
        {
            var resolvedLink = myResolver.ResolveLink(@"C:\Project X\Usermanual\introduction.md", file, root);

            Assert.Multiple(() =>
            {
                Assert.That(resolvedLink, Is.InstanceOf<ExternalLink>());
                Assert.That(@"C:\Project X\Usermanual\introduction.md", Is.EqualTo(resolvedLink.Uri.LocalPath));
            });
        }

        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1\chapter.md")]
        public void Anchor_Is_InternalLink(string root, string file)
        {
            var resolvedLink = myResolver.ResolveLink(@"#note", file, root);

            Assert.Multiple(() =>
            {
                Assert.That(resolvedLink, Is.InstanceOf<InternalLink>());
                Assert.That(@$"{file}#note", Is.EqualTo(resolvedLink.Uri.LocalPath));
            });
        }

        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1\chapter.md")]
        public void RelativePath_With_Anchor_Is_InternalLink2(string root, string file)
        {
            var resolvedLink = myResolver.ResolveLink(@"../Chapter 2/chapter#note", file, root);

            Assert.Multiple(() =>
            {
                Assert.That(resolvedLink, Is.InstanceOf<InternalLink>());
                Assert.That(@"C:\Project X\Usermanual\Chapter 2\chapter#note", Is.EqualTo(resolvedLink.Uri.LocalPath));
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
                Assert.That(resolvedLink, Is.InstanceOf<InternalLink>());
                Assert.That(@$"{currentDir}\introduction.md", Is.EqualTo(resolvedLink.Uri.LocalPath));
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
                Assert.That(resolvedLink, Is.InstanceOf<InternalLink>());
                Assert.That(@$"{currentDir}\introduction.md", Is.EqualTo(resolvedLink.Uri.LocalPath));
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
                Assert.That(resolvedLink, Is.InstanceOf<InternalLink>());
                Assert.That(@$"{currentDir}\SubChapter\introduction.md", Is.EqualTo(resolvedLink.Uri.LocalPath));
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1\chapter.md")]
        public void RelativePath_To_RootDirectory_Is_InternalLink(string root, string file)
        {
            var resolvedLink = myResolver.ResolveLink(@"../introduction.md", file, root);

            Assert.Multiple(() =>
            {
                Assert.That(resolvedLink, Is.InstanceOf<InternalLink>());
                Assert.That(@$"{root}\introduction.md", Is.EqualTo(resolvedLink.Uri.LocalPath));
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual", @"C:\Project X\Usermanual\Chapter 1\chapter.md")]
        public void RelativePath_To_Outside_RootDirectory_Is_ExternalLink(string root, string file)
        {
            var resolvedLink = myResolver.ResolveLink(@"../../introduction.md", file, root);

            Assert.Multiple(() =>
            {
                Assert.That(resolvedLink, Is.InstanceOf<ExternalLink>());
                Assert.That(@$"C:\Project X\introduction.md", Is.EqualTo(resolvedLink.Uri.LocalPath));
            });
        }
    }
}