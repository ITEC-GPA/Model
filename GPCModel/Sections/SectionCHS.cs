using System;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    [Serializable]
    public class SectionCHS : Section, ISection, ISerializable
    {
        #region Variables

        protected readonly double _externalDiameter; // Diameter external
        protected readonly double _thickness; // Thickness

        #endregion

        #region Properties

        /// <summary>
        /// The external diameter of CHS
        /// </summary>
        public double Diameter => _externalDiameter;

        /// <summary>
        /// The Thickness of the section
        /// </summary>
        public double Thickness => _thickness;

        /// <summary>
        /// The internal diameter of CHS
        /// </summary>
        public double DiameterInternal => _externalDiameter - (2 * _thickness);

        #endregion

        #region Public Constructors

        public SectionCHS(double externalDiameter, double thickness, Material material, string name)
            : base(material, name)
        {

            if (thickness > externalDiameter / 2.0)
                throw new ArgumentException($"Diameter cannot be lower than 2 * thickness ");

            _externalDiameter = externalDiameter < 0 ? throw new ArgumentException($"Diameter cannot be lower than zero") : externalDiameter;
            _thickness = thickness < 0 ? throw new ArgumentException($"Thickness cannot be lower than zero") : thickness;

            SetMechanicalProperties();
            _mesh = GetMesh();
        }

        public SectionCHS(SectionCHS section)
            : this(section.Diameter, (section.Diameter - section.DiameterInternal) / 2.0, section.Material, section.Name)
        {

        }

        protected SectionCHS(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _externalDiameter = info.GetDouble("D");
            _thickness = info.GetDouble("T");
        }

        #endregion

        #region Public method

        protected override void SetMechanicalProperties()
        {
            _area = CalculateArea();
            _j11 = CalculateJ();
            _j22 = CalculateJ();
            _jxx = CalculateJ();
            _jyy = CalculateJ();

            _jxy = CalculateJxy();
            _jp = _jxx + _jyy;

            _jt = CalculateJt();
            _jw = CalculateJw();
            _centroid = CalculateCentroid();
            _shearCenter = _centroid;
            _angleX1 = CalculateAngle();
            _wel1Max = CalculateWel();
            _wel1Min = CalculateWel();
            _wel2Max = CalculateWel();
            _wel2Min = CalculateWel();
            _wpl1 = CalculateWpl();
            _wpl2 = CalculateWpl();

            _isSymmetricAlongXLocalAxis = CalculateIsSymmetricAlongXLocalAxis();
            _isSymmetricAlongYLocalAxis = CalculateIsSymmetricAlongYLocalAxis();

        }

        protected override double CalculateArea()
        {
            return (Math.Pow(Diameter, 2.0) * Math.PI) / 4.0 - (Math.Pow(DiameterInternal, 2.0) * Math.PI) / 4.0;
        }

        protected virtual double CalculateJ()
        {
            return Math.PI * (Math.Pow(Diameter, 4.0) - Math.Pow(DiameterInternal, 4.0)) / (64.0);
        }

        protected override double CalculateJxy()
        {
            return 0;
        }

        protected override double CalculateJt()
        {
            return Math.PI * (Math.Pow(Diameter, 4.0) - Math.Pow(DiameterInternal, 4.0)) / (32.0);
        }

        protected override double CalculateJw()
        {
            return 0;
        }

        protected override Point2d CalculateCentroid()
        {
            return new Point2d(Diameter / 2.0, Diameter / 2.0);
        }

        protected virtual double CalculateWel()
        {
            return Math.PI * (Math.Pow(Diameter, 4.0) - Math.Pow(DiameterInternal, 4.0)) / (32.0 * _externalDiameter);
        }

        protected virtual double CalculateWpl()
        {
            return (Math.Pow(Diameter, 3.0) - Math.Pow(DiameterInternal, 3.0)) / (6.0);
        }

        protected override bool CalculateIsSymmetricAlongXLocalAxis()
        {
            return true;
        }

        protected override bool CalculateIsSymmetricAlongYLocalAxis()
        {
            return true;
        }

        #endregion

        #region Public override methods 

        protected Shape2d GetShape(int numberOfEdges)
        {
            return new Shape2d(new Polygon2d(_externalDiameter, numberOfEdges), new[] { new Polygon2d(_externalDiameter - _thickness, numberOfEdges) });
        }

        protected override Shape2d GetShape()
        {
            return new Shape2d(new Polygon2d(_externalDiameter), new[] { new Polygon2d(_externalDiameter - _thickness) });
        }

        protected Mesh GetMesh(int numberOfEdges = 16)
		{
            Shape2d shape = GetShape(numberOfEdges);

            Mesh mesh = new Mesh();

            for(int i = 0; i < shape.Fill.Count; i++)
			{
                mesh.AddFaceMesh(new Point3d[]
                {
                    new Point3d(shape.Fill[i]),
                    new Point3d(shape.Fill[shape.Fill.GetNextIndex(i)]),
                    new Point3d(shape.Holes[0][shape.Holes[0].GetNextIndex(i)]),
                    new Point3d(shape.Holes[0][i]),
                });
			}

            return mesh;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("D", _externalDiameter);
            info.AddValue("T", _thickness);
        }


        public override string ToString()
        {
            return $"CHS {_externalDiameter}x{_thickness}";
        }

        #endregion
    }
}