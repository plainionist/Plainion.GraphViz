using System.Linq;
using NUnit.Framework;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Services;

namespace Plainion.GraphViz.Modules.CodeInspection.Tests
{
    [TestFixture]
    class ReflectorTests
    {
        [Test]
        public void GetUsedTypes_typeof_Found()
        {
            var reflector = new Reflector(new AssemblyLoader(), typeof(TypeOf));

            var edges = reflector.GetUsedTypes();

            var types = edges
                .SelectMany(e => new[] { e.Source, e.Target })
                .Distinct();

            Assert.That(types, Contains.Item(typeof(AllTypesInspector)));
        }

        [Test]
        public void GetUsedTypes_GenericMethod_Found()
        {
            var reflector = new Reflector(new AssemblyLoader(), typeof(GenericMethod));

            var edges = reflector.GetUsedTypes();

            var types = edges
                .SelectMany(e => new[] { e.Source, e.Target })
                .Distinct();

            Assert.That(types, Contains.Item(typeof(AnalysisDocument)));
        }
    }
}
