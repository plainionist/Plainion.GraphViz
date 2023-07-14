using System;
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
        [TestCase(@"C:\Project X\Usermanual\introduction.md#note")]
        [TestCase(@"C:\Project X\Usermanual\introduction#note")]
        public void Link_Is_Valid(string link)
        {
            myFileSystem.AddFile(@"C:\Project X\Usermanual\introduction.md", new MockFileData(""));

            var verifiedLink = myLinkVerifier.VerifyLink(link);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<ValidLink>(verifiedLink);
                Assert.AreEqual(@"C:\Project X\Usermanual\introduction.md", verifiedLink.Path);
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual\introduction.md")]
        [TestCase(@"C:\Project X\Usermanual\introduction")]
        public void Link_Is_Invalid(string link)
        {
            myFileSystem.AddFile(@"C:\Project X\Usermanual\chapter 1.md", new MockFileData(""));

            var verifiedLink = myLinkVerifier.VerifyLink(link);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<InvalidLink>(verifiedLink);
                Assert.IsInstanceOf<FileNotFoundException>((verifiedLink as InvalidLink).Exception);
                Assert.AreEqual(link, verifiedLink.Path);
            });
        }

        [Test]
        [TestCase(@"C:\Project X\Usermanual\introdu#ction.md#note")]
        [TestCase(@"C:\Project X\Usermanual\introdu#ction#note")]
        public void Link_With_Invalid_Anchor_Is_Invalid(string link)
        {
            myFileSystem.AddFile(@"C:\Project X\Usermanual\introduction.md", new MockFileData(""));

            var verifiedLink = myLinkVerifier.VerifyLink(link);

            Assert.Multiple(() =>
            {
                Assert.IsInstanceOf<InvalidLink>(verifiedLink);
                Assert.IsInstanceOf<ArgumentOutOfRangeException>((verifiedLink as InvalidLink).Exception);
                Assert.AreEqual(link, verifiedLink.Path);
            });
        }
    }
}