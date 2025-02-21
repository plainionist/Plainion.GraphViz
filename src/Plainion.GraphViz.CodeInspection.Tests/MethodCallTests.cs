using System.Linq;
using NUnit.Framework;
using Plainion.GraphViz.CodeInspection;

namespace Plainion.GraphViz.Modules.CodeInspection.Tests
{
    [TestFixture]
    class MethodCallTests
    {
        [Test]
        public void Equals_SameValue_True()
        {
            var lhs = new MethodCall(new Method(typeof(bool),"ToString"), new Method(typeof(bool), "ToString"));
            var rhs = new MethodCall(new Method(typeof(bool), "ToString"), new Method(typeof(bool), "ToString"));
            Assert.That(lhs, Is.EqualTo(rhs));
        }

        [Test]
        public void Equals_DifferentMethods_False()
        {
            var lhs = new MethodCall(new Method(typeof(bool), "ToString"), new Method(typeof(bool), "ToString"));
            var rhs = new MethodCall(new Method(typeof(int), "GetHashCode"), new Method(typeof(int), "GetHashCode"));
            Assert.That(lhs, Is.Not.EqualTo(rhs));
        }

        [Test]
        public void Distinct_DuplicatesRemoved()
        {
            var lhs = new MethodCall(new Method(typeof(bool), "ToString"), new Method(typeof(bool), "ToString"));
            var rhs = new MethodCall(new Method(typeof(bool), "ToString"), new Method(typeof(bool), "ToString"));

            var set = new[] { lhs, rhs }.Distinct().ToList();

            Assert.That(set.Count, Is.EqualTo(1));
        }
    }
}
