using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Markdown;
using Plainion.GraphViz.Modules.MdFiles.Dependencies.Analyzer.Parser;

namespace Plainion.GraphViz.Modules.MdFiles.Tests
{
    [TestFixture]
    internal class ParserTests
    {
        [Test]
        public void Parse_SingleLink_MarkDownDocument()
        {
            var parser = new MarkdigParser(new FileSystem());

            var md = parser.LoadMarkdown("....[Introduction](UserManual/Introduction.md)....");

            Assert.IsTrue(md.Links.Any());
        }

        [Test]
        public void Parse_SingleLink_With_WhiteSpaces_MarkDownDocument()
        {
            var parser = new MarkdigParser(new FileSystem());

            var md = parser.LoadMarkdown("....[Introduction](<User Manual/Introduction.md>)....");

            Assert.IsTrue(md.Links.Any());
        }

        [Test]
        public void Parse_SingleLink_MarkDownFile()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { @"C:\manual.md", new MockFileData("...[Introduction](UserManual/Introduction.md)...") },
            });

            var parser = new MarkdigParser(fileSystem);

            var md = parser.LoadFile(@"C:\manual.md");

            Assert.IsTrue(md.Links.Any());
        }

        [Test]
        public void Parse_SingleImageLink_MarkDownDocument()
        {
            var parser = new MarkdigParser(new FileSystem());

            var md = parser.LoadMarkdown("...![This is a sample image](images/sample.png)...");

            Assert.IsTrue(md.Links.OfType<ImageLink>().Any());
        }

        [Test]
        public void Parse_MultipleLinks_MarkDownDocument()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Here comes a link");
            sb.AppendLine("[Introduction](UserManual/Introduction.md)");
            sb.AppendLine("There is another link");
            sb.AppendLine("[Chapter 1](UserManual/Chapter1.md)");
            sb.AppendLine("Here is a link to an image");
            sb.AppendLine("![Chart](Appendix/chart.jpg)");
            sb.AppendLine("Finish!");

            var parser = new MarkdigParser(new FileSystem());

            var md = parser.LoadMarkdown(sb.ToString());

            Assert.Multiple(() =>
            {
                Assert.IsTrue(md.Links.Count == 3);
                Assert.IsTrue(md.Links.OfType<DocLink>().Count() == 2);
                Assert.IsTrue(md.Links.OfType<ImageLink>().Count() == 1);
            });
        }

        [Test]
        public void Parse_EmptyLink_MarkDownDocument()
        {
            var parser = new MarkdigParser(new FileSystem());

            var md = parser.LoadMarkdown("[Missing Link]()");

            Assert.IsTrue(!md.Links.Any());
        }
    }
}