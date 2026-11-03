using System;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Structure.Assignments
{
    public enum LoadPatchDistributionKind { AutomaticOneEdge, AutomaticTwoEdges, AutomaticThreeEdges, AutomaticAllEdges, AngleSplit, Manual }
    /// <summary>Distribution of an unloaded-stiffness surface to its supporting edge beams.
    /// Edge indices are one-based in element connectivity order. Manual weights are dimensionless scaling factors.</summary>
    [Serializable, DataContract(Namespace="http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class LoadPatchDistribution
    {
        [field: DataMember(Order=0)] public LoadPatchDistributionKind Kind {get;set;} = LoadPatchDistributionKind.AutomaticAllEdges;
        [field: DataMember(Order=1)] public int[] SelectedEdges {get;set;} = new int[0];
        [field: DataMember(Order=2)] public double[] EdgeWeights {get;set;} = new double[4];
        public void Validate(int edges)
        {
            if(edges!=3&&edges!=4||!Enum.IsDefined(typeof(LoadPatchDistributionKind),Kind)||SelectedEdges==null||EdgeWeights==null||EdgeWeights.Length!=4)
                throw new ArgumentException("InvalidLoadPatchDistribution");
            if(SelectedEdges.Any(e=>e<1||e>edges)||SelectedEdges.Distinct().Count()!=SelectedEdges.Length||EdgeWeights.Any(w=>double.IsNaN(w)||double.IsInfinity(w)||w<0))
                throw new ArgumentException("InvalidLoadPatchEdgesOrWeights");
            if(Kind!=LoadPatchDistributionKind.AutomaticAllEdges&&SelectedEdges.Length==0)throw new ArgumentException("MissingLoadPatchEdges");
            if(Kind!=LoadPatchDistributionKind.Manual&&EdgeWeights.Any(w=>w!=0))throw new ArgumentException("LoadPatchWeightsRequireManualDistribution");
            if(Kind==LoadPatchDistributionKind.Manual&&(Enumerable.Range(1,4).Any(e=>!SelectedEdges.Contains(e)&&EdgeWeights[e-1]!=0)||!SelectedEdges.Any(e=>EdgeWeights[e-1]>0)))
                throw new ArgumentException("InconsistentLoadPatchWeights");
        }
    }
}
