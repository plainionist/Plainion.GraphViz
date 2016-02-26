using System;
using System.Collections.Generic;
using Plainion.GraphViz.Modules.Reflection.Analysis;

namespace Plainion.GraphViz.Modules.Reflection.Tests
{
    class Fake
    {
        public List<Type> Types = new List<Type>();

        public void Init()
        {
            Types.Add(typeof(AllTypesInspector));
        }
    }
}
