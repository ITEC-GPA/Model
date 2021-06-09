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

        protected readonly Vector3d _gravityVector;


        /// <summary>
        /// Get the unitized gravity vector
        /// </summary>
        public Vector3d GravityVector => _gravityVector;

        /// <summary>
        /// Get the gravity acceleration value
        /// </summary>
        public double Acceleration => _acceleration;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="loadCase"></param>
        /// <param name="gravityVector"></param>
        /// <param name="acceleration"></param>
        /// <remarks>The <see cref="LoadCase.LoadCaseType"/> of <paramref name="loadCase"/> must be <see cref="LoadCase.LoadCaseTypes.SelfWeight"/></remarks>
        public SelfWeightLoad(LoadCase loadCase, Vector3d gravityVector, double acceleration) 
            : base(loadCase)
        {
            if (loadCase.LoadCaseType != LoadCases.LoadCase.LoadCaseTypes.SelfWeight)
            {
                throw new ArgumentException($"LoadCaseType must be {LoadCases.LoadCase.LoadCaseTypes.SelfWeight}");
            }

            _gravityVector = gravityVector;
            _gravityVector.Unitize();
            _acceleration = acceleration;
        }


        public override GeometryBase GetGeometryBase()
        {
            return null;
        }



        #region Equals, HashCode and operators

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            if (obj is null)
                return false;

            var objCasted = obj as SelfWeightLoad;

            return objCasted != null && _acceleration.Equals(objCasted._acceleration) && _gravityVector.Equals(objCasted._gravityVector) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _acceleration.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<Vector3d>.Default.GetHashCode(_gravityVector);
            return hashCode;
        }

        public static bool operator ==(SelfWeightLoad obj1, SelfWeightLoad obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }


        public static bool operator !=(SelfWeightLoad obj1, SelfWeightLoad obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
