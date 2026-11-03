using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    public enum AluminiumManufacturingForm { Unspecified, Wrought, CastMold, CastSand }

    /// <summary>Declared material data [MPa], independent of the constitutive curve and design resistance.
    /// No consumer may substitute these values for characteristic strengths without an explicit method.
    /// Null means not declared; zero is retained as supplied by legacy documents.</summary>
    [Serializable, DataContract]
    public sealed class MaterialDeclarations
    {
        [DataMember(Order=9)] private int schema=1;
        [field:DataMember(Order=0)] public double? ExpectedYieldStress { get; set; }
        [field:DataMember(Order=1)] public double? ExpectedTensileStrength { get; set; }
        [field:DataMember(Order=2)] public double? ExpectedCompressiveStrength { get; set; }
        [field:DataMember(Order=3)] public double? CompressiveYieldStrength { get; set; }
        [field:DataMember(Order=4)] public double? TensileYieldStrength { get; set; }
        [field:DataMember(Order=5)] public double? TensileUltimateStrength { get; set; }
        [field:DataMember(Order=6)] public double? ShearUltimateStrength { get; set; }
        [field:DataMember(Order=7)] public string Alloy { get; set; }
        [field:DataMember(Order=8)] public AluminiumManufacturingForm AluminiumForm { get; set; }

        public void Validate()
        {
            Schema();
            foreach(var value in new[]{ExpectedYieldStress,ExpectedTensileStrength,ExpectedCompressiveStrength,
                CompressiveYieldStrength,TensileYieldStrength,TensileUltimateStrength,ShearUltimateStrength})
                if(value.HasValue&&(double.IsNaN(value.Value)||double.IsInfinity(value.Value)||value.Value<0))
                    throw new ArgumentException("InvalidDeclaredMaterialStrength");
            if(!Enum.IsDefined(typeof(AluminiumManufacturingForm),AluminiumForm))throw new ArgumentException("UnknownAluminiumManufacturingForm");
        }
        // Reading checks the schema only; the values are validated by the edit session and the material property constructors.
        private void Schema(){if(schema!=1)throw new SerializationException("UnsupportedMaterialDeclarationsSchema");}
        [OnDeserialized] private void Read(StreamingContext _)=>Schema();
        public override bool Equals(object obj)=>obj is MaterialDeclarations other&&Core.ModelValues.ContentEquals(this,other);
        public override int GetHashCode()=>Core.ModelValues.ContentHash(this);
    }
}
