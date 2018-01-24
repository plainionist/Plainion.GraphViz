using System;
using System.Linq;
using NUnit.Framework;
using Plainion.GraphViz.Modules.CodeInspection.Core;

namespace Plainion.GraphViz.Modules.CodeInspection.Tests
{
    [TestFixture]
    class Inspect_CalledMethods_Tests
    {
        [Test]
        public void CallOfInstanceMethod()
        {
            Verify(typeof(Caller), nameof(Caller.Main), typeof(Callee), nameof(Callee.InstanceMethod));
        }

        [Test]
        public void CallOfStaticMethod()
        {
            Verify(typeof(Caller), nameof(Caller.Main), typeof(Callee), nameof(Callee.StaticMethod));
        }

        [Test]
        public void CallOfBaseClassMethod()
        {
            Verify(typeof(Caller), nameof(Caller.Main), typeof(CalleeBase), nameof(Callee.BaseMethod));
        }

        [Test]
        public void CallOfInterfaceMethod()
        {
            Verify(typeof(Caller), nameof(Caller.Action), typeof(ICallee), nameof(ICallee.InterfaceMethod));
        }

        [Test]
        public void CallOfVirtualMethod()
        {
            Verify(typeof(Caller), nameof(Caller.Main), typeof(Callee), nameof(Callee.VirtualMethod));
        }

        [Test]
        public void CallOfExtensionMethod()
        {
            Verify(typeof(Caller), nameof(Caller.Main), typeof(CalleeExtensions), nameof(CalleeExtensions.ExtensionMethod));
        }

        private void Verify(Type callerType, string callerMethod, Type callingType, string callingMethod)
        {
            var reflector = new Inspector(new MonoLoader(), callerType);

            var calls = reflector.GetCalledMethods();

            var types = calls
                .Where(call => call.From.Name == callerMethod)
                .Select(call => call.To)
                .Distinct();

            var callee = new Method(callingType, callingMethod);
            Assert.That(types, Contains.Item(callee));
        }
    }

    class Caller
    {
        public void Main()
        {
            new Callee().InstanceMethod();
            Callee.StaticMethod();
            new Callee().BaseMethod();
            new Callee().VirtualMethod();
            new Callee().ExtensionMethod();
        }

        public void Action(ICallee callee)
        {
            callee.InterfaceMethod();
        }
    }

    interface ICallee
    {
        void InterfaceMethod();
    }

    class CalleeBase
    {
        public void BaseMethod()
        {
        }
    }

    class Callee : CalleeBase, ICallee
    {
        public void InstanceMethod()
        {
        }

        public static void StaticMethod()
        {
        }

        public void InterfaceMethod()
        {
        }

        public virtual void VirtualMethod()
        {
        }
    }

    static class CalleeExtensions
    {
        public static void ExtensionMethod(this Callee self)
        {
        }
    }
}
