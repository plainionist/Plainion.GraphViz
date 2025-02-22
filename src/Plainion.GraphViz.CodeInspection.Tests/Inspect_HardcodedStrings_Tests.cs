using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Plainion.GraphViz.CodeInspection;

namespace Plainion.GraphViz.Modules.CodeInspection.Tests
{
    [TestFixture]
    class Inspect_HardcodedMethods_Tests
    {
        [Test]
        public void FromConstructor()
        {
            Verify(typeof(SomeClass), "ctor", "default");
        }

        [Test]
        public void FromStaticMethod()
        {
            Verify(typeof(SomeClass), nameof(SomeClass.MainDummy), "main");
        }

        [Test]
        public void FromInstanceMethod()
        {
            Verify(typeof(SomeClass), nameof(SomeClass.DoIt), "do-it");
        }

        [Test]
        public void FromProperty()
        {
            Verify(typeof(SomeClass), nameof(SomeClass.ABC), "abc");
        }

        private void Verify(Type callerType, string callerMethod, string hardcodedString)
        {
            var reflector = new Inspector(new NullLoggerFactory().CreateLogger<Inspector>(), new MonoLoader(new[] { typeof(Caller).Assembly }), callerType);

            var strings = reflector.GetHardcodedStrings();

            Assert.That(strings, Contains.Item(hardcodedString));
        }
    }

    class SomeClass
    {
        public class Inner
        {
            public string Exec()
            {
                return "Exec";
            }
        }

        private static volatile string myValue;

        public SomeClass()
        {
            myValue = "default";
        }

        public static void MainDummy()
        {
            myValue = "main";
        }

        public void DoIt()
        {
            myValue = "do-it";
        }

        public string ABC
        {
            set { myValue = "abc"; }
            get { return myValue; }
        }
    }
}
