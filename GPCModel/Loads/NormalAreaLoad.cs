using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    [Serializable]
    public class NormalAreaLoad : Load, IAreaLoad
    {
        private double _pressure;

        private Shape _shape;

        #region Properties
        public double Pressure => _pressure;

        public Shape Shape => _shape;

        #endregion

        #region Public constructors 
        
        public NormalAreaLoad(double pressure, Shape shape, LoadCase loadCase)
            : this(pressure, shape, loadCase, Guid.NewGuid(), string.Empty)
        {

        }

        public NormalAreaLoad(double pressure, Shape shape, LoadCase loadCase, Guid guid, string name)
            : base(loadCase, guid, name)
        {
            this._pressure = pressure;
            this._shape = shape ?? throw new ArgumentNullException("Shape cannot be null");
        }

        public NormalAreaLoad(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _pressure = info.GetDouble("Pressure");
            _shape = (Shape)info.GetValue("Shape", typeof(Shape));
        }

        #endregion

        public Shape GetGeometry() => _shape;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Pressure", _pressure);
            info.AddValue("Shape", _shape);
        }
    }
}