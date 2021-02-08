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
    public class GlobalAreaLoad : Load, IAreaLoad
    {
        private double _px;
        private double _py;
        private double _pz;

        private Shape _shape;

        public double Px => _px;
        public double Py => _py;
        public double Pz => _pz;

        public Shape Shape => _shape;

        public GlobalAreaLoad(double px, double py, double pz, Shape shape, LoadCase loadCase, Guid guid)
            : base(loadCase, guid)
        {
            this._px = px;
            this._py = py;
            this._pz = pz;

            this._shape = shape ?? throw new ArgumentNullException("Shape cannot be null");
        }

        public GlobalAreaLoad(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {

        }

        public override GeometryBase GetGeometry() => _shape;
    }
}
