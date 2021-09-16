using GPC.Geometry;
using System;
using System.Runtime.Serialization;
using GPC.Model.Materials;
using System.Linq;

namespace GPC.Model.Sections
{
    
    public class SectionCircular : Section
    {        
        #region Variables

        protected double _diameter;

        #endregion


        #region Properties

        /// <summary>
        /// The external diameter
        /// </summary>
        public double Diameter => _diameter;

        #endregion


        #region Public Constructors

        /// <summary>
        /// The default constructor
        /// </summary>
        /// <param name="diameter">The diameter</param>
        /// <param name="material">The material</param>
        /// <param name="name">The section name</param>
        public SectionCircular(double diameter, Material material, string name) 
            : base(material, name)
        {
            _diameter = diameter;
            _area = CalculateArea();
        }

        protected SectionCircular(double diameter, Material material, string name, int id)
            : base(material, name)
        {
            _diameter = diameter;
            _area = CalculateArea();
            _id = id;
        }

        public SectionCircular(SectionCircular sectionCircular)
            : this(sectionCircular.Diameter, sectionCircular.Material, sectionCircular.Name, sectionCircular.Id)
        {

        }

        public SectionCircular(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _diameter = info.GetDouble("Diameter");
            _material = (Material)info.GetValue("Material", typeof(Material));
        }

        #endregion


        #region Public Methods Specific

        public double CalculateArea()
        {
            return (Math.Pow(_diameter, 2.0) * Math.PI) / 4.0;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Diameter", _diameter);
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
                fill.Add(new Point2d(0.5 * _diameter * Math.Cos(teta), 0.5 * _diameter * Math.Sin(teta)));
            }

            Shape shape = new Shape(fill, hole != null ? new[] { hole } : null);

            return new[] { new ShapeMaterial { Material = _material, Shape = shape } };
        }

        private Polygon3d ConvertCircleToPolygon(double radius, int edge)
        {
            if (edge < 2)
                throw new ArgumentException($"{edge} must be at least 3");

            Point3d[] vertices = new Point3d[edge];
            double teta = 2.0 * Math.PI / edge;

            for (int i = 0; i < edge; i++)
            {
                vertices[i] = new Point3d(radius * Math.Cos(teta * i), radius * Math.Sin(teta * i), 0.0);
            }

            return new Polygon3d(vertices.ToArray());
        }

        public Polygon3d ConvertCircleToPolygon(int edge = 32)
        {
            return ConvertCircleToPolygon(_diameter / 2.0, edge);
        }

        #endregion

    }    
}