using System;
using System.IO;
using System.Linq;
using GPC.Model.PostProcessing;

namespace GPC.Converter
{
    /// <summary>Ties results read later from a solver to the model imported from it, also after the model has been saved, reopened
    /// or completed for the checks. The import stores the analysis fingerprint and the solver binding fingerprint of the model
    /// (<see cref="ModelRevisions.SolverBindingFingerprint"/>). Results are accepted while the solver binding is unchanged: geometry,
    /// topology, axes, offsets, restraints, loads and load cases. Edited properties, materials, physical thicknesses or combinations
    /// give a warning only, because the results stay those of the analysed solver model. Bindings stored before the solver binding
    /// existed hold the analysis fingerprint alone and stay exact.</summary>
    public static class SourceBinding
    {
        private const string Version = "2";
        public const string PropertiesChangedCode = "ModelPropertiesChangedSinceImport";

        public static PreservedAssignment Create(string kind, string sourceHash, GPC.Model.Models.Model model, string reason)
        {
            if (string.IsNullOrWhiteSpace(kind) || string.IsNullOrWhiteSpace(sourceHash) || model == null) throw new ArgumentException("Explicit binding kind, source and model required.");
            return new PreservedAssignment { Kind = kind, SourceRecord = sourceHash, UnsupportedReason = reason,
                RawData = Version + "\n" + model.AnalysisFingerprint() + "\n" + model.SolverBindingFingerprint() };
        }

        /// <summary>Throws <see cref="InvalidDataException"/> when the model has no single binding to this source or its solver inputs changed;
        /// returns a warning when only properties, materials, thicknesses or combinations changed, otherwise null.</summary>
        public static ModelDiagnostic Check(GPC.Model.Models.Model model, string kind, string sourceHash, string program)
        {
            var bindings = model.PreservedSourceData.Where(p => p.Kind == kind && p.SourceRecord == sourceHash).ToArray();
            if (bindings.Length != 1) throw new InvalidDataException("The model has no binding to this " + program + " model: import it from the same source before reading results.");
            var parts = (bindings[0].RawData ?? "").Split('\n');
            if (parts.Length == 3 && parts[0] == Version)
            {
                if (parts[2] != model.SolverBindingFingerprint())
                    throw new InvalidDataException("Geometry, topology, axes, offsets, restraints, loads or load cases changed since the " + program
                        + " import: the results no longer belong to this model.");
                if (parts[1] == model.AnalysisFingerprint()) return null;
                return new ModelDiagnostic { Code = PropertiesChangedCode, Severity = DiagnosticSeverity.Warning,
                    Message = "Properties, materials, thicknesses or combinations changed since the " + program
                        + " import: the results are those of the analysed model, not of the edited properties." };
            }
            if (parts.Length == 1 && parts[0] == model.AnalysisFingerprint()) return null;
            throw new InvalidDataException("Model inputs changed since the " + program + " import (exact binding of an earlier import).");
        }
    }
}
