using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using GPC.Model.Checking.Contracts;
using GPC.Model.Core.Diagnostics;

namespace GPC.Model.Checking.Preparation
{
    public sealed class ShellActionPreparation
    {
        public DataStatus Status { get; internal set; }
        public ResultPlateForces DesignActions { get; internal set; }
        public string Method { get; internal set; }
        public IReadOnlyList<ModelDiagnostic> Diagnostics { get; internal set; }

        /// <summary>Compatibility facade over the explicit shell design-policy service.</summary>
        public static ShellActionPreparation Prepare(Models.Model model, int shellId, PointResultPlateForces sample, IShellDesignActionMethod method)
            => Design.Plate.ShellDesignActions.Prepare(model, shellId, sample, method);
    }
}