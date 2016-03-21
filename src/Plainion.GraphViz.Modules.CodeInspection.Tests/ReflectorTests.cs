using System;
using System.Linq;
using NUnit.Framework;

using Plainion.GraphViz.Model;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Services;
using Plainion.GraphViz.Modules.CodeInspection.Tests.TestData;


namespace Plainion.GraphViz.Modules.CodeInspection.Tests
{
    [TestFixture]
    class ReflectorTests
    {
        [Test]
        public void GetUsedTypes_typeof_Found()
        {
            Verify(typeof(TypeOf), typeof(AllTypesInspector));
        }

        private void Verify(Type code, Type usedType)
        {
            var reflector = new Reflector(new AssemblyLoader(), code);

            var edges = reflector.GetUsedTypes();

            var types = edges
                .SelectMany(e => new[] { e.Source, e.Target })
                .Distinct();

            Assert.That(types, Contains.Item(usedType));
        }

        [Test]
        public void GetUsedTypes_GenericMethod_Found()
        {
            Verify(typeof(GenericMethod), typeof(AnalysisDocument));
        }

        [Test]
        public void GetUsedTypes_LinqAnonymousType_Found()
        {
            Verify(typeof(LinqAnonymousType), typeof(Node));
        }
    }
}
