using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Model.Restrains;
using GPC.Model.Sections.Concrete;
using GPC.Model.Materials;

namespace GPC.Model.PostProcessing
{

    [Serializable]
    public sealed class NodeAssignments
    {
        // Availability is independent of kinematic constraints and exported results.
        public ComponentAvailability[] Dofs { get; set; }
        public bool? Active { get; set; }
        public List<RestrainAssignment> Restrains { get; private set; } = new List<RestrainAssignment>();
        public List<NodalLink> Links { get; private set; } = new List<NodalLink>();
        public List<SpringMatrix> GroundSprings { get; private set; } = new List<SpringMatrix>();
        public NodalMass Mass { get; set; }

        /// <summary>Add rejects overlapping DOFs in the same case/phase; ReplaceScope explicitly replaces that scope.</summary>
        public void AssignRestrain(RestrainAssignment assignment, AssignmentMode mode)
        {
            if (assignment?.Restrain == null) throw new ArgumentNullException(nameof(assignment));
            if (!Enum.IsDefined(typeof(AssignmentMode), mode)) throw new ArgumentOutOfRangeException(nameof(mode));
            var scope = Restrains.Where(a => Equals(a.Case, assignment.Case) && a.Phase == assignment.Phase).ToArray();
            if (mode == AssignmentMode.Add && scope.Any(a => a.Restrain.Restrains.Any(d => assignment.Restrain.Restrains.Any(n => n.Dof == d.Dof))))
                throw new InvalidOperationException("ConflictingRestrain: select ReplaceScope explicitly.");
            if (mode == AssignmentMode.ReplaceScope) foreach (var old in scope) Restrains.Remove(old);
            Restrains.Add(assignment);
        }
    }
}
