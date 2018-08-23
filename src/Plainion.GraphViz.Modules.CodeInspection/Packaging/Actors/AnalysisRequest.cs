namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Actors
{
    class AnalysisRequest
    {
        public string Spec { get; set; }

        public string[] PackagesToAnalyze { get; set; }

        public string OutputFile { get; set; }

        public bool UsedTypesOnly { get; set; }

        public bool CreateClustersForNamespaces { get; set; }

        public bool AllEdges { get; set; }
    }
}
