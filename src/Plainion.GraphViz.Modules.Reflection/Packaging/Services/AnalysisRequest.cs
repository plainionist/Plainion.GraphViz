namespace Plainion.GraphViz.Modules.Reflection.Analysis.Packaging.Services
{
    class AnalysisRequest
    {
        public string Spec { get; set; }

        public string PackageName { get; set; }

        public string OutputFile { get; set; }

        public AnalysisMode AnalysisMode { get; set; }
    }
}
