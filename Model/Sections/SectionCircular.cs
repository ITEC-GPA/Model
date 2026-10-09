using GPC.Geometry;
using GPC.Geometry.Meshes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A solid circular section, with the centre at (D / 2, D / 2); the properties are the exact ones of the circle
    /// </summary>
    [Serializable]
    public class SectionCircular : Section, ISerializable
    {
        /// <summary>
        /// The diameter
        /// </summary>
        protected double _diameter;

        /// <summary>
        /// The diameter (a positive new value calculates the properties again and discards the mesh; the shape is not discarded)
        /// </summary>
        public double Diameter
        {
            get => _diameter;
            set
            {
                if (_diameter != value && value > 0)
                {
                    _diameter = value;
                    SetMechanicalProperties();
                    ResetMesh();
                }
            }
        }

        /// <summary>
        /// The height: the diameter (the setter of the base class throws <see cref="NotImplementedException"/>)
        /// </summary>
        public override double Height => _diameter;

        /// <summary>
        /// The width: the diameter
        /// </summary>
        public override double Width => _diameter;

        /// <summary>
        /// The thin walls: null (the section is solid)
        /// </summary>
        public override ThinWallSection.ThinWall[] ThinWalls => null;

        #region Public Constructors

        /// <summary>
        /// The default constructor: calculates the properties
        /// </summary>
        /// <param name="diameter">The diameter</param>
        /// <param name="name">The section name</param>
        public SectionCircular(double diameter, string name = "")
            : base(name)
        {
            _diameter = diameter;
            SetMechanicalProperties();
        }

        /// <summary>
        /// Creates a copy of a section
        /// </summary>
        /// <param name="sectionCircular">The section to copy</param>
        public SectionCircular(SectionCircular sectionCircular)
            : this(sectionCircular.Diameter, sectionCircular.Name)
        {

        }


        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionCircular(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("SectionCircularVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            _diameter = info.GetDouble("Diameter");
        }

        #endregion

        #region Public Methods Specific

        /// <summary>
        /// Serializes the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 2;
            info.AddValue("SectionCircularVersion", version);

            info.AddValue("Diameter", _diameter);
        }

        #endregion

        #region Protected method

        /// <summary>
        /// The shape with the circle approximated by a polygon
        /// </summary>
        /// <param name="numberOfEdges">The number of edges of the polygon</param>
        /// <returns>The new shape</returns>
        protected Shape2d GetShape(int numberOfEdges = 32)
        {
            return new Shape2d(new Polygon2d(_diameter, numberOfEdges, _centroid));
        }

        /// <summary>
        /// The shape of the section (a polygon of 32 edges)
        /// </summary>
        /// <returns>The new shape</returns>
        protected override Shape2d GetShape()
        {
            return new Shape2d(new Polygon2d(_diameter, 32, _centroid));
        }

        /// <summary>The exact circular boundary, independent of the mesh discretization.</summary>
        public override System.Collections.Generic.IReadOnlyList<SectionCurveOutline> GetCurveOutlines() =>
            new[] { new SectionCurveOutline(SectionOutline.Circle(_diameter / 2, _diameter / 2, _diameter / 2)) };

        /// <summary>
        /// The exact outline: the circle as a polygon of <see cref="SectionOutline.SegmentsPerQuarter"/> sides every 90° (area within 1e-4), used
        /// by the numerical torsion and the overlaps with the concrete. The properties are the closed formulas; the shape (32 sides) is the one of
        /// the mesh
        /// </summary>
        /// <returns>The outline</returns>
        internal override Shape2d GetPlasticShape() =>
            new Shape2d(new Polygon2d(_diameter, 4 * SectionOutline.SegmentsPerQuarter, new Point2d(_diameter / 2.0, _diameter / 2.0)));

        /// <summary>
        /// A mesh with quadrangles between the outer polygon and an inner one of diameter D / 3 and triangles from the inner polygon to the centre
        /// </summary>
        /// <param name="numberOfEdges">The number of edges of the polygons</param>
        /// <returns>The new mesh</returns>
        protected Mesh GetMesh(int numberOfEdges = 32)
        {
            Point2d centroid = new Point2d(_diameter / 2.0, _diameter / 2.0);
            Shape2d shape = new Shape2d(new Polygon2d(_diameter, numberOfEdges, centroid), new[] { new Polygon2d(_diameter / 3.0, numberOfEdges, centroid) });

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

                mesh.AddFaceMesh(new Point3d[]
                {
                    new Point3d(shape.Holes[0][i]),
                    new Point3d(shape.Holes[0][shape.Holes[0].GetNextIndex(i)]),
                    centroid,
                });
            }

            return mesh;
        }

        /// <summary>
        /// The mesh of the section (32 edges)
        /// </summary>
        /// <returns>The new mesh</returns>
        protected override Mesh CreateMesh() => GetMesh(numberOfEdges: 32);

        /// <summary>
        /// Calculates the properties with the closed formulas of the circle (all the axes are principal)
        /// </summary>
        public override void SetMechanicalProperties()
        {
            InvalidateCalculatedProperties();
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
            _shearCenter = CalculateShearCenter();
            _angleX1 = CalculateAngle();
            _wel1Max = CalculateWel();
            _wel1Min = CalculateWel();
            _wel2Max = CalculateWel();
            _wel2Min = CalculateWel();
            _wpl1 = CalculateWpl();
            _wpl2 = CalculateWpl();

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
        /// Calculate the area: π D² / 4
        /// </summary>
        /// <returns>The area</returns>
        protected override double CalculateArea()
        {
            return Math.Pow(Diameter, 2.0) * Math.PI / 4.0;
        }

        /// <summary>
        /// Calculate the moment of inertia about any axis through the centre: π D⁴ / 64
        /// </summary>
        /// <returns>The moment of inertia</returns>
        protected virtual double CalculateJ()
        {
            return Math.PI * Math.Pow(Diameter, 4.0) / 64.0;
        }

        /// <summary>
        /// Calculate the product of inertia: 0
        /// </summary>
        /// <returns>0</returns>
        protected override double CalculateJxy()
        {
            return 0.0;
        }

        /// <summary>
        /// Calculate the torsion constant: π D⁴ / 32
        /// </summary>
        /// <returns>The torsion constant</returns>
        protected override double CalculateJt()
        {
            return Math.PI * Math.Pow(Diameter, 4.0) / 32.0;
        }

        /// <summary>
        /// Calculate the warping constant: 0
        /// </summary>
        /// <returns>0</returns>
        protected override double CalculateJw()
        {
            return 0.0;
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
        /// Calculate the shear centre: the centre (before, the same value from the base class)
        /// </summary>
        /// <returns>The shear centre</returns>
        protected override Point2d CalculateShearCenter() => CalculateCentroid();

        /// <summary>
        /// All the properties are exact (closed formulas of the circle)
        /// </summary>
        /// <param name="property">The property</param>
        /// <returns>The declared availability</returns>
        protected override PropertyAvailability DeclaredAvailability(SectionProperty property) =>
            Declared(property, PropertyAvailability.Exact, PropertyAvailability.Exact, PropertyAvailability.Exact);

        /// <summary>
        /// Calculate the elastic modulus: π D³ / 32
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected virtual double CalculateWel()
        {
            return Math.PI * Math.Pow(Diameter, 4.0) / (32.0 * Diameter);
        }

        /// <summary>
        /// Calculate the plastic modulus: D³ / 6
        /// </summary>
        /// <returns>The plastic modulus</returns>
        protected virtual double CalculateWpl()
        {
            return Math.Pow(Diameter, 3.0) / 6.0;
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

        #region Public Method

        /// <summary>
        /// The description of the section: "Circular D"
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString()
        {
            return $"Circular {_diameter}";
        }

        /// <summary>
        /// Equality of the section properties and of the diameter
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal section</returns>
        public override bool Equals(object obj)
        {
            return obj is SectionCircular circular && base.Equals(obj) && _diameter == circular._diameter;
        }

        /// <summary>
        /// The hash code of the section and of the diameter
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -23 + base.GetHashCode();
                hashCode = hashCode * -23 + _diameter.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// The points of the section (not implemented)
        /// </summary>
        /// <returns>Nothing</returns>
        /// <exception cref="NotImplementedException">Always</exception>
        public override Point2d[] GetSectionPoints()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Nothing: a solid circle has no corners
        /// </summary>
        /// <param name="sectionType">The type of the section</param>
        public override void SetEdgeTypeFromSteelType(SectionTypes sectionType) { }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>; a null <paramref name="left"/> throws <see cref="NullReferenceException"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are equal</returns>
        public static bool operator ==(SectionCircular left, SectionCircular right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are different</returns>
        public static bool operator !=(SectionCircular left, SectionCircular right)
        {
            return !(left == right);
        }

        #endregion
    }
}
