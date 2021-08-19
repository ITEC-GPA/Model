using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    [Serializable]
    public class NormalAreaLoad : Load, IAreaLoad
    {
        // Classe load e derivate deve rimanere immutabile 

        protected readonly double _pressure;
        protected readonly Shape _shape;
        private readonly CoordinateSystem _coordinateSystem;

        #region Properties

        public double Pressure => _pressure;

        public Shape Shape => _shape;

        public CoordinateSystem CoordinateSystem => _coordinateSystem;

        #endregion

        #region Public constructors 
        
        public NormalAreaLoad(double pressure, Shape shape, LoadCaseBase loadCase)
            : base(loadCase, Guid.NewGuid())
        {
            _pressure = pressure;
            _shape = shape ?? throw new ArgumentNullException("Shape cannot be null");
            _coordinateSystem = shape.GetCoordinateSystem();
        }


        public NormalAreaLoad(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _pressure = info.GetDouble("Pressure");
            _shape = (Shape)info.GetValue("Shape", typeof(Shape));
            _coordinateSystem = (CoordinateSystem)info.GetValue("CoordinateSystem", typeof(CoordinateSystem));
        }

        #endregion

        public Shape GetGeometry() => _shape;
        public override GeometryBase GetGeometryBase() => GetGeometry();

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Pressure", _pressure);
            info.AddValue("Shape", _shape);
            info.AddValue("CoordinateSystem", _coordinateSystem);
        }



        #region Equals, HasCode and operators

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            return (obj is NormalAreaLoad objCasted) && _shape.Equals(objCasted._shape) 
                                                     && _pressure.Equals(objCasted._pressure)
                                                     && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _pressure.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<Shape>.Default.GetHashCode(_shape);
                return hashCode;
            }
        }

        public static bool operator ==(NormalAreaLoad obj1, NormalAreaLoad obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(NormalAreaLoad obj1, NormalAreaLoad obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
