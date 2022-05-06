using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Fem.Attributes
{
    [Serializable]
    public class NodeForceAttribute : LoadCaseAttribute, INodeLoadCaseAttribute, IEquatable<NodeForceAttribute>, ISerializable
    {
        protected readonly double _fx;
        protected readonly double _fy;
        protected readonly double _fz;


        public NodeForceAttribute(string loadCaseName, double fx, double fy, double fz) 
            : base(loadCaseName)
        {
            _fx = fx;
            _fy = fy;
            _fz = fz;
        }

        protected NodeForceAttribute(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _fx = info.GetDouble("Fx");
            _fy = info.GetDouble("Fy");
            _fz = info.GetDouble("Fz");
        }


        protected NodeForceAttribute(NodeForceAttribute nodeForceAttribute)
            : base(nodeForceAttribute.LoadCaseName)
        {
            _fx = nodeForceAttribute._fx;
            _fy = nodeForceAttribute._fy;
            _fz = nodeForceAttribute._fz;
        }

        public override object Clone()
        {
            return new NodeForceAttribute(this);
        }

        public bool Equals(NodeForceAttribute other)
        {
            if (other is null)
                return false;

            return _fx == other._fx &&
                   _fy == other._fy &&
                   _fz == other._fz && base.Equals(other);
        }


        public override bool Equals(object obj)
        {
            return Equals((PlateNormalPressureAttribute)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -17;
                hashCode = hashCode * -23 + base.GetHashCode();
                hashCode = hashCode * -23 + _fx.GetHashCode();
                hashCode = hashCode * -23 + _fy.GetHashCode();
                hashCode = hashCode * -23 + _fz.GetHashCode();
                return hashCode; 
            }
        }

        #region Override Operator

        public static bool operator ==(NodeForceAttribute obj1, NodeForceAttribute obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            return obj1.Equals(obj2);
        }

        public static bool operator !=(NodeForceAttribute obj1, NodeForceAttribute obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
