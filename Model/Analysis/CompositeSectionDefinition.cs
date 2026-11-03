using System;
using System.Runtime.Serialization;
using GPC.Model.Sections.Concrete;

namespace GPC.Model.Analysis
{
    /// <summary>Standalone physical composite section and explicit homogenization inputs.
    /// Ratios Es/Ec are dimensionless. These inputs do not replace the physical section by a transformed one.
    /// Long/short/shrinkage are selected by the caller's phase, never inferred from a load-case name.</summary>
    [Serializable,DataContract(Namespace="http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class CompositeSectionDefinition
    {
        [DataMember(Order=0)] private int schema=1;
        [field:DataMember(Order=1)] public Guid Id { get; set; }=Guid.NewGuid();
        [field:DataMember(Order=2)] public ReinforcedConcreteSection PhysicalSection { get; set; }
        [field:DataMember(Order=3)] public double LongTermRatio { get; set; }
        [field:DataMember(Order=4)] public double ShortTermRatio { get; set; }
        /// <summary>Null means the explicitly documented long-term fallback.</summary>
        [field:DataMember(Order=5)] public double? ShrinkageRatio { get; set; }
        [field:DataMember(Order=6)] public bool IncludeConcrete { get; set; }=true;
        [field:DataMember(Order=7)] public bool IncludeReinforcement { get; set; }=true;
        public double EffectiveShrinkageRatio=>ShrinkageRatio??LongTermRatio;
        public void Validate()
        {
            if(schema!=1)throw new SerializationException("UnsupportedCompositeSectionDefinitionSchema");
            if(Id==Guid.Empty||PhysicalSection==null)throw new ArgumentException("MissingCompositePhysicalSection");
            foreach(var n in new[]{LongTermRatio,ShortTermRatio,EffectiveShrinkageRatio})
                if(double.IsNaN(n)||double.IsInfinity(n)||n<=0)throw new ArgumentException("InvalidCompositeModularRatio");
        }
        [OnDeserialized] private void Read(StreamingContext _)=>Validate();
    }
}
