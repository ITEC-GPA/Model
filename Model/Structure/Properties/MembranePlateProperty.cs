using System;
using System.Runtime.Serialization;

namespace GPC.Model.ElementProperties
{
    /// <summary>
    /// Fabric membrane plate property, registered once in the model and shared by its elements (review B3). Physical properties per
    /// unit width, independent of a fictitious FEM thickness: stiffnesses and strengths in N/mm, mass in tonne/mm². Null means not
    /// supplied, never an inferred zero. Warp/weft map to the material axes 1/2 of the element layer axes; NuWarpWeft =
    /// -epsilonWeft/epsilonWarp under warp tension. There is no isotropic material and no physical thickness: consumers that need
    /// one (<see cref="IFemPlateProperty"/>, the shell checks) must reject it.
    /// </summary>
    [Serializable]
    public sealed class MembranePlateProperty : PlateProperty
    {
        public double MassPerArea { get; set; }
        public double? StiffnessWarp { get; set; }
        public double? StiffnessWeft { get; set; }
        public double? ShearStiffness { get; set; }
        public double? NuWarpWeft { get; set; }
        public double? StrengthWarp { get; set; }
        public double? StrengthWeft { get; set; }

        public MembranePlateProperty(string name) : base(name) { }

        private MembranePlateProperty(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            // Schema check only: the values are validated by the edit session and the exporters (review B1).
            SerializationVersions.Read(info, "MembranePlatePropertyVersion", 1);
            MassPerArea = info.GetDouble("MassPerArea");
            StiffnessWarp = SerializationFields.Read<double?>(info, "StiffnessWarp"); StiffnessWeft = SerializationFields.Read<double?>(info, "StiffnessWeft");
            ShearStiffness = SerializationFields.Read<double?>(info, "ShearStiffness"); NuWarpWeft = SerializationFields.Read<double?>(info, "NuWarpWeft");
            StrengthWarp = SerializationFields.Read<double?>(info, "StrengthWarp"); StrengthWeft = SerializationFields.Read<double?>(info, "StrengthWeft");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context); info.AddValue("MembranePlatePropertyVersion", 1); info.AddValue("MassPerArea", MassPerArea);
            info.AddValue("StiffnessWarp", StiffnessWarp); info.AddValue("StiffnessWeft", StiffnessWeft); info.AddValue("ShearStiffness", ShearStiffness);
            info.AddValue("NuWarpWeft", NuWarpWeft); info.AddValue("StrengthWarp", StrengthWarp); info.AddValue("StrengthWeft", StrengthWeft);
        }

        public void Validate()
        {
            if (PhysicalThickness.HasValue) throw new ArgumentException("MembraneHasNoPhysicalThickness");
            Finite(MassPerArea); if (MassPerArea < 0) throw new ArgumentException("NegativeMembraneMass");
            foreach (var v in new[] { StiffnessWarp, StiffnessWeft, ShearStiffness, StrengthWarp, StrengthWeft })
                if (v.HasValue) { Finite(v.Value); if (v.Value <= 0) throw new ArgumentException("NonPositiveMembraneProperty"); }
            if (NuWarpWeft.HasValue)
            {
                Finite(NuWarpWeft.Value);
                if (!StiffnessWarp.HasValue || !StiffnessWeft.HasValue) throw new ArgumentException("MissingMembraneNormalStiffness");
                double coupling = NuWarpWeft.Value * Math.Sqrt(StiffnessWeft.Value) / Math.Sqrt(StiffnessWarp.Value);
                if (!(1 - coupling * coupling > 1e-12)) throw new ArgumentException("NonPositiveDefiniteMembraneCompliance");
            }
        }

        /// <summary>Row-major 3x3 membrane stiffness [N11,N22,N12] / [epsilon11,epsilon22,gamma12], N/mm.</summary>
        public double[] Stiffness()
        {
            Validate();
            if (!StiffnessWarp.HasValue || !StiffnessWeft.HasValue || !ShearStiffness.HasValue || !NuWarpWeft.HasValue)
                throw new InvalidOperationException("IncompleteMembraneConstitutiveData");
            double e1 = StiffnessWarp.Value, e2 = StiffnessWeft.Value, nu = NuWarpWeft.Value;
            double coupling = nu * Math.Sqrt(e2) / Math.Sqrt(e1), den = 1 - coupling * coupling;
            var result = new[] { e1 / den, nu * e2 / den, 0, nu * e2 / den, e2 / den, 0, 0, 0, ShearStiffness.Value };
            foreach (var value in result) Finite(value); return result;
        }

        private static void Finite(double v) { if (double.IsNaN(v) || double.IsInfinity(v)) throw new ArgumentException("NonFiniteMembraneProperty"); }

        /// <summary>Same name and same fingerprinted content; never throws.</summary>
        public override bool Equals(object obj) => obj is MembranePlateProperty other && base.Equals(other) && GPC.Model.Core.ModelValues.ContentEquals(this, other);
        public override int GetHashCode() => Name == null ? 0 : StringComparer.Ordinal.GetHashCode(Name);
    }
}
