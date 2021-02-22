
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.LoadCases;
using GPC.Geometry;

namespace GPC.Model.FEM.Attributes
{
    public class PlatePressureAttribute : LoadCaseAttribute, IPlateLoadCaseAttribute, IEquatable<PlatePressureAttribute>, ISerializable
    {
        #region variables
        private double _p11;
                        
        private double _p22;
                        
        private double _p33;

        private CoordinateSystem _sys;
        #endregion

        #region properties
        public double P11 => _p11;
                          
        public double P22 => _p22;
                          
        public double P33 => _p33;
        public CoordinateSystem Sys => _sys;
        #endregion


        public PlatePressureAttribute(LoadCase loadCase, CoordinateSystem sys, double p11, double p22, double p33) 
            : this(loadCase, sys, p11, p22, p33, string.Empty, Guid.NewGuid())
        {

        }

        public PlatePressureAttribute(LoadCase loadCase, CoordinateSystem sys, double p11, double p22, double p33, string name)
            : this(loadCase, sys, p11, p22, p33, name, Guid.NewGuid())
        {

        }

        public PlatePressureAttribute(LoadCase loadCase, CoordinateSystem sys, Vector3d p)
            : this(loadCase, sys, p.X, p.Y, p.Z, string.Empty, Guid.NewGuid())
        {

        }

        public PlatePressureAttribute(LoadCase loadCase, CoordinateSystem sys, Vector3d p, string name)
            : this(loadCase, sys, p.X, p.Y, p.Z, name, Guid.NewGuid())
        {

        }

        public PlatePressureAttribute(LoadCase loadCase, CoordinateSystem sys, double p11, double p22, double p33, string name, Guid guid)
            : base(loadCase, name, guid)
        {
            _p11 = p11;
            _p22 = p22;
            _p33 = p33;
            _sys = sys;
        }

        public PlatePressureAttribute(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _p11 = info.GetDouble("p11");
            _p22 = info.GetDouble("p22");
            _p33 = info.GetDouble("p33");
            _sys = (CoordinateSystem) info.GetValue("sys", typeof(CoordinateSystem));
        }


        public bool Equals(PlatePressureAttribute other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && _p11.Equals(other._p11)
                                    && _p22.Equals(other._p22)
                                    && _p33.Equals(other._p33)
                                    && _sys.Equals(other._sys)
                                    && base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as PlatePressureAttribute);
        }


        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _p11.GetHashCode();
            hashCode = hashCode * -17 + _p22.GetHashCode();
            hashCode = hashCode * -17 + _p33.GetHashCode();
            hashCode = hashCode * -17 + _sys.GetHashCode();
            return hashCode;
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("p11", _p11);
            info.AddValue("p22", _p22);
            info.AddValue("p33", _p33);
            info.AddValue("csy", _sys);
        }


        #region Override Operator

        public static bool operator ==(PlatePressureAttribute obj1, PlatePressureAttribute obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(PlatePressureAttribute obj1, PlatePressureAttribute obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
