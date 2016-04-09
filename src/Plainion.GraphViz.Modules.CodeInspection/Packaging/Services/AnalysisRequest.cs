namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Services
{
    class AnalysisRequest
    {
        public string Spec { get; set; }

        public string[] PackagesToAnalyze { get; set; }

        public string OutputFile { get; set; }
    }
}
