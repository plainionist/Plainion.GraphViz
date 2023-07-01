using System.IO.Abstractions;
using System.Linq;
using NUnit.Framework;

namespace Plainion.GraphViz.Modules.MdFiles.Tests
{
    [TestFixture]
    internal class ParserTests
    {
        [Test]
        public void Parse_MarkDownDocument()
        {
            var parser = new Dependencies.Parser.MarkdigParser(new FileSystem());

            var md = parser.LoadMarkdown("[Introduction](UserManual/Introduction.md)");

            Assert.IsTrue(md.Links.Any());
        }
    }
}