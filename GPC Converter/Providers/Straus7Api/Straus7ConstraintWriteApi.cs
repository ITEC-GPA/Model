using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using GPC.Model.Constraints;

namespace GPC.Converter.Straus7
{
    public interface IStraus7ConstraintWriteApi
    {
        /// <summary>Rigid link in all six freedoms (plane 0) between two native nodes, defined in native UCS <paramref name="ucs"/>.</summary>
        void SetRigidLink(int link,int ucs,int master,int slave);
    }
    public sealed partial class Straus7NativeWriter : IStraus7ConstraintWriteApi
    {
        public void SetRigidLink(int link,int ucs,int master,int slave)
        {
            api.CheckWrite(api.WriteFunction<NativeRigidLink>("St7SetRigidLink")(Id,link,ucs,0,new[]{2,master,slave}),"St7SetRigidLink");
        }
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int NativeRigidLink(int file,int link,int ucs,int plane,int[] nodes);
    }
    public sealed partial class Straus7ModelWritePlan
    {
        private sealed partial class Builder
        {
            internal bool RequiresConstraintApi;
            private void Constraints()
            {
                if(Model.Costrains.Count==0)return;
                if(Freedoms.Count>1)throw new NotSupportedException("UnscopedRigidLinkWithMultipleFreedomCases");
                var pairs=new HashSet<string>();int index=0;
                foreach(var constraint in Model.Costrains.Values.OrderBy(c=>c.Id))
                {
                    if(!(constraint is RigidLink link)||link.StartNode==null||link.EndNodes==null||link.EndNodes.Length==0)throw new NotSupportedException("OnlyExplicitRigidBodyConstraintsSupported");
                    var expected=link.EndNodes.SelectMany(n=>RigidLink.GetRigidLink(link.StartNode,n)).ToArray();
                    // Imported or edited equations are authoritative. Reject a stale geometric lever arm instead of silently rebuilding it.
                    if(link.Links==null||link.Links.Length!=expected.Length||link.Links.Where((row,i)=>!SameEquation(row,expected[i])).Any())
                        throw new NotSupportedException("RigidLinkEquationsDoNotMatchCurrentGeometry");
                    foreach(var slave in link.EndNodes)
                    {
                        if(!Entities.TryGetValue(link.StartNode.Guid,out int master)||slave==null||!Entities.TryGetValue(slave.Guid,out int target)||master==target)
                            throw new ArgumentException("InvalidRigidLinkNodes");
                        if(!pairs.Add(Math.Min(master,target)+":"+Math.Max(master,target)))throw new ArgumentException("DuplicateRigidLink");
                        int id=++index;Operations.Add(a=>((IStraus7ConstraintWriteApi)a).SetRigidLink(id,1,master,target)); // UCS 1: global axes
                    }
                }
                RequiresConstraintApi=true;
            }
            private static bool SameEquation(MultiPointsCostrain actual,MultiPointsCostrain expected)
            {
                if(actual==null||actual.ConstValue!=0||actual.Equations==null||actual.Equations.Length!=expected.Equations.Length)return false;
                return !actual.Equations.Where((term,i)=>term.NodeSlave?.Guid!=expected.Equations[i].NodeSlave.Guid||term.GdlNode!=expected.Equations[i].GdlNode||term.Value!=expected.Equations[i].Value).Any();
            }
        }
    }
}
