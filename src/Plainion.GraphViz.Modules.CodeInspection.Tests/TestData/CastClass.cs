
using Plainion.GraphViz.Algorithms;

namespace Plainion.GraphViz.Modules.CodeInspection.Tests.TestData
{
    class CastClass
    {
        public void Init(object arg)
        {
            var ignore = ( ShowCycles )arg;
            var ignore2 =arg as UnfoldAndHide;
        }
    }
}
