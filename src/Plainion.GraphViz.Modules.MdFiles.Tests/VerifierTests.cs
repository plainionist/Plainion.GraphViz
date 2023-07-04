using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using NUnit.Framework;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Verifier;

namespace Plainion.GraphViz.Modules.MdFiles.Tests
{
    [TestFixture]
    internal class VerifierTests
    {
        private MockFileSystem myFileSystem;
        private ILinkVerifier myLinkVerifier;

        [SetUp]
        public void SetUp()
        {
            myFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            myLinkVerifier = new LinkVerifier(myFileSystem);
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual\introduction.md")]
        [TestCase(@"C:\Project X\Usermanual\introduction")]
        public void Link_Is_Valid(string url)
        {
            myFileSystem.AddFile(@"C:\Project X\Usermanual\introduction.md", new MockFileData(""));

            var verifiedLink = myLinkVerifier.VerifyInternalLink(url);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<ValidLink>(verifiedLink);
                Assert.AreEqual(verifiedLink.Url, @"C:\Project X\Usermanual\introduction.md");
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual\introduction.md")]
        [TestCase(@"C:\Project X\Usermanual\introduction")]
        public void Link_Is_Invalid(string url)
        {
            myFileSystem.AddFile(@"C:\Project X\Usermanual\chapter 1.md", new MockFileData(""));

            var verifiedLink = myLinkVerifier.VerifyInternalLink(url);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<InvalidLink>(verifiedLink);
                Assert.IsInstanceOf<FileNotFoundException>((verifiedLink as InvalidLink).Exception);
            });
        }
    }
}