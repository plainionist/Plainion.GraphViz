using System;
using System.Collections.Generic;
using Plainion.GraphViz.Modules.CodeInspection.Inheritance;

namespace Plainion.GraphViz.Modules.CodeInspection.Tests.TestData
{
    class TypeOf
    {
        public List<Type> Types = new List<Type>();

        public void Init()
        {
            Types.Add(typeof(AllTypesInspector));
        }
    }
}
