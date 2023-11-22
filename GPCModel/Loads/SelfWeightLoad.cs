using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    /// <summary>
    /// This class rapresent a self weight load of an object
    /// </summary>
    public class SelfWeightLoad : Load
    {
        #region Variables

        protected double _acceleration;
        protected Vector3d _gravityAxis;

        #endregion

        #region Properties

        /// <summary>
        /// Get the unitized gravity vector
        /// </summary>
        public Vector3d GravityAxis { get => _gravityAxis; set => _gravityAxis = value; }

        /// <summary>
        /// Get the gravity acceleration value
        /// </summary>
        public double Acceleration { get => _acceleration; set => _acceleration = value; }

        /// <summary>
        /// Get the gravity vector, i.e the gravity axis * acceleration 
        /// </summary>
        public Vector3d GravityVector => _gravityAxis * _acceleration;

        #endregion

        #region Constructor

        /// <param name="loadCase"></param>
        /// <param name="acceleration">A positive acceleration means acceleration along the positive axis</param>
        /// <remarks>The <see cref="LoadCase.LoadCaseType"/> of <paramref name="loadCase"/> must be <see cref="LoadCase.LoadCaseTypes.SelfWeight"/>. Default vector is global.Z</remarks>
        public SelfWeightLoad(LoadCase loadCase, double acceleration)
            : base(loadCase)
        {
            if (loadCase.LoadCaseType != LoadCases.LoadCase.LoadCaseTypes.SelfWeight)
                throw new ArgumentException($"LoadCaseType must be {LoadCases.LoadCase.LoadCaseTypes.SelfWeight}");

            _gravityAxis = CoordinateSystem.Global.V3; // di default è Z
            _acceleration = acceleration;
        }

        protected SelfWeightLoad(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _acceleration = info.GetDouble("Acceleration");
            _gravityAxis = (Vector3d)info.GetValue("GravityAxis", typeof(Vector3d));
        }

        #endregion

        #region Equals, HashCode and operators

        public override GeometryBase GetGeometryBase()
        {
            return null;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("GravityAxis", _gravityAxis);
            info.AddValue("Acceleration", _acceleration);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            return (obj is SelfWeightLoad objCasted) &&
                _acceleration.Equals(objCasted._acceleration) &&
                _gravityAxis.Equals(objCasted._gravityAxis) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _acceleration.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<Vector3d>.Default.GetHashCode(_gravityAxis);
                return hashCode;
            }
        }

        public static bool operator ==(SelfWeightLoad obj1, SelfWeightLoad obj2)
        {
            if (obj1 is null)
                return obj2 is null;

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(SelfWeightLoad obj1, SelfWeightLoad obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
