using NUnit.Framework;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Services;

namespace Plainion.GraphViz.Modules.CodeInspection.Tests
{
    [TestFixture]
    class ReflectorTests
    {
        [Test]
        public void GetUsedTypes_typeof_found()
        {
            var reflector = new Reflector(new AssemblyLoader(), typeof(Fake));
            
            var types = reflector.GetUsedTypes();

            Assert.That(types, Contains.Item(typeof(AllTypesInspector)));
        }
    }
}
