using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using GPC.Model.Elements;

namespace GPC.Model.PostProcessing
{

    [Serializable]
    public sealed class AnalysisSource
    {
        public string Program { get; set; }
        public string SolverVersion { get; set; }
        public string ModelRevision { get; set; }
        public string AnalysisId { get; set; }
        public string GeometryHash { get; set; }
    }
}
