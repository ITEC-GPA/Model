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
        /// <summary>One-based edges; a zero entry is padding and is ignored (legacy BridGHe stores int[4] padded with zeros).</summary>
        [field: DataMember(Order=1)] public int[] SelectedEdges {get;set;} = new int[0];
        /// <summary>Manual weights; on the automatic and angle-split kinds they are kept but not used (see <see cref="IgnoresWeights"/>).</summary>
        [field: DataMember(Order=2)] public double[] EdgeWeights {get;set;} = new double[4];
        /// <summary>The distinct selected edges, zero padding removed, ascending.</summary>
        public int[] Edges => (SelectedEdges??new int[0]).Where(e=>e!=0).Distinct().OrderBy(e=>e).ToArray();
        /// <summary>True when weights are given for a kind that does not use them (legacy data shape, review B7): writers pass zeros.</summary>
        public bool IgnoresWeights => Kind!=LoadPatchDistributionKind.Manual&&EdgeWeights!=null&&EdgeWeights.Any(w=>w!=0);
        /// <summary>The weights the distribution uses: the manual weights, zeros for the other kinds.</summary>
        public double[] EffectiveWeights => Kind==LoadPatchDistributionKind.Manual?(double[])EdgeWeights.Clone():new double[4];
        public void Validate(int edges)
        {
            if(edges!=3&&edges!=4||!Enum.IsDefined(typeof(LoadPatchDistributionKind),Kind)||SelectedEdges==null||EdgeWeights==null||EdgeWeights.Length!=4)
                throw new ArgumentException("InvalidLoadPatchDistribution");
            var selected=SelectedEdges.Where(e=>e!=0).ToArray();
            if(selected.Any(e=>e<1||e>edges)||selected.Distinct().Count()!=selected.Length||EdgeWeights.Any(w=>double.IsNaN(w)||double.IsInfinity(w)||w<0))
                throw new ArgumentException("InvalidLoadPatchEdgesOrWeights");
            if(Kind!=LoadPatchDistributionKind.AutomaticAllEdges&&selected.Length==0)throw new ArgumentException("MissingLoadPatchEdges");
            if(Kind==LoadPatchDistributionKind.Manual&&(Enumerable.Range(1,4).Any(e=>!selected.Contains(e)&&EdgeWeights[e-1]!=0)||!selected.Any(e=>EdgeWeights[e-1]>0)))
                throw new ArgumentException("InconsistentLoadPatchWeights");
        }
    }
}
