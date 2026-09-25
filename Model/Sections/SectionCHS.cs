using GPC.Geometry;
using GPC.Geometry.Meshes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A circular hollow section (CHS), with the centre at (D / 2, D / 2); the thin walls are the sides of a polygon of 32 edges on the middle
    /// line, the properties are the exact ones of the annulus
    /// </summary>
    [Serializable]
    public class SectionCHS : ThinWallSection, ISerializable, IEquatable<SectionCHS>
    {
        #region Variables

        /// <summary>
        /// The external diameter
        /// </summary>
        protected double _externalDiameter;
        /// <summary>
        /// The thickness
        /// </summary>
        protected double _thickness;

        #endregion

        #region Properties

        /// <summary>
        /// The external diameter of CHS
        /// </summary>
        public double Diameter
        {
            get => _externalDiameter;
            set
            {
                if (_externalDiameter != value)
                {
                    _externalDiameter = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The Thickness of the section
        /// </summary>
        public double Thickness
        {
            get => _thickness;
            set
            {
                if (_thickness != value)
                {
                    _thickness = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The internal diameter of CHS
        /// </summary>
        public double DiameterInternal => _externalDiameter - (2 * _thickness);

        /// <summary>
        /// The height: the external diameter
        /// </summary>
        public override double Height
        {
            get => Diameter;
            set => Diameter = value;
        }

        /// <summary>
        /// The width: the external diameter
        /// </summary>
        public override double Width => _externalDiameter;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="externalDiameter">The external diameter</param>
        /// <param name="thickness">The thickness</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentException">If a dimension is negative or the thickness is bigger than the radius</exception>
        public SectionCHS(double externalDiameter, double thickness, string name = "")
            : base(name)
        {
            if (thickness > externalDiameter / 2.0)
                throw new ArgumentException($"Diameter cannot be lower than 2 * thickness ");

            _externalDiameter = externalDiameter < 0 ? throw new ArgumentException($"Diameter cannot be lower than zero") : externalDiameter;
            _thickness = thickness < 0 ? throw new ArgumentException($"Thickness cannot be lower than zero") : thickness;

            CalculateSection();
        }

        /// <summary>
        /// Creates a copy of a section
        /// </summary>
        /// <param name="section">The section to copy</param>
        public SectionCHS(SectionCHS section)
            : this(section.Diameter, (section.Diameter - section.DiameterInternal) / 2.0, section.Name)
        {

        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionCHS(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("SectionCHSVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            _externalDiameter = info.GetDouble("D");
            _thickness = info.GetDouble("T");

            ResetMesh();
        }

        #endregion

        #region Public method

        /// <summary>
        /// Calculates the properties with the closed formulas of the annulus (all the axes are principal)
        /// </summary>
        public override void SetMechanicalProperties()
        {
            _area = CalculateArea();
            _j11 = CalculateJ();
            _j22 = _j11;
            _jxx = _j11;
            _jyy = _j11;

            _jxy = CalculateJxy();
            _jp = _jxx + _jyy;

            _jt = CalculateJt();
            _jw = CalculateJw();
            _centroid = CalculateCentroid();
            _shearCenter = _centroid;
            _angleX1 = CalculateAngle();
            _wel1Max = CalculateWel();
            _wel1Min = _wel1Max;
            _wel2Max = _wel1Max;
            _wel2Min = _wel1Max;
            _wpl1 = CalculateWpl();
            _wpl2 = _wpl1;

            // the moduli respect to X and Y are the same (before, not set: zero)
            _welXMax = _wel1Max;
            _welXMin = _wel1Max;
            _welYMax = _wel1Max;
            _welYMin = _wel1Max;
            _wplX = _wpl1;
            _wplY = _wpl1;

            _isSymmetricAlongXLocalAxis = CalculateIsSymmetricAlongXLocalAxis();
            _isSymmetricAlongYLocalAxis = CalculateIsSymmetricAlongYLocalAxis();

        }

        /// <summary>
        /// Calculate the area: π (D² - Di²) / 4
        /// </summary>
        /// <returns>The area</returns>
        protected override double CalculateArea()
        {
            return (Math.Pow(Diameter, 2.0) * Math.PI) / 4.0 - (Math.Pow(DiameterInternal, 2.0) * Math.PI) / 4.0;
        }

        /// <summary>
        /// Calculate the moment of inertia about any axis through the centre: π (D⁴ - Di⁴) / 64
        /// </summary>
        /// <returns>The moment of inertia</returns>
        protected virtual double CalculateJ()
        {
            return Math.PI * (Math.Pow(Diameter, 4.0) - Math.Pow(DiameterInternal, 4.0)) / (64.0);
        }

        /// <summary>
        /// Calculate the product of inertia: 0
        /// </summary>
        /// <returns>0</returns>
        protected override double CalculateJxy()
        {
            return 0;
        }

        /// <summary>
        /// Calculate the torsion constant: π (D⁴ - Di⁴) / 32
        /// </summary>
        /// <returns>The torsion constant</returns>
        protected override double CalculateJt()
        {
            return Math.PI * (Math.Pow(Diameter, 4.0) - Math.Pow(DiameterInternal, 4.0)) / (32.0);
        }

        /// <summary>
        /// Calculate the warping constant: 0
        /// </summary>
        /// <returns>0</returns>
        protected override double CalculateJw()
        {
            return 0;
        }

        /// <summary>
        /// Calculate the centroid: the centre (D / 2, D / 2)
        /// </summary>
        /// <returns>The centroid</returns>
        protected override Point2d CalculateCentroid()
        {
            return new Point2d(Diameter / 2.0, Diameter / 2.0);
        }

        /// <summary>
        /// Calculate the elastic modulus: π (D⁴ - Di⁴) / (32 D)
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected virtual double CalculateWel()
        {
            return Math.PI * (Math.Pow(Diameter, 4.0) - Math.Pow(DiameterInternal, 4.0)) / (32.0 * _externalDiameter);
        }

        /// <summary>
        /// Calculate the plastic modulus: (D³ - Di³) / 6
        /// </summary>
        /// <returns>The plastic modulus</returns>
        protected virtual double CalculateWpl()
        {
            return (Math.Pow(Diameter, 3.0) - Math.Pow(DiameterInternal, 3.0)) / (6.0);
        }

        /// <summary>
        /// The section is symmetric respect to X
        /// </summary>
        /// <returns>True</returns>
        protected override bool CalculateIsSymmetricAlongXLocalAxis()
        {
            return true;
        }

        /// <summary>
        /// The section is symmetric respect to Y
        /// </summary>
        /// <returns>True</returns>
        protected override bool CalculateIsSymmetricAlongYLocalAxis()
        {
            return true;
        }

        #endregion

        #region Public override methods 

        /// <summary>
        /// The shape with the circles approximated by polygons
        /// </summary>
        /// <param name="numberOfEdges">The number of edges of the polygons</param>
        /// <returns>The new shape</returns>
        protected Shape2d GetShape(int numberOfEdges)
        {
            var center = new Point2d(0.5 * _externalDiameter, 0.5 * _externalDiameter);
            return new Shape2d(new Polygon2d(_externalDiameter, numberOfEdges, center), new[] { new Polygon2d(_externalDiameter - 2.0 * _thickness, numberOfEdges, center) });
        }

        /// <summary>
        /// The shape of the section (circles with the default number of edges)
        /// </summary>
        /// <returns>The new shape</returns>
        protected override Shape2d GetShape()
        {
            var center = new Point2d(0.5 * _externalDiameter, 0.5 * _externalDiameter);
            return new Shape2d(new Polygon2d(_externalDiameter, origin: center), new[] { new Polygon2d(_externalDiameter - 2.0 * _thickness, origin: center) });
        }

        /// <summary>
        /// A mesh with a quadrangle between each edge of the outer and of the inner polygons
        /// </summary>
        /// <param name="numberOfEdges">The number of edges of the polygons</param>
        /// <returns>The new mesh</returns>
        protected Mesh GetMesh(int numberOfEdges = 32)
        {
            Shape2d shape = GetShape(numberOfEdges);

            Mesh mesh = new Mesh();

            for (int i = 0; i < shape.Fill.Count; i++)
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

        /// <summary>
        /// The mesh of the section (32 quadrangles)
        /// </summary>
        /// <returns>The new mesh</returns>
        protected override Mesh CreateMesh() => GetMesh(numberOfEdges: 32);

        /// <summary>
        /// Serializes the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 2;
            info.AddValue("SectionCHSVersion", version);

            info.AddValue("D", _externalDiameter);
            info.AddValue("T", _thickness);
        }

        /// <summary>
        /// The description of the section: "CHS D x t"
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString()
        {
            return $"CHS {_externalDiameter}x{_thickness}";
        }

        /// <summary>
        /// The shear center: the centroid (already set)
        /// </summary>
        /// <returns>The shear center</returns>
        protected override Point2d CalculateShearCenter() => _shearCenter;

        /// <summary>
        /// The plastic modulus respect to the axis 1 (already set)
        /// </summary>
        /// <returns>The plastic modulus</returns>
        protected override double CalculateWpl1() => _wpl1;

        /// <summary>
        /// The plastic modulus respect to the axis 2 (already set)
        /// </summary>
        /// <returns>The plastic modulus</returns>
        protected override double CalculateWpl2() => _wpl1;

        /// <summary>
        /// The elastic modulus (already set)
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected override double CalculateWel1Max() => _wel1Max;

        /// <summary>
        /// The elastic modulus (already set)
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected override double CalculateWel1Min() => _wel1Max;

        /// <summary>
        /// The elastic modulus (already set)
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected override double CalculateWel2Max() => _wel1Max;

        /// <summary>
        /// The elastic modulus (already set)
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected override double CalculateWel2Min() => _wel1Max;

        /// <summary>
        /// Builds the thin walls on the sides of a polygon on the middle line, discards the mesh and the shape and calculates the properties
        /// </summary>
        /// <param name="numberOfEdges">The number of edges of the polygon</param>
        private void CalculateSection(int numberOfEdges = 32)
        {
            var poly = new Polygon2d(_externalDiameter - _thickness, numberOfEdges, new Point2d(0.5 * _externalDiameter, 0.5 * _externalDiameter));
            var lines = poly.Explode();

            var thinWalls = new ThinWall[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                thinWalls[i] = new ThinWall(line.Start, line.End, _thickness);
            }

            SetThinWalls(thinWalls);

            ResetMesh();

            _shape = null; // before SetMechanicalProperties (it was after: the properties computed on the shape used the old one)

            SetMechanicalProperties();
        }

        #endregion

        #region Equals, hashcode, operators

        /// <summary>
        /// Equality with another CHS (see <see cref="Equals(SectionCHS)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal section</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as SectionCHS);
        }

        /// <summary>
        /// Equality of the section properties and of the dimensions
        /// </summary>
        /// <param name="other">The section to compare</param>
        /// <returns>True if the sections are equal</returns>
        public bool Equals(SectionCHS other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   _externalDiameter == other._externalDiameter &&
                   _thickness == other._thickness;
        }

        /// <summary>
        /// The hash code of the section and of the dimensions
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -241850544;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + _externalDiameter.GetHashCode();
                hashCode = hashCode * -1521134295 + _thickness.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(SectionCHS)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are equal</returns>
        public static bool operator ==(SectionCHS left, SectionCHS right)
        {
            return EqualityComparer<SectionCHS>.Default.Equals(left, right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(SectionCHS)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are different</returns>
        public static bool operator !=(SectionCHS left, SectionCHS right)
        {
            return !(left == right);
        }

        #endregion
    }
}