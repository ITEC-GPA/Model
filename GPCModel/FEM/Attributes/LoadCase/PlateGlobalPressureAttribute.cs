
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
    public class PlateGlobalPressureAttribute : LoadCaseAttribute, IPlateLoadCaseAttribute, IEquatable<PlateGlobalPressureAttribute>, ISerializable
    {

        private double _fX;
                        
        private double _fY;
                        
        private double _fZ;


        public double Fx => _fX;
                          
        public double Fy => _fY;
                          
        public double Fz => _fZ;


        public PlateGlobalPressureAttribute(LoadCase loadCase, double fx, double fy, double fz) 
            : this(loadCase, fx, fy, fz, string.Empty, Guid.NewGuid())
        {

        }

        public PlateGlobalPressureAttribute(LoadCase loadCase, double fx, double fy, double fz, string name)
            : this(loadCase, fx, fy, fz, name, Guid.NewGuid())
        {

        }

        public PlateGlobalPressureAttribute(LoadCase loadCase, Vector3d force)
            : this(loadCase, force.X, force.Y, force.Z, string.Empty, Guid.NewGuid())
        {

        }

        public PlateGlobalPressureAttribute(LoadCase loadCase, Vector3d force, string name)
            : this(loadCase, force.X, force.Y, force.Z, name, Guid.NewGuid())
        {

        }

        public PlateGlobalPressureAttribute(LoadCase loadCase, double fx, double fy, double fz, string name, Guid guid)
            : base(loadCase, name, guid)
        {
            _fX = fx;
            _fY = fy;
            _fZ = fz;
        }

        public PlateGlobalPressureAttribute(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _fX = info.GetDouble("fx");
            _fY = info.GetDouble("fy");
            _fZ = info.GetDouble("fz");
        }


        public bool Equals(PlateGlobalPressureAttribute other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && _fX.Equals(other._fX)
                                    && _fY.Equals(other._fY)
                                    && _fZ.Equals(other._fZ)
                                    && base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as PlateGlobalPressureAttribute);
        }


        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _fX.GetHashCode();
            hashCode = hashCode * -17 + _fY.GetHashCode();
            hashCode = hashCode * -17 + _fZ.GetHashCode();
            return hashCode;
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("fx", _fX);
            info.AddValue("fy", _fY);
            info.AddValue("fz", _fZ);
        }


        #region Override Operator

        public static bool operator ==(PlateGlobalPressureAttribute obj1, PlateGlobalPressureAttribute obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(PlateGlobalPressureAttribute obj1, PlateGlobalPressureAttribute obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
