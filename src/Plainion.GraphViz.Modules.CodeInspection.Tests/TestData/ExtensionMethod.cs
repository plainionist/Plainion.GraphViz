
using System;

namespace Plainion.GraphViz.Modules.CodeInspection.Tests.TestData
{
    static class ExtensionMethod
    {
        public static void Times(this int number, Action action)
        {
        }
    }

    class ExtensionMethodUser
    {
        public void Run()
        {
            5.Times(() => { });
        }
    }
}
