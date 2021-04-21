
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
    public class BeamDistribuitedLoadAttribute : LoadCaseAttribute, IBeamLoadCaseAttribute, IEquatable<BeamDistribuitedLoadAttribute>, ISerializable
    {
        #region variables
        private double _q1;
        private double _q2;
        private double _q3;

        private CoordinateSystem _coordinateSystem; //if null the values are take like the local system of the beam
        #endregion

        #region properties
        public double Q1 => _q1;
                          
        public double Q2 => _q2;
                         
        public double Q3 => _q3;
        public CoordinateSystem CoordinateSystem => _coordinateSystem;

        #endregion


        public BeamDistribuitedLoadAttribute(LoadCase loadCase, double q1, double q2, double q3, CoordinateSystem coordinateSystem = null) 
            : this(loadCase, q1, q2, q3, string.Empty, Guid.NewGuid(), coordinateSystem)
        {

        }

        public BeamDistribuitedLoadAttribute(LoadCase loadCase, double q1, double q2, double q3, string name, CoordinateSystem coordinateSystem = null)
            : this(loadCase, q1, q2, q3, name, Guid.NewGuid(), coordinateSystem)
        {

        }

        public BeamDistribuitedLoadAttribute(LoadCase loadCase, Vector3d q, CoordinateSystem coordinateSystem = null)
            : this(loadCase, q.X, q.Y, q.Z, string.Empty, Guid.NewGuid(), coordinateSystem)
        {

        }

        public BeamDistribuitedLoadAttribute(LoadCase loadCase, Vector3d q, string name, CoordinateSystem coordinateSystem = null)
            : this(loadCase, q.X, q.Y, q.Z, name, Guid.NewGuid(), coordinateSystem)
        {

        }

        public BeamDistribuitedLoadAttribute(LoadCase loadCase, double q1, double q2, double q3, string name, Guid guid, CoordinateSystem sys = null)
            : base(loadCase, name, guid)
        {
            _q1 = q1;
            _q2 = q2;
            _q3 = q3;
            _coordinateSystem = sys;
        }


        public BeamDistribuitedLoadAttribute(BeamDistribuitedLoadAttribute platePressureAttribute)
            : base(platePressureAttribute)
        {
            _q1 = platePressureAttribute._q1;
            _q2 = platePressureAttribute._q2;
            _q3 = platePressureAttribute._q3;
            _coordinateSystem = platePressureAttribute.CoordinateSystem;
        }


        public BeamDistribuitedLoadAttribute(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _q1 = info.GetDouble("q1");
            _q2 = info.GetDouble("q2");
            _q3 = info.GetDouble("q3");
            _coordinateSystem = (CoordinateSystem) info.GetValue("sys", typeof(CoordinateSystem));
        }


        public bool Equals(BeamDistribuitedLoadAttribute other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && _q1.Equals(other._q1)
                                    && _q2.Equals(other._q2)
                                    && _q3.Equals(other._q3)
                                    && _coordinateSystem.Equals(other._coordinateSystem)
                                    && base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as BeamDistribuitedLoadAttribute);
        }

        public override int GetHashCode()
        {
            var hashCode = 1997487538;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + _q1.GetHashCode();
            hashCode = hashCode * -1521134295 + _q2.GetHashCode();
            hashCode = hashCode * -1521134295 + _q3.GetHashCode();
            hashCode = hashCode * -1521134295 + _coordinateSystem.GetHashCode();
            return hashCode;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("q1", _q1);
            info.AddValue("q2", _q2);
            info.AddValue("q3", _q3);
            info.AddValue("CoordinateSystem", _coordinateSystem);
        }

        public override object Clone()
        {
            return new BeamDistribuitedLoadAttribute(this);
        }

        #region Override Operator

        public static bool operator ==(BeamDistribuitedLoadAttribute obj1, BeamDistribuitedLoadAttribute obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(BeamDistribuitedLoadAttribute obj1, BeamDistribuitedLoadAttribute obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
