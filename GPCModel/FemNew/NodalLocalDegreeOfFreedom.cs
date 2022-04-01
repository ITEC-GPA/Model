using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem;
using GPC.Model.Fem.FemObjects;

namespace GPC.Model.Fem
{

    public struct NodalLocalDegreeOfFreedom : IEquatable<NodalLocalDegreeOfFreedom>, INodalDegreeOfFreedom
    {
        public DegreeOfFreedoms.LocalDegreeOfFreedoms DegreeOfFreedom { get; }

        public Node Node { get; }


        public NodalLocalDegreeOfFreedom(Node node, DegreeOfFreedoms.LocalDegreeOfFreedoms degreeOfFreedom)
        {
            DegreeOfFreedom = degreeOfFreedom;
            Node = node ?? throw new ArgumentNullException(nameof(node));
        }


        #region Equals, Hascode, operators

        public override bool Equals(object obj)
        {
            return obj is NodalLocalDegreeOfFreedom freedom && Equals(freedom);
        }

        public bool Equals(NodalLocalDegreeOfFreedom other)
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

        public static bool operator ==(NodalLocalDegreeOfFreedom left, NodalLocalDegreeOfFreedom right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NodalLocalDegreeOfFreedom left, NodalLocalDegreeOfFreedom right)
        {
            return !(left == right);
        }


        #endregion
    }

    public static class NodalLocalDegreeOfFreedomExtensions
    {
        public static NodalGlobalDegreeOfFreedom[] ToGlobal(this NodalLocalDegreeOfFreedom[] local)
        {
            return local.Select(i => i.ToGlobal()).ToArray();
        }

        public static NodalGlobalDegreeOfFreedom ToGlobal(this NodalLocalDegreeOfFreedom local)
        {
            return new NodalGlobalDegreeOfFreedom(local.Node, local.DegreeOfFreedom.ToGlobal());
        }

    }
}
