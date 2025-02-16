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

        public static DotSettings ScalableForcceDirectedPlancement => new()
        {
            GraphAttributes = {
                { "overlap", "prism" },
                { "start", "rand" },
                { "splines", "true" },
                { "edgeweight", "2" },
                { "K", "0.5" },
                { "compound", "true" },
                { "sep", "0.1" },
                { "nodesep", "0.2" },
                { "normalize", "true" }
            }
        };

        public static DotSettings ForceDirectedPlacement => new()
        {
            GraphAttributes = {
                { "overlap", "prism" },
                { "start", "rand" },
                { "splines", "true" },
                { "packmode", "graph" },
                { "nodesep", "0.3" },
                { "sep", "0.2" },
                { "K", "0.7" },
                { "normalize", "true" },
                { "edgeweight", "2" }
            }
        };

        public static DotSettings NeatSpring => new()
        {
            GraphAttributes = {
                { "mode", "major" },
                { "compound", "true" },
                { "overlap", "prism" },
                { "nodesep", "0.5" },
                { "sep", "0.5" },
                { "start", "rand" },
                { "normalize", "true" },
                { "edgeweight", "2" }
            }
        };

        public static DotSettings FromAlgorithm(LayoutAlgorithm algo) =>
            algo switch
            {
                LayoutAlgorithm.Flow => Flow,
                LayoutAlgorithm.ScalableForcceDirectedPlancement => ScalableForcceDirectedPlancement,
                LayoutAlgorithm.ForceDirectedPlacement => ForceDirectedPlacement,
                LayoutAlgorithm.NeatSpring => NeatSpring,
                _ => Default
            };
    }
}
