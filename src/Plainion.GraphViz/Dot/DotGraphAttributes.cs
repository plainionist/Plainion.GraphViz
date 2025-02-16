using System.Collections.Generic;

namespace Plainion.GraphViz.Dot
{
    public class DotSettings
    {
        public Dictionary<string, string> GraphAttributes { get; } = [];

        public static DotSettings Default => new()
        {
            GraphAttributes = {
                { "RankDir", "BT" },
                { "Ratio", "compress" },
                { "RankSep", "2.0 equally" },
            }
        };

        public static DotSettings Flow => new()
        {
            GraphAttributes = {
                { "RankDir", "LR" },
            }
        };
    }
}
