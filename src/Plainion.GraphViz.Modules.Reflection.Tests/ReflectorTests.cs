using NUnit.Framework;
using Plainion.GraphViz.Modules.Reflection.Inheritance;
using Plainion.GraphViz.Modules.Reflection.Packaging.Services;

namespace Plainion.GraphViz.Modules.Reflection.Tests
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
