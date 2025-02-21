using System.Linq;
using NUnit.Framework;
using Plainion.GraphViz.CodeInspection;

namespace Plainion.GraphViz.Modules.CodeInspection.Tests
{
    [TestFixture]
    class MethodTests
    {
        [Test]
        public void Equals_SameValue_True()
        {
            var lhs = new Method(typeof(bool), "ToString");
            var rhs = new Method(typeof(bool), "ToString");
            Assert.That(lhs, Is.EqualTo(rhs));
        }

        [Test]
        public void Equals_DifferentTypes_False()
        {
            var lhs = new Method(typeof(bool), "ToString");
            var rhs = new Method(typeof(int), "ToString");
            Assert.That(lhs, Is.Not.EqualTo(rhs));
        }

        [Test]
        public void Equals_DifferentMethodName_False()
        {
            var lhs = new Method(typeof(bool), "ToString");
            var rhs = new Method(typeof(bool), "GetHashCode");
            Assert.That(lhs, Is.Not.EqualTo(rhs));
        }

        [Test]
        public void Distinct_DuplicatesRemoved()
        {
            var lhs = new Method(typeof(bool), "ToString");
            var rhs = new Method(typeof(bool), "ToString");

            var set = new[] { lhs, rhs }.Distinct().ToList();

            Assert.That(set.Count, Is.EqualTo(1));
        }
    }
}
