using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Loads
{
    /// <summary>
    /// This class rapresent a self weight load of an object
    /// </summary>
    public class SelfWeightLoad : Load
    {
        // Classe load e derivate deve rimanere immutabile 

        protected readonly double _acceleration;

        protected Vector3d _gravityVector;


        /// <summary>
        /// Get the unitized gravity vector
        /// </summary>
        public Vector3d GravityVector => _gravityVector;

        /// <summary>
        /// Get the gravity acceleration value
        /// </summary>
        public double Acceleration => _acceleration;


        /// <param name="loadCase"></param>
        /// <param name="acceleration">A positive acceleration means acceleration along the positive axis</param>
        /// <remarks>The <see cref="LoadCase.LoadCaseType"/> of <paramref name="loadCase"/> must be <see cref="LoadCase.LoadCaseTypes.SelfWeight"/>. Default vector is global.Z</remarks>
        public SelfWeightLoad(LoadCase loadCase, double acceleration) 
            : base(loadCase)
        {
            if (loadCase.LoadCaseType != LoadCases.LoadCase.LoadCaseTypes.SelfWeight)
            {
                throw new ArgumentException($"LoadCaseType must be {LoadCases.LoadCase.LoadCaseTypes.SelfWeight}");
            }

            _gravityVector = CoordinateSystem.Global.V3; // di default è Z
            
            _acceleration = acceleration;
        }


        public override GeometryBase GetGeometryBase()
        {
            return null;
        }

        public void SetGravityAxisToX()
        {
            _gravityVector = CoordinateSystem.Global.V1;
        }

        public void SetGravityAxisToY()
        {
            _gravityVector = CoordinateSystem.Global.V2;
        }
        public void SetGravityAxisToZ()
        {
            _gravityVector = CoordinateSystem.Global.V3;
        }

        #region Equals, HashCode and operators

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            return (obj is SelfWeightLoad objCasted) && _acceleration.Equals(objCasted._acceleration)
                                                     && _gravityVector.Equals(objCasted._gravityVector) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _acceleration.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<Vector3d>.Default.GetHashCode(_gravityVector);
                return hashCode; 
            }
        }

        public static bool operator ==(SelfWeightLoad obj1, SelfWeightLoad obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

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
