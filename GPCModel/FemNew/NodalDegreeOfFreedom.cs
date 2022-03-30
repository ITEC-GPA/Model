using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem.FemObjects;

namespace GPC.Model.Fem
{
    public  struct NodalDegreeOfFreedom : IEquatable<NodalDegreeOfFreedom>
    {
        public DegreeOfFreedom DegreeOfFreedom { get; }

        public Node Node { get; }


        public NodalDegreeOfFreedom(Node node, DegreeOfFreedom degreeOfFreedom)
        {
            DegreeOfFreedom = degreeOfFreedom;
            Node = node ?? throw new ArgumentNullException(nameof(node));
        }



        #region Equals, Hascode, operators

        public override bool Equals(object obj)
        {
            return obj is NodalDegreeOfFreedom freedom && Equals(freedom);
        }

        public bool Equals(NodalDegreeOfFreedom other)
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

        public static bool operator ==(NodalDegreeOfFreedom left, NodalDegreeOfFreedom right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NodalDegreeOfFreedom left, NodalDegreeOfFreedom right)
        {
            return !(left == right);
        } 

        #endregion


    }
}
