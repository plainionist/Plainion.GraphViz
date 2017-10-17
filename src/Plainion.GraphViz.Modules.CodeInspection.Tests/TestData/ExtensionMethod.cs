
using System;

namespace Plainion.GraphViz.Modules.CodeInspection.Tests.TestData
{
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
}
