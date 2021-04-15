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
        private double _p1;
        private double _p2;
        private double _p3;

        private Shape _shape;

        private CoordinateSystem _coordinateSystem;

        public double P1 => _p1;
        public double P2 => _p2;
        public double P3 => _p3;

        public Shape Shape => _shape;
        public CoordinateSystem CoordinateSystem => _coordinateSystem;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="shape"></param>
        /// <param name="loadCase"></param>
        /// <remarks><see cref="CoordinateSystem"/> set to Global</remarks>
        public AreaLoad(double p1, double p2, double p3, Shape shape, LoadCase loadCase)
            : this(p1, p2, p3, shape, loadCase, CoordinateSystem.Global)
        {

        }

        public AreaLoad(double p1, double p2, double p3, Shape shape, LoadCase loadCase, CoordinateSystem coordinateSystem)
            : base(loadCase, Guid.NewGuid())
        {

            this._p1 = p1;
            this._p2 = p2;
            this._p3 = p3;

            this._shape = shape ?? throw new ArgumentNullException("Shape cannot be null");
            this._coordinateSystem = coordinateSystem ?? throw new ArgumentNullException(nameof(coordinateSystem));
        }


        public AreaLoad(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        public Shape GetGeometry() => _shape;
        public override GeometryBase GetGeometryBase() => GetGeometry();

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

            if (obj is null)
                return false;

            var objCasted = obj as AreaLoad;

            return objCasted != null && _shape.Equals(objCasted._shape) && _coordinateSystem.Equals(objCasted._coordinateSystem)
                                                                        && _p1.Equals(objCasted._p1) && _p2.Equals(objCasted._p2) && _p3.Equals(objCasted._p3) && base.Equals(objCasted);
        }

        public override int GetHashCode()
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

        public static bool operator ==(AreaLoad obj1, AreaLoad obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(AreaLoad obj1, AreaLoad obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
