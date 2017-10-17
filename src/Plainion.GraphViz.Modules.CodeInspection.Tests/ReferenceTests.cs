using System;
using System.Linq;
using NUnit.Framework;

using Plainion.GraphViz.Algorithms;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Modules.CodeInspection.Core;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Services;
using Plainion.GraphViz.Modules.CodeInspection.Tests.TestData;


namespace Plainion.GraphViz.Modules.CodeInspection.Tests
{
    [TestFixture]
    class ReferenceTests
    {
        [Test]
        public void Equals_SameValue_True()
        {
            var lhs = new Reference(typeof(bool), typeof(bool), ReferenceType.Undefined);
            var rhs = new Reference(typeof(bool), typeof(bool), ReferenceType.Undefined);
            Assert.That(lhs, Is.EqualTo(rhs));
        }

        [Test]
        public void Equals_DifferentTypes_False()
        {
            var lhs = new Reference(typeof(bool), typeof(double), ReferenceType.Undefined);
            var rhs = new Reference(typeof(bool), typeof(int), ReferenceType.Undefined);
            Assert.That(lhs, Is.Not.EqualTo(rhs));
        }

        [Test]
        public void Equals_DifferentReferenceType_False()
        {
            var lhs = new Reference(typeof(bool), typeof(bool), ReferenceType.Undefined);
            var rhs = new Reference(typeof(bool), typeof(bool), ReferenceType.Calls);
            Assert.That(lhs, Is.Not.EqualTo(rhs));
        }

        [Test]
        public void Distinct_DuplicatesRemoved()
        {
            var lhs = new Reference(typeof(bool), typeof(bool), ReferenceType.Undefined);
            var rhs = new Reference(typeof(bool), typeof(bool), ReferenceType.Undefined);

            var set = new[] { lhs, rhs }.Distinct().ToList();

            Assert.That(set.Count, Is.EqualTo(1));
        }
    }
}
