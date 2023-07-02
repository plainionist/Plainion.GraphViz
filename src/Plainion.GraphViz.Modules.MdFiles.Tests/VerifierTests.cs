using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using NUnit.Framework;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Verifier;

namespace Plainion.GraphViz.Modules.MdFiles.Tests
{
    [TestFixture]
    internal class VerifierTests
    {
        [Test]
        [TestCase(@"C:\Project X\Usermanual\introduction.md")]
        [TestCase(@"C:\Project X\Usermanual\introduction")]
        public void Link_Is_Valid(string url)
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"C:\Project X\Usermanual\introduction.md", new MockFileData("") },
            });

            var verifier = new LinkVerifier(fileSystem);
            var verifiedLink = verifier.VerifyInternalLink(url);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(verifiedLink is ValidLink);
                Assert.IsTrue(verifiedLink.Url.Equals(@"C:\Project X\Usermanual\introduction.md"));
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual\introduction.md")]
        [TestCase(@"C:\Project X\Usermanual\introduction")]
        public void Link_Is_Invalid(string url)
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"C:\Project X\Usermanual\chapter 1.md", new MockFileData("") },
            });

            var verifier = new LinkVerifier(fileSystem);
            var verifiedLink = verifier.VerifyInternalLink(url);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(verifiedLink is InvalidLink);
                Assert.IsTrue((verifiedLink as InvalidLink).Exception is FileNotFoundException);
            });
        }
    }
}