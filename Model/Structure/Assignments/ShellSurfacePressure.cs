using System;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Core.Coordinates;
using GPC.Model.LoadCases;

namespace GPC.Model.Structure.Assignments
{
    public enum SurfacePressureProjection { None, Resultant, Components }
    /// <summary>Uniform pressure on an outer shell face, N/mm². Exactly one of GlobalPressure or NormalPressure is required.
    /// Global directions stay fixed. Normal pressure follows the current face and is positive INWARDS on that face.
    /// Projection applies to global pressure only. Face geometry includes shell thickness and offset.</summary>
    [Serializable,DataContract(Namespace="http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class ShellSurfacePressure
    {
        [field:DataMember(Order=0)] public LoadCaseBase Case {get;set;}
        [field:DataMember(Order=1)] public ShellFace Face {get;set;}
        [field:DataMember(Order=2)] public Vector3d GlobalPressure {get;set;}
        [field:DataMember(Order=3)] public double? NormalPressure {get;set;}
        [field:DataMember(Order=4)] public SurfacePressureProjection Projection {get;set;}
        public void Validate()
        {
            if(Case==null||string.IsNullOrWhiteSpace(Case.Name)||!Enum.IsDefined(typeof(ShellFace),Face)||!Enum.IsDefined(typeof(SurfacePressureProjection),Projection)
                ||(GlobalPressure!=null)==NormalPressure.HasValue)throw new ArgumentException("InvalidShellSurfacePressure");
            if(GlobalPressure!=null&&!Axes.IsFinite(GlobalPressure))throw new ArgumentException("NonFiniteShellSurfacePressure");
            if(NormalPressure.HasValue&&(double.IsNaN(NormalPressure.Value)||double.IsInfinity(NormalPressure.Value)||Projection!=SurfacePressureProjection.None))
                throw new ArgumentException("InvalidNormalSurfacePressure");
        }
        [OnDeserialized] private void OnRead(StreamingContext context)=>Validate();
    }
}
