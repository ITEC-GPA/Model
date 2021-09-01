using GPC.Geometry;
using System;
using System.Runtime.Serialization;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    
    public class SectionCircular : Section
    {        
        #region Variables

        protected readonly double _dext; /// Diameter external

        #endregion


        #region Properties

        public double Diameter => _dext;

        #endregion


        #region Public Constructors

        public SectionCircular(double dext, Material material, string name) 
            : base(material, name)
        {
            _dext = dext;
            _area = CalculateArea();
        }

        public SectionCircular(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _dext = info.GetDouble("Dext");
            _material = (Material)info.GetValue("Material", typeof(Material));
        }

        #endregion


        #region Public Methods Specific

        public double CalculateWpl()
        {
            return Math.Pow(_dext, 3.0) / 6.0;
        }

        public double CalculateArea()
        {
            return (Math.Pow(_dext, 2.0) * Math.PI) / 4.0;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Dext", _dext);
            info.AddValue("Material", _material);
        }

        public override ShapeMaterial[] GetShapes()
        {
            int divisions = 36;
            Polygon2d hole =null;
            Polygon2d fill = new Polygon2d();
    
            for (int i = 0; i < divisions; i++)
            {
                double teta = i * 2 * Math.PI / divisions;
                fill.Add(new Point2d(0.5 * _dext * Math.Cos(teta), 0.5 * _dext * Math.Sin(teta)));
            }

            Shape shape = new Shape(fill, hole != null ? new[] { hole } : null);

            return new[] { new ShapeMaterial { Material = _material, Shape = shape } };
        }

        #endregion
        
    }    
}