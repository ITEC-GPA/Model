
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
        private double _p1;
                        
        private double _p2;
                        
        private double _p3;

        private CoordinateSystem _coordinateSystem;
        #endregion

        #region properties
        public double P1 => _p1;
                          
        public double P2 => _p2;
                          
        public double P3 => _p3;
        public CoordinateSystem CoordinateSystem => _coordinateSystem;

        #endregion


        public PlatePressureAttribute(LoadCase loadCase, CoordinateSystem coordinateSystem, double p11, double p22, double p33) 
            : this(loadCase, coordinateSystem, p11, p22, p33, string.Empty, Guid.NewGuid())
        {

        }

        public PlatePressureAttribute(LoadCase loadCase, CoordinateSystem coordinateSystem, double p11, double p22, double p33, string name)
            : this(loadCase, coordinateSystem, p11, p22, p33, name, Guid.NewGuid())
        {

        }

        public PlatePressureAttribute(LoadCase loadCase, CoordinateSystem coordinateSystem, Vector3d p)
            : this(loadCase, coordinateSystem, p.X, p.Y, p.Z, string.Empty, Guid.NewGuid())
        {

        }

        public PlatePressureAttribute(LoadCase loadCase, CoordinateSystem coordinateSystem, Vector3d p, string name)
            : this(loadCase, coordinateSystem, p.X, p.Y, p.Z, name, Guid.NewGuid())
        {

        }

        public PlatePressureAttribute(LoadCase loadCase, CoordinateSystem sys, double p1, double p2, double p3, string name, Guid guid)
            : base(loadCase, name, guid)
        {
            _p1 = p1;
            _p2 = p2;
            _p3 = p3;
            _coordinateSystem = sys;
        }


        public PlatePressureAttribute(PlatePressureAttribute platePressureAttribute)
            : base(platePressureAttribute)
        {
            _p1 = platePressureAttribute._p1;
            _p2 = platePressureAttribute._p2;
            _p3 = platePressureAttribute._p3;
            _coordinateSystem = platePressureAttribute.CoordinateSystem;
        }


        public PlatePressureAttribute(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _p1 = info.GetDouble("p1");
            _p2 = info.GetDouble("p2");
            _p3 = info.GetDouble("p3");
            _coordinateSystem = (CoordinateSystem) info.GetValue("sys", typeof(CoordinateSystem));
        }


        public bool Equals(PlatePressureAttribute other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && _p1.Equals(other._p1)
                                    && _p2.Equals(other._p2)
                                    && _p3.Equals(other._p3)
                                    && _coordinateSystem.Equals(other._coordinateSystem)
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
            hashCode = hashCode * -17 + _p1.GetHashCode();
            hashCode = hashCode * -17 + _p2.GetHashCode();
            hashCode = hashCode * -17 + _p3.GetHashCode();
            hashCode = hashCode * -17 + _coordinateSystem.GetHashCode();
            return hashCode;
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("p1", _p1);
            info.AddValue("p2", _p2);
            info.AddValue("p3", _p3);
            info.AddValue("CoordinateSystem", _coordinateSystem);
        }

        public override object Clone()
        {
            return new PlatePressureAttribute(this);
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
