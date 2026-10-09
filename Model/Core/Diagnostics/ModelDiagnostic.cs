using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using GPC.Model.Elements;
using GPC.Model.Core.Identity;

namespace GPC.Model.Core.Diagnostics
{
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class ModelDiagnostic
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Code>k__BackingField", IsRequired = true)]
        public string Code { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Severity>k__BackingField", IsRequired = true)]
        public DiagnosticSeverity Severity { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<ElementId>k__BackingField", IsRequired = true)]
        public int? ElementId { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Family>k__BackingField", IsRequired = true)]
        public EntityFamily? Family { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Station>k__BackingField", IsRequired = true)]
        public double? Station { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Dataset>k__BackingField", IsRequired = true)]
        public string Dataset { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Case>k__BackingField", IsRequired = true)]
        public string Case { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Record>k__BackingField", IsRequired = true)]
        public string Record { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Message>k__BackingField", IsRequired = true)]
        public string Message { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<SuggestedAction>k__BackingField", IsRequired = true)]
        public string SuggestedAction { get; set; }

        public static ModelDiagnostic Error(string code, Element element = null, string message = null) => new ModelDiagnostic
        {
            Code = code,
            Severity = DiagnosticSeverity.Error,
            ElementId = element?.Id,
            Family = element is NodeElement ? EntityFamily.Node : element is BeamElement ? EntityFamily.Beam : element is AreaElement ? EntityFamily.Shell : element is VolumeElement ? EntityFamily.Solid : (EntityFamily? )null,
            Message = message ?? code,
            SuggestedAction = "Supply or correct the source assignment explicitly."
        };
    }
}
