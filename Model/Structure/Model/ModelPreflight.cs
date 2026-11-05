using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Core.Diagnostics;
using GPC.Model.ElementProperties;

namespace GPC.Model.Models
{
    /// <summary>Domain checks of the physical action and property records. Serialization, copies and fingerprints never run
    /// them, so an edit that leaves a dangling reference (a removed group, case or element still cited by a stage or a moving
    /// load) keeps the model copyable and saveable: <see cref="ModelEditSession.Validate"/> reports it and the exporters reject
    /// it through <see cref="RequireExportable"/>.</summary>
    public static class ModelPreflight
    {
        /// <summary>Diagnostics of stages, moving loads, form finding and elastic properties.</summary>
        public static IReadOnlyList<ModelDiagnostic> ValidateDomain(this Model model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            var result = new List<ModelDiagnostic>(global::GPC.Model.Stages.StageAnalysisState.Diagnose(model));
            Check(result, "InvalidMovingLoads", () => model.MovingLoads?.Validate(model));
            Check(result, "InvalidFormFinding", () => model.FormFinding?.Validate(model));
            if (model.FormFinding != null) result.AddRange(model.FormFinding.IgnoredInputs());
            foreach (var property in model.BeamProperties.Values.OfType<ElasticBeamProperty>())
                Check(result, "InvalidElasticProperty", property.Validate, property.Name);
            foreach (var property in model.PlateProperties.Values)
                if (property is ElasticPlateProperty elastic) Check(result, "InvalidElasticProperty", elastic.Validate, elastic.Name);
                else if (property is MembranePlateProperty membrane) Check(result, "InvalidElasticProperty", membrane.Validate, membrane.Name);
            // Descriptive bridge attributes (lane L11) on the wrong kind of element or repeated.
            result.AddRange(global::GPC.Model.Structure.Bridges.BridgeElementAttributes.Diagnose(model));
            return result.AsReadOnly();
        }

        /// <summary>Export preflight: throws <see cref="InvalidOperationException"/> listing the error codes and messages.</summary>
        public static void RequireExportable(this Model model)
        {
            var errors = model.ValidateDomain().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            if (errors.Length != 0) throw new InvalidOperationException("InvalidModelForExport: " + string.Join("; ", errors.Select(d => d.Code + " (" + d.Message + ")")));
        }

        private static void Check(List<ModelDiagnostic> result, string code, Action validate, string owner = null)
        {
            try { validate(); }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException)
            { result.Add(ModelDiagnostic.Error(code, message: owner == null ? ex.Message : owner + ": " + ex.Message)); }
        }
    }
}
