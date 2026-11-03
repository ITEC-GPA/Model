using System;
using System.Runtime.Serialization;

namespace GPC.Model.ElementProperties
{
    public enum BeamCutoffFailure { Brittle, Ductile }
    /// <summary>Axial force limits in N, nonnegative magnitudes. Brittle removes force after failure;
    /// ductile caps force. KeepMass controls mass after failure, independently from axial resistance.</summary>
    [Serializable,DataContract]
    public sealed class BeamCutoffLaw
    {
        [field:DataMember(Order=0)] public BeamCutoffFailure Failure {get;set;}
        [field:DataMember(Order=1)] public double TensionLimit {get;set;}
        [field:DataMember(Order=2)] public double CompressionLimit {get;set;}
        [field:DataMember(Order=3)] public bool KeepMass {get;set;}
        public void Validate()
        {
            if(!Enum.IsDefined(typeof(BeamCutoffFailure),Failure))throw new ArgumentException("InvalidCutoffFailure");
            ElasticPropertyValidation.Nonnegative(TensionLimit,"TensionLimit");ElasticPropertyValidation.Nonnegative(CompressionLimit,"CompressionLimit");
        }
        [OnDeserialized]private void OnRead(StreamingContext context)=>Validate();
    }
}
