using System;
using System.Collections.Generic;
using GPC.Model.Constraints;
using GPC.Model.Restraints;
using GPC.Model.Core.Coordinates;
using GPC.Model.Structure.Assignments;

namespace GPC.Model.Results.Processing
{
    public static class NodalKinematics
    {
        /// <summary>Expresses the scoped nodal constraints in global DOFs. Springs remain separate operators.</summary>
        public static IReadOnlyList<MultiPointsCostrain> ToGlobalEquations(RestrainAssignment assignment)
        {
            if (assignment?.Restrain == null) throw new ArgumentNullException(nameof(assignment));
            var restraint = assignment.Restrain; Axes.Validate(restraint.CoordinateSystem);
            var axes = new[] { restraint.CoordinateSystem.V1, restraint.CoordinateSystem.V2, restraint.CoordinateSystem.V3 };
            var equations = new List<MultiPointsCostrain>();
            foreach (var dof in restraint.Restrains)
            {
                int index = (int)dof.Dof;
                if (index < 0 || index >= 6) throw new NotSupportedException("UnsupportedDof");
                if (!dof.IsRestrained && !dof.HasImposedDisplacement) continue;
                if (dof.HasImposedDisplacement && assignment.Case == null) throw new InvalidOperationException("MissingImposedDisplacementCase");
                if (dof.IsRestrained && dof.HasImposedDisplacement && dof.ImposedDisplacement != 0) throw new InvalidOperationException("ConflictingRestrain");
                var axis = axes[index % 3]; int start = 3 * (index / 3);
                equations.Add(new MultiPointsCostrain(new[]{
                    new MultiPointsCostrain.Equation(restraint.Point,(GeometryRestrain.DOF)start,axis.X),
                    new MultiPointsCostrain.Equation(restraint.Point,(GeometryRestrain.DOF)(start+1),axis.Y),
                    new MultiPointsCostrain.Equation(restraint.Point,(GeometryRestrain.DOF)(start+2),axis.Z)},
                    dof.HasImposedDisplacement ? dof.ImposedDisplacement : 0));
            }
            return equations.AsReadOnly();
        }
    }
}
