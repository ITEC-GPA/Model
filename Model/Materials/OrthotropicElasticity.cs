using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    /// <summary>Linear orthotropic elasticity in material axes. E and G: N/mm².
    /// Nu12=-epsilon2/epsilon1 under uniaxial stress1; likewise Nu23 and Nu31.
    /// Reciprocal coefficients follow symmetry of compliance. Engineering shear strains are used.</summary>
    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.Materials")]
    public sealed class OrthotropicElasticity : IEquatable<OrthotropicElasticity>
    {
        [field: DataMember(Order = 0)] public double E1 { get; set; }
        [field: DataMember(Order = 1)] public double E2 { get; set; }
        [field: DataMember(Order = 2)] public double E3 { get; set; }
        [field: DataMember(Order = 3)] public double G12 { get; set; }
        [field: DataMember(Order = 4)] public double G23 { get; set; }
        [field: DataMember(Order = 5)] public double G31 { get; set; }
        [field: DataMember(Order = 6)] public double Nu12 { get; set; }
        [field: DataMember(Order = 7)] public double Nu23 { get; set; }
        [field: DataMember(Order = 8)] public double Nu31 { get; set; }

        public void Validate()
        {
            Positive(E1); Positive(E2); Positive(E3); Positive(G12); Positive(G23); Positive(G31);
            Finite(Nu12); Finite(Nu23); Finite(Nu31);
            // Congruence with diag(sqrt(Ei)) avoids taking determinants of dimensional, tiny compliances.
            double a = -Nu12 * Math.Sqrt(E2) / Math.Sqrt(E1), b = -Nu31 * Math.Sqrt(E1) / Math.Sqrt(E3),
                c = -Nu23 * Math.Sqrt(E3) / Math.Sqrt(E2);
            Finite(a); Finite(b); Finite(c);
            double d = 1-a*a;
            if (!(d > 1e-12) || !(1-b*b-(c-a*b)*(c-a*b)/d > 1e-12))
                throw new ArgumentException("NonPositiveDefiniteOrthotropicCompliance");
        }
        /// <summary>Row-major 6x6 compliance [11,22,33,12,23,31], units mm²/N.</summary>
        public double[] Compliance()
        {
            Validate(); var s=new double[36];
            s[0]=1/E1; s[7]=1/E2; s[14]=1/E3; s[21]=1/G12; s[28]=1/G23; s[35]=1/G31;
            s[1]=s[6]=-Nu12/E1; s[2]=s[12]=-Nu31/E3; s[8]=s[13]=-Nu23/E2;
            foreach(var value in s) Finite(value); return s;
        }
        public bool Equals(OrthotropicElasticity o) => !(o is null) && E1==o.E1 && E2==o.E2 && E3==o.E3 && G12==o.G12 && G23==o.G23 && G31==o.G31 && Nu12==o.Nu12 && Nu23==o.Nu23 && Nu31==o.Nu31;
        public override bool Equals(object o) => Equals(o as OrthotropicElasticity);
        public override int GetHashCode() => E1.GetHashCode() ^ E2.GetHashCode() ^ E3.GetHashCode() ^ G12.GetHashCode() ^ G23.GetHashCode() ^ G31.GetHashCode() ^ Nu12.GetHashCode() ^ Nu23.GetHashCode() ^ Nu31.GetHashCode();
        private static void Finite(double v) { if (double.IsNaN(v)||double.IsInfinity(v)) throw new ArgumentException("NonFiniteOrthotropicProperty"); }
        private static void Positive(double v) { Finite(v); if(v<=0) throw new ArgumentException("NonPositiveOrthotropicModulus"); }
    }

    /// <summary>Physical fabric properties per unit width, independent of a fictitious FEM thickness.
    /// Stiffnesses and strengths: N/mm; mass: tonne/mm². Null means not supplied, never an inferred zero.
    /// Warp/weft map to material axes 1/2. NuWarpWeft=-epsilonWeft/epsilonWarp under warp tension.</summary>
    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.Materials")]
    public sealed class MembraneMaterialData
    {
        [field: DataMember(Order = 0)] public string Name { get; set; }
        [field: DataMember(Order = 1)] public double MassPerArea { get; set; }
        [field: DataMember(Order = 2)] public double? StiffnessWarp { get; set; }
        [field: DataMember(Order = 3)] public double? StiffnessWeft { get; set; }
        [field: DataMember(Order = 4)] public double? ShearStiffness { get; set; }
        [field: DataMember(Order = 5)] public double? NuWarpWeft { get; set; }
        [field: DataMember(Order = 6)] public double? StrengthWarp { get; set; }
        [field: DataMember(Order = 7)] public double? StrengthWeft { get; set; }

        public void Validate()
        {
            Finite(MassPerArea); if(MassPerArea<0) throw new ArgumentException("NegativeMembraneMass");
            foreach(var v in new[]{StiffnessWarp,StiffnessWeft,ShearStiffness,StrengthWarp,StrengthWeft})
                if(v.HasValue) { Finite(v.Value); if(v.Value<=0) throw new ArgumentException("NonPositiveMembraneProperty"); }
            if(NuWarpWeft.HasValue)
            {
                Finite(NuWarpWeft.Value);
                if(!StiffnessWarp.HasValue || !StiffnessWeft.HasValue) throw new ArgumentException("MissingMembraneNormalStiffness");
                double coupling=NuWarpWeft.Value*Math.Sqrt(StiffnessWeft.Value)/Math.Sqrt(StiffnessWarp.Value);
                if(!(1-coupling*coupling>1e-12)) throw new ArgumentException("NonPositiveDefiniteMembraneCompliance");
            }
        }
        /// <summary>Row-major 3x3 membrane stiffness [N11,N22,N12] / [epsilon11,epsilon22,gamma12], N/mm.</summary>
        public double[] Stiffness()
        {
            Validate();
            if(!StiffnessWarp.HasValue||!StiffnessWeft.HasValue||!ShearStiffness.HasValue||!NuWarpWeft.HasValue)
                throw new InvalidOperationException("IncompleteMembraneConstitutiveData");
            double e1=StiffnessWarp.Value,e2=StiffnessWeft.Value,nu=NuWarpWeft.Value;
            double coupling=nu*Math.Sqrt(e2)/Math.Sqrt(e1),den=1-coupling*coupling;
            var result=new[]{e1/den,nu*e2/den,0,nu*e2/den,e2/den,0,0,0,ShearStiffness.Value};
            foreach(var value in result) Finite(value); return result;
        }
        private static void Finite(double v) { if(double.IsNaN(v)||double.IsInfinity(v)) throw new ArgumentException("NonFiniteMembraneProperty"); }
    }
}
