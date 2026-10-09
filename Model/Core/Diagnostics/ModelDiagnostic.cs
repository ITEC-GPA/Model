using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using GPC.Model.Elements;

namespace GPC.Model.PostProcessing
{

    [Serializable]
    public sealed class ModelDiagnostic
    {
        public string Code { get; set; }
        public DiagnosticSeverity Severity { get; set; }
        public int? ElementId { get; set; }
        public EntityFamily? Family { get; set; }
        public double? Station { get; set; }
        public string Dataset { get; set; }
        public string Case { get; set; }
        public string Record { get; set; }
        public string Message { get; set; }
        public string SuggestedAction { get; set; }
        public static ModelDiagnostic Error(string code, Element element = null, string message = null) => new ModelDiagnostic
        {
            Code = code,
            Severity = DiagnosticSeverity.Error,
            ElementId = element?.Id,
            Family = element is NodeElement ? EntityFamily.Node : element is BeamElement ? EntityFamily.Beam : element is AreaElement ? EntityFamily.Shell : element is VolumeElement ? EntityFamily.Solid : (EntityFamily?)null,
            Message = message ?? code,
            SuggestedAction = "Supply or correct the source assignment explicitly."
        };
    }
}
