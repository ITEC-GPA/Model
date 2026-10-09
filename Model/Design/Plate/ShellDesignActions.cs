using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Preparation;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;

namespace GPC.Model.Design.Plate
{
    /// <summary>Applies a caller-selected design method after physical validation. No plate verifier is selected implicitly.</summary>
    public static class ShellDesignActions
    {
        /// <summary>Validates and rotates all eight concomitant values before an explicitly selected structural method.</summary>
        public static ShellActionPreparation Prepare(Models.Model model, int shellId, PointResultPlateForces sample, IShellDesignActionMethod method)
        {
            var shell = model.AreaElements[shellId];
            var prepared = ShellInputPreparation.Prepare(model, shellId, sample, null);
            var errors = prepared.Diagnostics.ToList(); var status = prepared.Status;
            if (method == null) { errors.Add(ModelDiagnostic.Error("MissingShellDesignMethod", shell)); if (status != DataStatus.Stale) status = DataStatus.NotSupported; }
            if (errors.Count > 0) return new ShellActionPreparation { Status = status, Diagnostics = errors };
            try
            {
                var target = prepared.Input.LocalForces.CoordinateSystem;
                var design = method.DesignActions(prepared.Input.LocalForces, prepared.Input.Assignments);
                CheckFinite(design); Axes.Validate(design.CoordinateSystem);
                if (string.IsNullOrWhiteSpace(method.NameAndVersion) || Axes.Length(design.CoordinateSystem.Origin - target.Origin) > 1e-8
                    || Axes.Dot(design.CoordinateSystem.V1, target.V1) < 1 - 1e-10 || Axes.Dot(design.CoordinateSystem.V2, target.V2) < 1 - 1e-10)
                    throw new ArgumentException("Design method must identify itself and return actions in the supplied reinforcement axes.");
                if (!prepared.Input.IsCurrent) return new ShellActionPreparation { Status = DataStatus.Stale,
                    Diagnostics = new[] { ModelDiagnostic.Error("ShellInputsChangedDuringPreparation", shell) } };
                return new ShellActionPreparation { Status = DataStatus.Ready, DesignActions = design, Method = method.NameAndVersion, Diagnostics = errors };
            }
            catch (NotSupportedException ex) { errors.Add(ModelDiagnostic.Error("UnsupportedShellTransformation", shell, ex.Message)); status = DataStatus.NotSupported; }
            catch (ArgumentException ex) { errors.Add(ModelDiagnostic.Error("InvalidShellDesignActions", shell, ex.Message)); status = DataStatus.Insufficient; }
            return new ShellActionPreparation { Status = status, Diagnostics = errors };
        }
        private static void CheckFinite(ResultPlateForces f)
        {
            if (f == null) throw new ArgumentException("Missing shell resultants.");
            foreach (var v in new[] { f.Fxx, f.Fyy, f.Fxy, f.Fxz, f.Fyz, f.Mxx, f.Myy, f.Mxy }) NumericGuard.Finite(v, "shellResultant");
        }
    }
}
