using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    public enum MaterialNonlinearResponse { NonlinearElastic, Elastoplastic }
    public enum MaterialHardening { Isotropic, Kinematic, Takeda }

    /// <summary>Explicit analysis properties, independent of characteristic/design strength tables.
    /// N, mm, s, tonne and Celsius. Thermal energy uses N mm; adapters convert to their declared energy unit.</summary>
    [Serializable,DataContract]
    public sealed class MaterialAnalysisData
    {
        /// <summary>Optional beam shear modulus [N/mm²], used instead of deriving G from E and nu.</summary>
        [field:DataMember(Order=0)] public double? BeamShearModulus { get; set; }
        /// <summary>Viscous damping per volume [N s/mm⁴].</summary>
        [field:DataMember(Order=1)] public double ViscousDampingDensity { get; set; }
        [field:DataMember(Order=2)] public double ModalDampingRatio { get; set; }
        /// <summary>Isotropic heat conductivity [N/(s °C)] = N mm/(s mm °C).</summary>
        [field:DataMember(Order=3)] public double ThermalConductivity { get; set; }
        /// <summary>Heat capacity [N mm/(tonne °C)].</summary>
        [field:DataMember(Order=4)] public double SpecificHeat { get; set; }
        /// <summary>Optional ordered pairs [strain, stress MPa], nonnegative tension branch starting at (0,0).
        /// Explicit constitutive input, not a design resistance curve. Compression uses the symmetric branch.</summary>
        [field:DataMember(Order=5)] public double[] StressStrainPairs { get; set; }
        [field:DataMember(Order=6)] public MaterialNonlinearResponse NonlinearResponse { get; set; }
        [field:DataMember(Order=7)] public MaterialHardening Hardening { get; set; }
        public void Validate()
        {
            if(BeamShearModulus.HasValue){Nonnegative(BeamShearModulus.Value);if(BeamShearModulus.Value==0)throw new ArgumentException("ZeroBeamShearModulus");}
            Nonnegative(ViscousDampingDensity);Nonnegative(ModalDampingRatio);Nonnegative(ThermalConductivity);Nonnegative(SpecificHeat);
            if(!Enum.IsDefined(typeof(MaterialNonlinearResponse),NonlinearResponse)||!Enum.IsDefined(typeof(MaterialHardening),Hardening))throw new ArgumentException("UnknownMaterialAnalysisLaw");
            if(StressStrainPairs!=null)
            {
                if(StressStrainPairs.Length<4||StressStrainPairs.Length%2!=0||StressStrainPairs[0]!=0||StressStrainPairs[1]!=0)throw new ArgumentException("InvalidAnalysisStressStrainTable");
                for(int i=0;i<StressStrainPairs.Length;i+=2)
                {Nonnegative(StressStrainPairs[i]);Nonnegative(StressStrainPairs[i+1]);if(i>0&&StressStrainPairs[i]<=StressStrainPairs[i-2])throw new ArgumentException("UnorderedAnalysisStressStrainTable");}
            }
            else if(NonlinearResponse!=MaterialNonlinearResponse.NonlinearElastic||Hardening!=MaterialHardening.Isotropic)throw new ArgumentException("MaterialLawRequiresStressStrainTable");
        }
        private static void Nonnegative(double value){if(double.IsNaN(value)||double.IsInfinity(value)||value<0)throw new ArgumentException("InvalidMaterialAnalysisValue");}
        public override bool Equals(object obj)=>obj is MaterialAnalysisData other&&Core.ModelValues.Fingerprint(new object[]{this})==Core.ModelValues.Fingerprint(new object[]{other});
        public override int GetHashCode()=>23;
    }
}
