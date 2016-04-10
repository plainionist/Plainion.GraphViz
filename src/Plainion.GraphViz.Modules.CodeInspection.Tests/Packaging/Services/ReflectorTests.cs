using System;
using System.Linq;
using NUnit.Framework;

using Plainion.GraphViz.Algorithms;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Services;
using Plainion.GraphViz.Modules.CodeInspection.Tests.TestData;


namespace Plainion.GraphViz.Modules.CodeInspection.Tests.Packaging.Services
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
            Verify(code, usedType, EdgeType.Undefined);
        }

        private void Verify(Type code, Type usedType, EdgeType edgeType)
        {
            var reflector = new Reflector(new AssemblyLoader(), code);

            var edges = reflector.GetUsedTypes();

            var types = edges
                .Where(e => edgeType == EdgeType.Undefined || e.EdgeType == edgeType)
                .Select(e => e.Target)
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

        [Test]
        public void GetUsedTypes_Constructor_Found()
        {
            Verify( typeof( Constructor ), typeof( ShowCycles ) );
        }

        [Test]
        public void GetUsedTypes_NewArray_Found()
        {
            Verify( typeof( NewArray ), typeof( ShowCycles ) );
        }

        [Test]
        public void GetUsedTypes_HardCast_Found()
        {
            Verify( typeof( CastClass ), typeof( ShowCycles ) );
        }

        [Test]
        public void GetUsedTypes_AsCast_Found()
        {
            Verify( typeof( CastClass ), typeof( UnfoldAndHidePrivateNodes ) );
        }

        [Test]
        public void GetUsedTypes_VirtualMethodCall_Found()
        {
            Verify(typeof(VirtualMethodCall), typeof(ShowCycles));
        }

        [Test]
        public void GetUsedTypes_InterfaceMethod_Found()
        {
            Verify(typeof(InterfaceMethod), typeof(ILayoutEngine));
        }
    }
}
