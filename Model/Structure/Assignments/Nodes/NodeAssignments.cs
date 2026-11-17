using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Model.Restraints;
using GPC.Model.Sections.Concrete;
using GPC.Model.Materials;
using GPC.Model.Results.State;

namespace GPC.Model.Structure.Assignments
{
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class NodeAssignments
    {
        [field: OptionalField]
        [field: DataMember(Name = "<Physical>k__BackingField", IsRequired = false, EmitDefaultValue = false)]
        public NodePhysicalData Physical { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Dofs>k__BackingField", IsRequired = true)]
        // Availability is independent of kinematic constraints and exported results.
        public ComponentAvailability[] Dofs { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Active>k__BackingField", IsRequired = true)]
        public bool? Active { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Restrains>k__BackingField", IsRequired = true)]
        public List<RestrainAssignment> Restrains { get; private set; } = new List<RestrainAssignment>();

        [field: System.Runtime.Serialization.DataMember(Name = "<Links>k__BackingField", IsRequired = true)]
        public List<NodalLink> Links { get; private set; } = new List<NodalLink>();

        [field: System.Runtime.Serialization.DataMember(Name = "<GroundSprings>k__BackingField", IsRequired = true)]
        public List<SpringMatrix> GroundSprings { get; private set; } = new List<SpringMatrix>();

        [field: System.Runtime.Serialization.DataMember(Name = "<Mass>k__BackingField", IsRequired = true)]
        public NodalMass Mass { get; set; }

        /// <summary>The restraints of a freedom case (<see cref="RestrainAssignment.FreedomCase"/> with the Guid of <paramref name="freedomCase"/>;
        /// null: the restraints without a freedom case), for example those of the freedom case of a load case (<see cref="LoadCaseBase.FreedomCase"/>).
        /// A filter of <see cref="Restrains"/>, in their order.</summary>
        public IEnumerable<RestrainAssignment> RestrainsOf(GPC.Model.FreedomCases.FreedomCase freedomCase)
            => Restrains.Where(r => r != null && r.FreedomCase?.Guid == freedomCase?.Guid);

        /// <summary>The springs to ground of a freedom case (<see cref="SpringMatrix.FreedomCase"/>, as <see cref="RestrainsOf"/>).</summary>
        public IEnumerable<SpringMatrix> GroundSpringsOf(GPC.Model.FreedomCases.FreedomCase freedomCase)
            => GroundSprings.Where(s => s != null && s.FreedomCase?.Guid == freedomCase?.Guid);

        /// <summary>Add rejects overlapping DOFs in the same case/freedom case/phase; ReplaceScope explicitly replaces that scope.</summary>
        public void AssignRestrain(RestrainAssignment assignment, AssignmentMode mode)
        {
            if (assignment?.Restrain == null)
                throw new ArgumentNullException(nameof(assignment));
            if (!Enum.IsDefined(typeof(AssignmentMode), mode))
                throw new ArgumentOutOfRangeException(nameof(mode));
            var scope = Restrains.Where(a => Equals(a.Case, assignment.Case) && a.FreedomCase?.Guid == assignment.FreedomCase?.Guid && a.Phase == assignment.Phase).ToArray();
            if (mode == AssignmentMode.Add && scope.Any(a => a.Restrain.Restrains.Any(d => assignment.Restrain.Restrains.Any(n => n.Dof == d.Dof))))
                throw new InvalidOperationException("ConflictingRestrain: select ReplaceScope explicitly.");
            if (mode == AssignmentMode.ReplaceScope)
                foreach (var old in scope)
                    Restrains.Remove(old);
            Restrains.Add(assignment);
        }
    }
}
