namespace Plainion.GraphViz.Dot
{
    public class DotSettings
    {
        public string RankDir { get; set; }

        public string Ratio { get; set; }

        public string RankSep { get; set; }
    }

    public class DotPresets
    {
        public static DotSettings Default
        {
            get
            {
                return new DotSettings
                {
                    RankDir = "BT",
                    Ratio = "conpress",
                    RankSep = "2.0 equally"
                };
            }
        }

        public static DotSettings Flow
        {
            get
            {
                return new DotSettings
                {
                    RankDir = "LR",
                    Ratio = null,
                    RankSep = null
                };
            }
        }
    }
}
