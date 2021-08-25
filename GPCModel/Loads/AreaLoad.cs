using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Loads
{
    [Serializable]
    public class AreaLoad : Load, IAreaLoad
    {
        private readonly double _p1;
        private readonly double _p2;
        private readonly double _p3;

        private readonly Shape _shape;

        private readonly CoordinateSystem _coordinateSystem;

        public double P1 => _p1;
        public double P2 => _p2;
        public double P3 => _p3;

        /// <summary>
        /// In the global reference system
        /// </summary>
        public Shape Shape => _shape;

        /// <summary>
        /// reference system of the load
        /// </summary>
        public CoordinateSystem CoordinateSystem => _coordinateSystem;


        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="shape">In the global reference system</param>
        /// <param name="loadCase"></param>
        /// <param name="coordinateSystem">Reference system of the load</param>
        /// <remarks><see cref="CoordinateSystem"/> set to <see cref="Shape.GetCoordinateSystem()"/></remarks>
        public AreaLoad(double p1, double p2, double p3, Shape shape, LoadCaseBase loadCase, CoordinateSystem coordinateSystem)
            : base(loadCase, Guid.NewGuid())
        {
            _p1 = p1;
            _p2 = p2;
            _p3 = p3;

            _shape = shape ?? throw new ArgumentNullException("Shape cannot be null");
            _coordinateSystem = coordinateSystem;
        }

        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="shape">In the global reference system</param>
        /// <param name="loadCase"></param>
        /// <remarks><see cref="CoordinateSystem"/> set to <see cref="Shape.GetCoordinateSystem()"/></remarks>
        public AreaLoad(double p1, double p2, double p3, Shape shape, LoadCaseBase loadCase)
            : this(p1, p2, p3, shape, loadCase, CoordinateSystem.Global)
        {

        }


        public AreaLoad(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        public Shape GetGeometry() => _shape;
        public override GeometryBase GetGeometryBase() => GetGeometry();


        /// <summary>
        /// Convert this load into a normal area loads.
        /// </summary>
        /// <remarks>The not normal portion will be lost</remarks>
        public NormalAreaLoad ConvertToNormalAreaLoad()
        {
            var globalLoad = this.GetGlobalLoadVector();

            return new NormalAreaLoad(_shape.GetCoordinateSystem().ToLocal(globalLoad).Z, _shape, LoadCase);
        }


        /// <returns>The total load vector in the local system. i.e. _p1 * area, _p2 * area, _p3 * area</returns>
        public Vector3d GetLocalLoadVector()
        {
            double area = _shape.GetArea();
            return new Vector3d(_p1 * area, _p2 * area, _p3 * area);
        }

        /// <returns>The total global load vector in the local system. i.e. _p1 * area, _p2 * area, _p3 * area</returns>
        public Vector3d GetGlobalLoadVector()
        {
            if (_coordinateSystem == CoordinateSystem.Global)
                return GetLocalLoadVector();
            else
            {
                return _coordinateSystem.ToGlobal(GetLocalLoadVector());
            }
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            throw new NotImplementedException();
        }

        #region Equals, HasCode and operators

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            return (obj is AreaLoad objCasted) && _shape.Equals(objCasted._shape) && _coordinateSystem.Equals(objCasted._coordinateSystem)
                                               && _p1.Equals(objCasted._p1) && _p2.Equals(objCasted._p2) && _p3.Equals(objCasted._p3) 
                                               && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _p1.GetHashCode();
                hashCode = hashCode * -17 + _p2.GetHashCode();
                hashCode = hashCode * -17 + _p3.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<CoordinateSystem>.Default.GetHashCode(_coordinateSystem);
                hashCode = hashCode * -17 + EqualityComparer<Shape>.Default.GetHashCode(_shape);
                return hashCode; 
            }
        }

        public static bool operator ==(AreaLoad obj1, AreaLoad obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(AreaLoad obj1, AreaLoad obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
