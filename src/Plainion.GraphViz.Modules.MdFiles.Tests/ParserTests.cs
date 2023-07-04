using System.Collections.Generic;
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
        private IMarkdownParser myParser;
        private MockFileSystem myFileSystem;

        [SetUp]
        public void SetUp()
        {
            myFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());
            myParser = new MarkdigParser(myFileSystem);
        }

        [Test]
        public void Parse_SingleLink_MarkDownDocument()
        {
            var md = myParser.LoadMarkdown("....[Introduction](UserManual/Introduction.md)....");

            Assert.IsNotEmpty(md.Links);
        }

        [Test]
        public void Parse_SingleLink_With_WhiteSpaces_MarkDownDocument()
        {
            var md = myParser.LoadMarkdown("....[Introduction](<User Manual/Introduction.md>)....");

            Assert.IsNotEmpty(md.Links);
        }

        [Test]
        public void Parse_SingleLink_MarkDownFile()
        {
            myFileSystem.AddFile(@"C:\manual.md", new MockFileData("...[Introduction](UserManual/Introduction.md)..."));

            var md = myParser.LoadFile(@"C:\manual.md");

            Assert.IsNotEmpty(md.Links);
        }

        [Test]
        public void Parse_SingleImageLink_MarkDownDocument()
        {
            var md = myParser.LoadMarkdown("...![This is a sample image](images/sample.png)...");

            Assert.IsNotEmpty(md.Links.OfType<ImageLink>());
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

            var md = myParser.LoadMarkdown(sb.ToString());

            Assert.Multiple(() =>
            {
                Assert.AreEqual(md.Links.Count, 3);
                Assert.AreEqual(md.Links.OfType<DocLink>().Count(), 2);
                Assert.AreEqual(md.Links.OfType<ImageLink>().Count(), 1);
            });
        }

        [Test]
        public void Parse_EmptyLink_MarkDownDocument()
        {
            var md = myParser.LoadMarkdown("[Missing Link]()");

            Assert.IsEmpty(md.Links);
        }
    }
}