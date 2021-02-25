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

        public AreaLoad(double p1, double p2, double p3, Shape shape, LoadCase loadCase, CoordinateSystem coordinateSystem)
            : this(p1, p2, p3, shape, loadCase, coordinateSystem, Guid.NewGuid(), string.Empty)
        {

        }

        public AreaLoad(double p1, double p2, double p3, Shape shape, LoadCase loadCase, CoordinateSystem coordinateSystem, Guid guid, string name)
            : base(loadCase, guid, name)
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

        public override GeometryBase GetGeometry() => _shape;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            throw new NotImplementedException();
        }
    }
}
