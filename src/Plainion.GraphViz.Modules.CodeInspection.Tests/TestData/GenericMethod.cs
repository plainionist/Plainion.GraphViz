using System;
using Plainion.GraphViz.Modules.CodeInspection.Packaging.Services;

namespace Plainion.GraphViz.Modules.CodeInspection.Tests
{
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
}
