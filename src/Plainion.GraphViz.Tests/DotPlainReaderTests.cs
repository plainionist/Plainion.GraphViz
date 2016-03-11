using System;
using System.IO;
using NUnit.Framework;
using Plainion.GraphViz.Dot;

namespace Plainion.GraphViz.Tests
{
    [TestFixture]
    class DotPlainReaderTests
    {
        [Test]
        public void ReadLine_NormalEdgeLine_ReadTillEnd()
        {
            var line = "edge \"syngo.CT.BizLogic.Applications.DualEnergy.Eval.DEBasicCP.Core.Visualization.UIControl.DimUnsupportedCommandsOrder\" \"syngo.CT.BizLogic.Applications.DualEnergy.Eval.DEBasicCP.Core.Infrastructure.AbstractDualEnergyScopeBaseOrder\" 19 294.63 25.208 294.31 25.261 293.95 25.317 293.61 25.361 291.75 25.611 286.82 25.298 285.2 26.25 284.38 26.732 284.71 27.41 283.96 28 281.85 29.668 280.39 28.513 278.34 30.25 277.64 30.838 278.06 31.543 277.27 32 275.44 33.058 244.14 33.211 235.32 33.232 \".\" 281.74 29.125 solid black";

            using (var r = new StringReader(line))
            {
                var reader = new DotPlainReader(r);
                reader.ReadLine("edge");

                Assert.That(reader.CurrentLine, Does.EndWith("solid black"));

            }
        }

        [Test]
        public void ReadLine_EdgeLineWithBreak_ReadTillEnd()
        {
            var line =
                "edge \"syngo.CT.BizLogic.Applications.DualEnergy.Eval.DEBasicCP.Core.Visualization.SegmentationHandling.AbstractDisplayingSegmentationOrder_\\" +
                Environment.NewLine +
                "SAFETY\" \"syngo.CT.BizLogic.Applications.DualEnergy.Eval.DEBasicCP.Core.Infrastructure.AbstractDualEnergyScopeBaseOrder\" 13 363.38 30.68 363.02 30.706 362.65 30.731 362.31 30.75 347.96 31.531 344.35 31.187 330 31.75 327.66 31.842 327.08 31.929 324.74 32 289.12 33.087 245.68 33.218 235.32 33.234 \".\" 330.03 31.875 solid black";

            using (var r = new StringReader(line))
            {
                var reader = new DotPlainReader(r);
                reader.ReadLine("edge");

                Assert.That(reader.CurrentLine, Does.EndWith("solid black"));

            }
        }
    }
}
