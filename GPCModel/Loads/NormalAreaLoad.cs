using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    [Serializable]
    public class NormalAreaLoad : Load, IAreaLoad
    {
        // Classe load e derivate deve rimanere immutabile 

        protected readonly double _pressure;
        protected readonly Shape _shape;

        #region Properties
        public double Pressure => _pressure;

        public Shape Shape => _shape;

        #endregion

        #region Public constructors 
        
        public NormalAreaLoad(double pressure, Shape shape, LoadCaseBase loadCase)
            : base(loadCase, Guid.NewGuid())
        {
            _pressure = pressure;
            _shape = shape ?? throw new ArgumentNullException("Shape cannot be null");
        }


        public NormalAreaLoad(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _pressure = info.GetDouble("Pressure");
            _shape = (Shape)info.GetValue("Shape", typeof(Shape));
        }

        #endregion

        public Shape GetGeometry() => _shape;
        public override GeometryBase GetGeometryBase() => GetGeometry();

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Pressure", _pressure);
            info.AddValue("Shape", _shape);
        }
    }
}
