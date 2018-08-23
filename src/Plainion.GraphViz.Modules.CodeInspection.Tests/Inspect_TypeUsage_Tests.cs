using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Plainion.GraphViz.Algorithms;
using Plainion.GraphViz.Model;
using Plainion.GraphViz.Modules.CodeInspection.Core;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance.Services;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Analyzers;

namespace Plainion.GraphViz.Modules.CodeInspection.Tests
{
    [TestFixture]
    class Inspect_TypeUsage_Tests
    {
        [Test]
        public void GetUsedTypes_typeof_Found()
        {
            Verify(typeof(TypeOf), typeof(AllTypesActor));
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
            Verify( typeof( CastClass ), typeof( UnfoldAndHide ) );
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

        [Test]
        public void GetUsedTypes_ExtensionMethod_Found()
        {
            Verify(typeof(ExtensionMethodUser), typeof(ExtensionMethod));
        }

        private void Verify(Type code, Type usedType)
        {
            Verify(code, usedType, ReferenceType.Undefined);
        }

        private void Verify(Type code, Type usedType, ReferenceType edgeType)
        {
            var reflector = new Inspector(new MonoLoader(), code);

            var edges = reflector.GetUsedTypes();

            var types = edges
                .Where(e => edgeType == ReferenceType.Undefined || e.ReferenceType == edgeType)
                .Select(e => e.To)
                .Distinct();

            Assert.That(types, Contains.Item(usedType));
        }
    }

    class CastClass
    {
        public void Init(object arg)
        {
            var ignore = (ShowCycles)arg;
            var ignore2 = arg as UnfoldAndHide;
        }
    }

    class Constructor
    {
        public void Init()
        {
            var ignore = new ShowCycles(null);
        }
    }

    static class ExtensionMethod
    {
        public static void Times(this ExtendedObject obj, Action action)
        {
        }
    }

    class ExtendedObject
    {
        public void Times(Action action, int count)
        {
        }

    }

    class ExtensionMethodUser
    {
        public void Run()
        {
            var obj = new ExtendedObject();
            obj.Times(() => { });
        }
    }

    class GenericMethod
    {
        public void Init()
        {
            Dump<AnalysisDocument>();
        }

        private void Dump<T>()
        {
            Console.WriteLine(typeof(T));
        }
    }

    class InterfaceMethod
    {
        public void Init(ILayoutEngine obj)
        {
            obj.Relayout(null);
        }
    }

    class LinqAnonymousType
    {
        public IEnumerable<IGraphItem> Find()
        {
            var items = GetActiveViews<IGraphItem>();

            return items
                .Select(item => new
                {
                    Value = item,
                    Node = item as Node
                })
                .Where(x => x.Node != null)
                .Select(x => x.Value)
                .ToList();
        }

        private IEnumerable<T> GetActiveViews<T>() where T : class, IGraphItem
        {
            return new T[] {
                new Node("1") as T,
                new Edge(new Node("2"), new Node("3")) as T};
        }
    }

    class NewArray
    {
        public void Init()
        {
            var ignore = new ShowCycles[8];
        }
    }

    class TypeOf
    {
        public List<Type> Types = new List<Type>();

        public void Init()
        {
            Types.Add(typeof(AllTypesActor));
        }
    }

    class VirtualMethodCall
    {
        public void Init(ShowCycles algo)
        {
            algo.Execute();
        }
    }
}
