﻿using Plainion.GraphViz.Actors.Client;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging.Actors
{
    class AnalysisRequest
    {
        public string Spec { get;  set; }

        public string[] PackagesToAnalyze { get; set; }
    }

    class AnalysisMessage : AnalysisRequest
    {
        public string OutputFile { get; set; }
    }

    class AnalysisResponse : FinishedMessage
    {
        public string File { get; set; }
    }
}
