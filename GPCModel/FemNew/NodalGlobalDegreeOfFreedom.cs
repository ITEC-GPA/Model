using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem.FemObjects;

namespace GPC.Model.Fem
{

    public struct NodalGlobalDegreeOfFreedom : IEquatable<NodalGlobalDegreeOfFreedom>
    {
        public DegreeOfFreedoms.GlobalDegreeOfFreedoms DegreeOfFreedom { get; }

        public Node Node { get; }


        public NodalGlobalDegreeOfFreedom(Node node, DegreeOfFreedoms.GlobalDegreeOfFreedoms degreeOfFreedom)
        {
            DegreeOfFreedom = degreeOfFreedom;
            Node = node ?? throw new ArgumentNullException(nameof(node));
        }



        #region Equals, Hascode, operators

        public override bool Equals(object obj)
        {
            return obj is NodalGlobalDegreeOfFreedom freedom && Equals(freedom);
        }

        public bool Equals(NodalGlobalDegreeOfFreedom other)
        {
            return DegreeOfFreedom == other.DegreeOfFreedom && Node.Equals(other.Node);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -17;
                hashCode = hashCode * -23 + DegreeOfFreedom.GetHashCode();
                hashCode = hashCode * -23 + Node.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(NodalGlobalDegreeOfFreedom left, NodalGlobalDegreeOfFreedom right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NodalGlobalDegreeOfFreedom left, NodalGlobalDegreeOfFreedom right)
        {
            return !(left == right);
        }


        #endregion

    }

    public static class NodalGlobalDegreeOfFreedomExtensions
    {
        public static NodalLocalDegreeOfFreedom[] ToLocal(this NodalGlobalDegreeOfFreedom[] global)
        {
            return global.Select(i => i.ToLocal()).ToArray();
        }

        public static NodalLocalDegreeOfFreedom ToLocal(this NodalGlobalDegreeOfFreedom global)
        {
            return new NodalLocalDegreeOfFreedom(global.Node, global.DegreeOfFreedom.ToLocal());
        }
    }
}
