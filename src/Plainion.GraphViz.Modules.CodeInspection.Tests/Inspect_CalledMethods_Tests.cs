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

        [Test(Description = "We always get the 'declaring' type - not the one the method is finally called on")]
        public void CallOfVirtualBaseClassMethod()
        {
            Verify(typeof(Caller), nameof(Caller.Action), typeof(CalleeBase), nameof(Callee.VirtualBaseMethod));
        }

        [Test(Description = "We always get the 'declaring' type - not the one the method is finally called on")]
        public void CallOfBaseInterfaceMethod()
        {
            Verify(typeof(Caller), nameof(Caller.Action), typeof(ICalleeBase), nameof(ICalleeBase.BaseInterfaceMethod));
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

        [Test]
        public void CallOfProperty()
        {
            Verify(typeof(Caller), nameof(Caller.SpecialMethods), typeof(Callee), "get_Property");
            Verify(typeof(Caller), nameof(Caller.SpecialMethods), typeof(Callee), "set_Property");
        }

        [Test]
        public void CallOfIndexer()
        {
            Verify(typeof(Caller), nameof(Caller.SpecialMethods), typeof(Callee), "get_Item");
            Verify(typeof(Caller), nameof(Caller.SpecialMethods), typeof(Callee), "set_Item");
        }

        [Test]
        public void CallOfEvent()
        {
            Verify(typeof(Caller), nameof(Caller.Main), typeof(CalleeExtensions), nameof(CalleeExtensions.ExtensionMethod));
        }

        [Test]
        public void FromInnerClass()
        {
            Verify(typeof(Caller.Inner), nameof(Caller.Inner.Exec), typeof(Callee), nameof(Callee.InstanceMethod));
        }

        [Test]
        public void FromPrivatMethod()
        {
            var reflector = new Inspector(new MonoLoader(), typeof(Caller));

            var calls = reflector.GetCalledMethods();

            var expected = new[] {
                new MethodCall(new Method(typeof(Caller), nameof(Caller.Facade)),new Method(typeof(Caller), "DoItSilently")),
                new MethodCall(new Method(typeof(Caller), "DoItSilently"),new Method( typeof(Callee), nameof(Callee.StaticMethod)))
            };

            Assert.That(calls.Intersect(expected), Is.EquivalentTo(expected));
        }

        [Test]
        public void FromConstructor()
        {
            Verify(typeof(Caller), ".ctor", typeof(Callee), nameof(Callee.InstanceMethod));
        }

        [Test]
        public void FromProperty()
        {
            Verify(typeof(Caller), "set_ABC", typeof(Callee), nameof(Callee.InstanceMethod));
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
        public class Inner
        {
            public void Exec()
            {
                new Callee().InstanceMethod();
            }
        }

        public Caller()
        {
            new Callee().InstanceMethod();
        }

        public string ABC
        {
            set { new Callee().InstanceMethod(); }
        }

        public static void Main()
        {
            new Callee().InstanceMethod();
            Callee.StaticMethod();
            new Callee().BaseMethod();
            new Callee().ExtensionMethod();
            var c4 = new Callee();
            c4.VirtualMethod();
        }

        public void Action(ICallee callee)
        {
            callee.InterfaceMethod();

            callee.BaseInterfaceMethod();

            var c4 = new Callee();
            c4.VirtualBaseMethod();
        }

        public void SpecialMethods()
        {
            var callee = new Callee();
            if (callee.Property == 42)
            {
                callee.Property = 42 * 2;
            }

            callee["a"] = callee["b"];
            callee.Event += (s, e) => { };
        }

        public void Facade()
        {
            DoItSilently();
        }

        private void DoItSilently()
        {
            Callee.StaticMethod();
        }
    }

    interface ICalleeBase
    {
        void BaseInterfaceMethod();
    }

    interface ICallee : ICalleeBase
    {
        void InterfaceMethod();
    }

    class CalleeBase
    {
        public void BaseMethod()
        {
        }

        public virtual void VirtualBaseMethod()
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

        public void BaseInterfaceMethod()
        {
        }

        public void InterfaceMethod()
        {
        }

        public virtual void VirtualMethod()
        {
        }

        public override void VirtualBaseMethod()
        {
        }

        public int Property { get; set; }

        public string this[string key]
        {
            get { return null; }
            set { Event(this, EventArgs.Empty); }
        }

        public event EventHandler Event;
    }

    static class CalleeExtensions
    {
        public static void ExtensionMethod(this Callee self)
        {
        }
    }
}
