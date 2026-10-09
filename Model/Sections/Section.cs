using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;
using System.Threading;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A cross-section: area, moments of inertia, elastic and plastic moduli, centroid, shear center and principal axes. The properties are
    /// calculated from the shape (<see cref="SetMechanicalProperties"/>) or given (generic section). Axes: X and Y of the shape; the axis 1 is
    /// the principal axis of the maximum moment of inertia (<see cref="J11"/>), with direction <see cref="AngleX1"/> from X, the axis 2 is at
    /// +90° from it
    /// </summary>
    [Serializable]
    public class Section : ModelObjectId, ISectionShape, ISectionCurveShape, ISerializable
    {
        #region Enumerator

        /// <summary>
        /// How the section is formed
        /// </summary>
        public enum FormedTypes
        {
            /// <summary>
            /// Hot finished
            /// </summary>
            HotFinished,
            /// <summary>
            /// Cold formed
            /// </summary>
            ColdFormed,
        }

        /// <summary>
        /// How the section is made
        /// </summary>
        public enum SectionTypes
        {
            /// <summary>
            /// Rolled
            /// </summary>
            Rolled,
            /// <summary>
            /// Welded
            /// </summary>
            Welded,
        }

        #endregion

        #region Variables

        /// <summary>
        /// The material of the section.
        /// The material property should not be used, it is only for backward compatibility, to be able to read the material in serializations of old files.
        /// </summary>
        [System.ComponentModel.Browsable(false)]
        internal Material _material;

        /// <summary>
        /// The area
        /// </summary>
        protected double _area;
        /// <summary>
        /// The moment of inertia about the X axis through the centroid
        /// </summary>
        protected double _jxx;
        /// <summary>
        /// The moment of inertia about the Y axis through the centroid
        /// </summary>
        protected double _jyy;
        /// <summary>
        /// The product of inertia (integral of x y dA, centroidal axes)
        /// </summary>
        protected double _jxy;
        /// <summary>
        /// The polar moment of inertia
        /// </summary>
        protected double _jp;
        /// <summary>
        /// The torsion constant
        /// </summary>
        protected double _jt;
        /// <summary>
        /// The warping constant
        /// </summary>
        protected double _jw;
        /// <summary>
        /// The moment of inertia about the principal axis 1 (the maximum)
        /// </summary>
        protected double _j11;
        /// <summary>
        /// The moment of inertia about the principal axis 2 (the minimum)
        /// </summary>
        protected double _j22;

        /// <summary>
        /// The plastic modulus respect to the axis 1 (NaN: calculated at the first access to <see cref="Wpl1"/>)
        /// </summary>
        protected double _wpl1;
        /// <summary>
        /// The plastic modulus respect to the axis 2 (NaN: calculated at the first access to <see cref="Wpl2"/>)
        /// </summary>
        protected double _wpl2;
        /// <summary>
        /// The elastic modulus respect to the axis 1 of the fibre with the maximum coordinate
        /// </summary>
        protected double _wel1Max;
        /// <summary>
        /// The elastic modulus respect to the axis 1 of the fibre with the minimum coordinate
        /// </summary>
        protected double _wel1Min;
        /// <summary>
        /// The elastic modulus respect to the axis 2 of the fibre with the maximum coordinate
        /// </summary>
        protected double _wel2Max;
        /// <summary>
        /// The elastic modulus respect to the axis 2 of the fibre with the minimum coordinate
        /// </summary>
        protected double _wel2Min;

        /// <summary>
        /// The plastic modulus respect to X (NaN: calculated at the first access to <see cref="WplX"/>)
        /// </summary>
        protected double _wplX;
        /// <summary>
        /// The plastic modulus respect to Y (NaN: calculated at the first access to <see cref="WplY"/>)
        /// </summary>
        protected double _wplY;
        /// <summary>
        /// The elastic modulus respect to X of the top fibre
        /// </summary>
        protected double _welXMax;
        /// <summary>
        /// The elastic modulus respect to X of the bottom fibre
        /// </summary>
        protected double _welXMin;
        /// <summary>
        /// The elastic modulus respect to Y of the right fibre
        /// </summary>
        protected double _welYMax;
        /// <summary>
        /// The elastic modulus respect to Y of the left fibre
        /// </summary>
        protected double _welYMin;

        /// <summary>
        /// The shear center
        /// </summary>
        protected Point2d _shearCenter;
        /// <summary>
        /// The centroid
        /// </summary>
        protected Point2d _centroid;
        /// <summary>
        /// The angle of the principal axis 1 from X (radians)
        /// </summary>
        protected double _angleX1;

        /// <summary>
        /// True if the section is symmetric with respect to the X axis
        /// </summary>
        protected bool _isSymmetricAlongXLocalAxis;
        /// <summary>
        /// True if the section is symmetric with respect to the Y axis
        /// </summary>
        protected bool _isSymmetricAlongYLocalAxis;

        /// <summary>
        /// The mesh (null until the first access to <see cref="Mesh"/>)
        /// </summary>
        protected Mesh _mesh;
        /// <summary>
        /// The shape (created by <see cref="GetShape"/> at the first access to <see cref="Shape"/>)
        /// </summary>
        protected Shape2d _shape;

        /// <summary>
        /// The size of the mesh elements (0: the default of the mesher)
        /// </summary>
        private double _meshSize;

        /// <summary>
        /// The torsion solved with the finite elements for the values left to compute (null until the first access to one of them)
        /// </summary>
        // Runtime results never overwrite the serialized section definition.
        [NonSerialized]
        private CalculationCache _calculated;

        private sealed class CalculationCache
        {
            internal readonly Lazy<SectionTorsionProperties> Torsion;
            internal readonly Lazy<Point2d> ShearCenter;
            internal readonly Lazy<double> Wpl1, Wpl2, WplX, WplY;

            internal CalculationCache(Section section)
            {
                Torsion = new Lazy<SectionTorsionProperties>(() => section.CalculateTorsionProperties());
                ShearCenter = new Lazy<Point2d>(() => section.OnTheAxesOfSymmetry(Torsion.Value.ShearCenter));
                Wpl1 = new Lazy<double>(() => section.PlasticModulus(section._angleX1));
                Wpl2 = new Lazy<double>(() => section.PlasticModulus(section._angleX1 + Math.PI / 2.0));
                WplX = new Lazy<double>(() => section.PlasticModulus(0.0));
                WplY = new Lazy<double>(() => section.PlasticModulus(Math.PI / 2.0));
            }
        }

        private CalculationCache Calculated => LazyInitializer.EnsureInitialized(ref _calculated, () => new CalculationCache(this));

        #endregion

        #region Properties

        /// <summary>
        /// The area of the section
        /// </summary>
        public double Area => _area;

        /// <summary>
        /// The torsion constant
        /// </summary>
        /// <remarks>For a generic shape it is computed with the finite elements at the first access (see <see cref="CalculateTorsionProperties"/>;
        /// before, 0)</remarks>
        public double Jt => double.IsNaN(_jt) && SolvesTorsionNumerically ? Calculated.Torsion.Value.TorsionConstant : _jt;

        /// <summary>
        /// The warping constant
        /// </summary>
        /// <remarks>For a generic shape it is computed with the finite elements at the first access (see <see cref="CalculateTorsionProperties"/>;
        /// before, 0); <see cref="double.NaN"/> when not available (see <see cref="GetAvailability"/>)</remarks>
        public double Jw => double.IsNaN(_jw) && SolvesTorsionNumerically ? Calculated.Torsion.Value.WarpingConstant : _jw;

        /// <summary>
        /// The moment of inertia (second moment of area) about the X axis through the centroid
        /// </summary>
        public double Jxx => _jxx;

        /// <summary>
        /// The moment of inertia (second moment of area) about the Y axis through the centroid
        /// </summary>
        public double Jyy => _jyy;

        /// <summary>
        /// Product of Inertia: Integral of xy dA
        /// </summary>
        public double Jxy => _jxy;

        /// <summary>
        /// Polar Moment of Inertia: Integral of x^2 + y^2 dA = Jxx + Jyy
        /// </summary>
        public double Jp => _jp;

        /// <summary>
        /// The moment of inertia about the principal axis 1 (the maximum one)
        /// </summary>
        public double J11 => _j11;

        /// <summary>
        /// The moment of inertia about the principal axis 2 (the minimum one)
        /// </summary>
        public double J22 => _j22;

        /// <summary>
        /// The plastic modulus calculated respect the 1-principal axes
        /// </summary>
        /// <remarks>For a generic shape, the exact plastic modulus is computed at the first access (before, the minimum elastic modulus)</remarks>
        public double Wpl1 => double.IsNaN(_wpl1) ? Calculated.Wpl1.Value : _wpl1;

        /// <summary>
        /// The plastic modulus calculated respect the 2-principal axes
        /// </summary>
        /// <remarks>For a generic shape, the exact plastic modulus is computed at the first access (before, the minimum elastic modulus)</remarks>
        public double Wpl2 => double.IsNaN(_wpl2) ? Calculated.Wpl2.Value : _wpl2;

        /// <summary>
        /// The elastic modulus calculated respect the 1-principal axes and the minimum (with sign) distance respect the centroid
        /// </summary>
        public double Wel1Min => _wel1Min;

        /// <summary>
        /// The elastic modulus calculated respect the 1-principal axes and the maximum (with sign) distance respect the centroid
        /// </summary>
        public double Wel1Max => _wel1Max;

        /// <summary>
        /// The elastic modulus calculated respect the 2-principal axes and the minimum (with sign) distance respect the centroid
        /// </summary>
        public double Wel2Min => _wel2Min;

        /// <summary>
        /// The elastic modulus calculated respect the 2-principal axes and the maximum (with sign) distance respect the centroid
        /// </summary>
        public double Wel2Max => _wel2Max;

        /// <summary>
        /// The minimum elastic modulus calculated respect the 1-principal axes 
        /// </summary>
        public double Wel1 => Math.Min(_wel1Max, _wel1Min);

        /// <summary>
        /// The minimum elastic modulus calculated respect the 2-principal axes 
        /// </summary>
        public double Wel2 => Math.Min(_wel2Max, _wel2Min);

        /// <summary>
        /// The elastic modulus calculated respect the X axes and the minimum (with sign) distance respect the centroid
        /// </summary>
        public double WelXMin => _welXMin;

        /// <summary>
        /// The elastic modulus calculated respect the X axes and the maximum distance (with sign) respect the centroid
        /// </summary>
        public double WelXMax => _welXMax;

        /// <summary>
        /// The elastic modulus calculated respect the Y axes and the minimum (with sign) distance respect the centroid
        /// </summary>
        public double WelYMin => _welYMin;

        /// <summary>
        /// The elastic modulus calculated respect the Y axes and the maximum (with sign) distance respect the centroid
        /// </summary>
        public double WelYMax => _welYMax;

        /// <summary>
        /// The minimum elastic modulus calculated respect the X-principal axes 
        /// </summary>
        public double WelX => Math.Min(_welXMax, _welXMin);

        /// <summary>
        /// The minimum elastic modulus calculated respect the Y axis
        /// </summary>
        public double WelY => Math.Min(_welYMax, _welYMin);

        /// <summary>
        /// The plastic modulus calculated respect the X axes
        /// </summary>
        /// <remarks>For a generic shape, the exact plastic modulus is computed at the first access (before, the minimum elastic modulus)</remarks>
        public double WplX => double.IsNaN(_wplX) ? Calculated.WplX.Value : _wplX;

        /// <summary>
        /// The plastic modulus calculated respect the Y axes
        /// </summary>
        /// <remarks>For a generic shape, the exact plastic modulus is computed at the first access (before, <see cref="Wel2Max"/>)</remarks>
        public double WplY => double.IsNaN(_wplY) ? Calculated.WplY.Value : _wplY;

        /// <summary>
        /// The centroid of the section
        /// </summary>
        public Point2d Centroid => _centroid;

        /// <summary>
        /// The shear center of the section (in the coordinates of the shape)
        /// </summary>
        /// <remarks>For a generic shape it is computed with the finite elements at the first access (see <see cref="CalculateTorsionProperties"/>;
        /// before, the centroid)</remarks>
        public Point2d ShearCenter => _shearCenter is null && SolvesTorsionNumerically ? Calculated.ShearCenter.Value : _shearCenter;

        /// <summary>
        /// The shear centre solved with the finite elements, on the axes of symmetry of the section (the mesh is not symmetric: the coordinate
        /// along an axis of symmetry differs from the one of the centroid by about 1e-5 of the size)
        /// </summary>
        /// <param name="point">The shear centre of the finite elements</param>
        /// <returns>The shear centre with the coordinates of the centroid along the axes of symmetry</returns>
        private Point2d OnTheAxesOfSymmetry(Point2d point)
        {
            if (point is null || _centroid is null)
                return point;
            double x = _isSymmetricAlongYLocalAxis && !double.IsNaN(point.X) ? _centroid.X : point.X;
            double y = _isSymmetricAlongXLocalAxis && !double.IsNaN(point.Y) ? _centroid.Y : point.Y;
            return new Point2d(x, y);
        }

        /// <summary>
        /// The shear center of the section relative to the centroid
        /// </summary>
        public Point2d ShearCenterLocalCoord => ShearCenter - _centroid;

        /// <summary>
        /// The angle of rotation of the principal axis
        /// </summary>
        public double AngleX1 => _angleX1;

        /// <summary>
        /// The radius of gyration respect the axis 2: sqrt(J22 / A), the one of the buckling in the direction of the axis 1
        /// (the convention of the checkers: e.g. Cop2011Checker uses the effective length 1 with R11)
        /// </summary>
        public double R11 => Math.Sqrt(J22 / Area);

        /// <summary>
        /// The radius of gyration respect the axis 1: sqrt(J11 / A), the one of the buckling in the direction of the axis 2
        /// </summary>
        public double R22 => Math.Sqrt(J11 / Area);

        /// <summary>
        /// The radius of gyration respect the Y axis: sqrt(Jyy / A), the one of the buckling in the direction of the X axis (the same convention
        /// of <see cref="R11"/>)
        /// </summary>
        public double Rxx => Math.Sqrt(Jyy / Area);

        /// <summary>
        /// The radius of gyration respect the X axis: sqrt(Jxx / A), the one of the buckling in the direction of the Y axis (the same convention
        /// of <see cref="R22"/>)
        /// </summary>
        public double Ryy => Math.Sqrt(Jxx / Area);

        /// <summary>
        /// The radius of gyration respect the <see cref="Jxy"/>, with the sign of <see cref="Jxy"/> (before, NaN when Jxy is negative)
        /// </summary>
        public double Rxy => Math.Sign(Jxy) * Math.Sqrt(Math.Abs(Jxy) / Area);

        /// <summary>
        /// The radius of gyration respect <see cref="Jp"/> 
        /// </summary>
        public double Rp => Math.Sqrt(Jp / Area);

        /// <summary>
        /// True if the section is symmetric with respect to the X axis (the Y axis of the Eurocode)
        /// </summary>
        public bool IsSymmetricAlongXLocalAxis => _isSymmetricAlongXLocalAxis;

        /// <summary>
        /// True if the section is symmetric with respect to the Y axis (the Z axis of the Eurocode)
        /// </summary>
        public bool IsSymmetricAlongYLocalAxis => _isSymmetricAlongYLocalAxis;

        /// <summary>
        /// True if the section is symmetric with respect to both the X and the Y axes
        /// </summary>
        public bool IsDoubleSymmetric => (IsSymmetricAlongXLocalAxis && IsSymmetricAlongYLocalAxis);

        /// <summary>
        /// The mesh of the section, generated at the first access
        /// </summary>
        public virtual Mesh Mesh
        {
            get
            {
                if (_mesh is null)
                    Interlocked.CompareExchange(ref _mesh, CreateMesh(), null);
                return _mesh;
            }
        }

        /// <summary>
        /// The shape of the section, created at the first access (see <see cref="GetShape"/>)
        /// </summary>
        public virtual Shape2d Shape
        {
            get
            {
                if (_shape is null)
                    _shape = GetShape();
                return _shape;
            }
        }

        /// <summary>
        /// The height of the section (not implemented in the base class: it throws <see cref="NotImplementedException"/>)
        /// </summary>
        public virtual double Height
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// The width of the section (not implemented in the base class: it throws <see cref="NotImplementedException"/>)
        /// </summary>
        public virtual double Width
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// The thin walls of the section (not implemented in the base class: it throws <see cref="NotImplementedException"/>)
        /// </summary>
        public virtual ThinWallSection.ThinWall[] ThinWalls => throw new NotImplementedException();

        /// <summary>
        /// True if the torsion constant, the warping constant and the shear centre left to compute (<see cref="double.NaN"/> and null after
        /// <see cref="SetMechanicalProperties"/>, as in the generic shapes) are computed with the finite elements at the first access; false if
        /// the missing values mean "not available" (e.g. the warping constant of <see cref="SectionBuiltUp"/>)
        /// </summary>
        private protected virtual bool SolvesTorsionNumerically => true;

        /// <summary>
        /// Discards runtime calculations after changing the definition. Overrides of <see cref="SetMechanicalProperties"/>
        /// must call this method if they do not call the base implementation. Mutation during calculation is not supported.
        /// </summary>
        protected void InvalidateCalculatedProperties()
        {
            Interlocked.Exchange(ref _calculated, null);
            _mesh = null;
        }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a section without properties (the derived classes set them)
        /// </summary>
        /// <param name="name">The name of the section</param>
        protected Section(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Creates a section of a generic shape (the properties are calculated by <see cref="SetMechanicalProperties"/>)
        /// </summary>
        /// <param name="shape">The shape</param>
        /// <param name="name">The name of the section</param>
        public Section(Shape2d shape, string name = "")
            : base(name)
        {
            _shape = shape;
        }

        /// <summary>
        /// The default constructor of generic section: Jxx, Jyy and Jxy are obtained from the principal moments and the angle
        /// </summary>
        /// <param name="area">The area</param>
        /// <param name="j11">The moment of inertia around the first principal axis</param>
        /// <param name="j22">The moment of inertia around the second principal axis</param>
        /// <param name="jt">The torsion constant</param>
        /// <param name="jw">The warping constant</param>
        /// <param name="centroid">The centroid of the section</param>
        /// <param name="shearCenter">The shear center of the section (Z is ignored)</param>
        /// <param name="angle">The angle of rotation of the principal axis</param>
        /// <param name="name">The name of the section</param>
        /// <exception cref="ArgumentException">If the area or a moment of inertia is negative</exception>
        /// <remarks>Axis convention: X-axes is the Y-axes for Eurocode and Y-axes is the Z-axes for Eurocode
        /// If the X-axes is principal, the first moment of inertia is J11, If the Y-axes is principal, the first moment of inertia is J22.
        /// The elastic and plastic moduli are not set (zero)</remarks>
        public Section(double area, double j11, double j22, double jt, double jw,
            Point2d centroid, Point3d shearCenter, double angle, string name)
            : base(name)
        {
            _area = area < 0 ? throw new ArgumentException($"Area cannot be lower than zero") : area;
            _j11 = j11 < 0 ? throw new ArgumentException($"Moment of Inertia J11 cannot be lower than zero") : j11;
            _j22 = j22 < 0 ? throw new ArgumentException($"Moment of Inertia J22 cannot be lower than zero") : j22;
            _jt = jt < 0 ? throw new ArgumentException($"Moment of Inertia Jt cannot be lower than zero") : jt;
            _jw = jw < 0 ? throw new ArgumentException($"Moment of Inertia Jw cannot be lower than zero") : jw;
            _centroid = centroid;
            _shearCenter = shearCenter;
            _angleX1 = angle;

            // before, J11 and J22 were written only in Jxx and Jyy (J11, J22 and Jp were zero), also with the principal axes rotated.
            // Axes X-Y from the principal ones (the inverse of CalculateAngle / CalculateJAlpha)
            double cos = Math.Cos(angle), sin = Math.Sin(angle);
            _jxx = j11 * cos * cos + j22 * sin * sin;
            _jyy = j11 * sin * sin + j22 * cos * cos;
            _jxy = -0.5 * (j11 - j22) * Math.Sin(2.0 * angle);
            _jp = j11 + j22;
        }

        /// <summary>
        /// Deserialization constructor (the shape is read from the version 3)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected Section(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("SectionVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            _area = info.GetDouble("Area");
            _jxx = info.GetDouble("Jxx");
            _jyy = info.GetDouble("Jyy");
            _jxy = info.GetDouble("Jxy");
            _jp = info.GetDouble("Jp");
            _jt = info.GetDouble("Jt");
            _jw = info.GetDouble("Jw");
            _j11 = info.GetDouble("J11");
            _j22 = info.GetDouble("J22");
            _wpl1 = info.GetDouble("WPL1");
            _wpl2 = info.GetDouble("WPL2");
            _wel1Max = info.GetDouble("WEL1Max");
            _wel1Min = info.GetDouble("WEL1Min");
            _wel2Max = info.GetDouble("WEL2Max");
            _wel2Min = info.GetDouble("WEL2Min");
            _wplX = info.GetDouble("WPLX");
            _wplY = info.GetDouble("WPLY");
            _welXMax = info.GetDouble("WELXMax");
            _welXMin = info.GetDouble("WELXMin");
            _welYMax = info.GetDouble("WELYMax");
            _welYMin = info.GetDouble("WELYMin");
            _centroid = (Point2d)info.GetValue("Centroid", typeof(Point2d));
            _shearCenter = (Point2d)info.GetValue("ShearCenter", typeof(Point2d));
            _isSymmetricAlongXLocalAxis = (bool)info.GetValue("IsSymmetricAlongXLocalAxis", typeof(bool));
            _isSymmetricAlongYLocalAxis = (bool)info.GetValue("IsSymmetricAlongYLocalAxis", typeof(bool));
            _angleX1 = info.GetDouble("AngleX1");

            if (version >= 3)
                _shape = (Shape2d)info.GetValue("Shape2d", typeof(Shape2d));

            // before the version 4 the generic shapes had the torsion constant and the warping constant 0 and the shear centre in the centroid,
            // placeholders: now they are computed with the finite elements at the first access
            if (version < 4 && GetType() == typeof(Section) && _shape != null && _jt == 0 && _jw == 0)
            {
                _jt = double.NaN;
                _jw = double.NaN;
                _shearCenter = null;
            }

            // In old generic sections these values mixed the definition with the last computed results.
            // A generic section with a shape has no independently supplied torsion or plastic properties.
            if (version < 5 && GetType() == typeof(Section) && _shape != null)
            {
                _jt = _jw = _wpl1 = _wpl2 = _wplX = _wplY = double.NaN;
                _shearCenter = null;
            }
        }

        #endregion

        #region Public method

        /// <summary>
        /// Update the mesh size and regenerate the mesh with the new size
        /// </summary>
        /// <param name="size">The size of the mesh elements (not positive: the default of the mesher)</param>
        public void SetMeshSize(double size)
        {
            _meshSize = size > 0 ? size : 0;
            _mesh = GetMesh();
        }

        /// <summary>
        /// The torsion properties computed with the finite elements on the exact region of the section (with the fillets and the welds of the
        /// rolled and welded sections): Saint-Venant torsion constant, warping constant and shear centre, see <see cref="SectionTorsionProperties"/>.
        /// The properties <see cref="Jt"/>, <see cref="Jw"/> and <see cref="ShearCenter"/> of the sections with formulas are not changed
        /// </summary>
        /// <param name="meshSize">The size of the elements (not positive: half the minimum thickness, see <see cref="SectionTorsionProperties.MeshSize"/>)</param>
        /// <returns>The properties; not solved (<see cref="SectionTorsionProperties.Error"/>) for a section without region</returns>
        public virtual SectionTorsionProperties CalculateTorsionProperties(double meshSize = 0) => SectionTorsion.Calculate(GetPlasticShape(), meshSize);

        #endregion

        #region Protected methods

        /// <summary>
        /// The mesh returned by <see cref="Mesh"/> when it is not generated yet: the size is the minimum between half the largest side and the
        /// smallest side of the bounding box
        /// </summary>
        /// <returns>The new mesh</returns>
        protected virtual Mesh CreateMesh()
        {
            Point2d bBox = Shape.Get2dBoundingBox().Size;
            double size = Math.Min(Math.Max(bBox.X, bBox.Y) / 2.0, Math.Min(bBox.X, bBox.Y));
            return GetMesh(size);
        }

        /// <summary>
        /// Discards the mesh: it will be generated again at the next access to <see cref="Mesh"/>
        /// </summary>
        protected void ResetMesh()
        {
            _mesh = null;
        }

        #endregion

        #region Protected virtual methods

        /// <summary>
        /// Calculates all the properties of the section (area, centroid, moments of inertia, principal axes, shear center and moduli), in the
        /// order needed by the calculations
        /// </summary>
        public virtual void SetMechanicalProperties()
        {
            InvalidateCalculatedProperties();
            _area = CalculateArea();

            _centroid = CalculateCentroid();
            _isSymmetricAlongXLocalAxis = CalculateIsSymmetricAlongXLocalAxis();
            _isSymmetricAlongYLocalAxis = CalculateIsSymmetricAlongYLocalAxis();

            _jxx = CalculateJxx();
            _jyy = CalculateJyy();
            _jxy = CalculateJxy();

            // J11 and J22 before the angle, that compares them with Jxx and Jyy (before, the values of the previous calculation were used)
            _j11 = CalculateJ11();
            _j22 = CalculateJ22();
            _angleX1 = CalculateAngle();

            _jp = _jxx + _jyy;
            _jt = CalculateJt();
            _jw = CalculateJw();

            _shearCenter = CalculateShearCenter();

            // the moduli respect to X and Y first: when X and Y are principal the principal moduli are taken from them
            _welXMax = CalculateWelXMax();
            _welXMin = CalculateWelXMin();
            _welYMax = CalculateWelYMax();
            _welYMin = CalculateWelYMin();
            _wplX = CalculateWplX();
            _wplY = CalculateWplY();

            _wel1Max = CalculateWel1Max();
            _wel1Min = CalculateWel1Min();
            _wel2Max = CalculateWel2Max();
            _wel2Min = CalculateWel2Min();
            _wpl1 = CalculateWpl1();
            _wpl2 = CalculateWpl2();
        }

        /// <summary>
        /// Calculate the area of the section
        /// </summary>
        /// <returns>The value of the area</returns>
        protected virtual double CalculateArea()
        {
            return _shape?.GetArea() ?? 0.0;
        }

        /// <summary>
        /// Calculate the moment of inertia about the principal axis 1 (the maximum one) from Jxx, Jyy and Jxy
        /// </summary>
        /// <returns>The moment of inertia</returns>
        protected virtual double CalculateJ11() => SectionHelper.CalculateJ11(_jxx, _jyy, _jxy);

        /// <summary>
        /// Calculate the moment of inertia about the principal axis 2 (the minimum one) from Jxx, Jyy and Jxy
        /// </summary>
        /// <returns>The moment of inertia</returns>
        protected virtual double CalculateJ22() => SectionHelper.CalculateJ22(_jxx, _jyy, _jxy);

        // The properties are integrated exactly on the boundary of the shape: the mesh is not needed

        /// <summary>
        /// Calculate the moment of inertia about the X axis through the centroid, integrated on the boundary of the shape
        /// </summary>
        /// <returns>The moment of inertia</returns>
        protected virtual double CalculateJxx()
        {
            SectionHelper.CalculateInertiaMoments(Shape, _centroid, out double Jxx, out _, out _, out _);
            return Jxx;
        }

        /// <summary>
        /// Calculate the moment of inertia about the Y axis through the centroid, integrated on the boundary of the shape
        /// </summary>
        /// <returns>The moment of inertia</returns>
        protected virtual double CalculateJyy()
        {
            SectionHelper.CalculateInertiaMoments(Shape, _centroid, out _, out double Jyy, out _, out _);
            return Jyy;
        }

        /// <summary>
        /// Calculate the product of inertia (centroidal axes), integrated on the boundary of the shape
        /// </summary>
        /// <returns>The product of inertia</returns>
        protected virtual double CalculateJxy()
        {
            SectionHelper.CalculateInertiaMoments(Shape, _centroid, out _, out _, out double Jxy, out _);
            return Jxy;
        }

        /// <summary>
        /// Calculate the torsion constant
        /// </summary>
        /// <returns><see cref="double.NaN"/> in the base class: computed with the finite elements at the first access to <see cref="Jt"/> (before, 0)</returns>
        protected virtual double CalculateJt()
        {
            return double.NaN;
        }

        /// <summary>
        /// Calculate the warping constant
        /// </summary>
        /// <returns><see cref="double.NaN"/> in the base class: computed with the finite elements at the first access to <see cref="Jw"/> (before, 0)</returns>
        protected virtual double CalculateJw()
        {
            return double.NaN;
        }

        /// <summary>
        /// Calculate the angle of the principal axis 1 from X
        /// </summary>
        /// <returns>The angle (radians)</returns>
        protected virtual double CalculateAngle()
        {
            return SectionHelper.CalculateAngle(_j11, _j22, _jxx, _jyy, _jxy);
        }

        /// <summary>
        /// Calculate the centroid point of the section in X-Y plane
        /// </summary>
        /// <returns>The centroid</returns>
        protected virtual Point2d CalculateCentroid()
        {
            SectionHelper.CalculateStaticMoments(Shape, out double Sx, out double Sy);
            return SectionHelper.CalculateCentroid(Sx, Sy, _area);
        }

        /// <summary>
        /// Calculate the static moments of the section in X-Y plane (integrated on the mesh)
        /// </summary>
        /// <returns>The static moments respect to X and Y</returns>
        public virtual (double Sx, double Sy) CalculateStaticMoments()
        {
            SectionHelper.CalculateStaticMoments(Mesh, out double Sx, out double Sy);
            return (Sx, Sy);
        }

        /// <summary>
        /// Calculate the shear center
        /// </summary>
        /// <returns>null in the base class: computed with the finite elements at the first access to <see cref="ShearCenter"/> (before, the
        /// centroid)</returns>
        protected virtual Point2d CalculateShearCenter()
        {
            return null;
        }

        // Moduli respect to the principal axes 1 and 2 and to the axes X and Y.
        // The axis 1 is the principal axis of the maximum moment of inertia J11, with direction AngleX1 from X; the axis 2 is at +90° from it.
        // Coordinates of the principal system: x1 = (x - xc) cos + (y - yc) sin along the axis 1, y1 = -(x - xc) sin + (y - yc) cos along the axis 2.
        // The moduli respect to the axis 1 use the distances y1, the ones respect to the axis 2 the distances x1; "Min" is the fibre with the minimum
        // (negative) coordinate, "Max" the one with the maximum coordinate. Respect to X: Min at the bottom, Max at the top; respect to Y: Min on the left,
        // Max on the right. The moduli respect to X and Y are computed before the principal ones: when X and Y are principal the principal moduli are
        // taken from them (see PrincipalFromXY), so the sections that compute only the X and Y moduli get coherent principal moduli.
        // Before, the moduli respect to the axis 2 and to Y had the Min on the right (distance -x1), the other sections on the left

        /// <summary>
        /// The position of the principal axes respect to X and Y
        /// </summary>
        protected enum PrincipalAxes
        {
            /// <summary>The principal axes are rotated respect to X and Y</summary>
            Rotated,
            /// <summary>The axis 1 is X (angle 0): x1 = x, y1 = y</summary>
            AlongX,
            /// <summary>The axis 1 is -Y (angle -90°, the Y axis when Jyy is bigger than Jxx): x1 = -y, y1 = x</summary>
            AlongMinusY,
            /// <summary>The axis 1 is Y (angle +90°): x1 = y, y1 = -x</summary>
            AlongY,
            /// <summary>The axis 1 is -X (angle 180°): x1 = -x, y1 = -y</summary>
            AlongMinusX,
        }

        /// <summary>
        /// The position of the principal axes respect to X and Y, from <see cref="AngleX1"/> (tolerance 1e-12 on the angle)
        /// </summary>
        /// <returns>The position of the principal axes</returns>
        protected PrincipalAxes PrincipalFromXY()
        {
            const double tolerance = 1e-12;
            if (Math.Abs(_angleX1) <= tolerance)
                return PrincipalAxes.AlongX;
            if (Math.Abs(_angleX1 + Math.PI / 2.0) <= tolerance)
                return PrincipalAxes.AlongMinusY;
            if (Math.Abs(_angleX1 - Math.PI / 2.0) <= tolerance)
                return PrincipalAxes.AlongY;
            if (Math.Abs(Math.Abs(_angleX1) - Math.PI) <= tolerance)
                return PrincipalAxes.AlongMinusX;
            return PrincipalAxes.Rotated;
        }

        /// <summary>
        /// Calculate the plastic modulus respect to the axis 1
        /// </summary>
        /// <returns>The plastic modulus respect to the axis 1: <see cref="WplX"/> or <see cref="WplY"/> when X and Y are principal, otherwise
        /// <see cref="double.NaN"/> (the exact plastic modulus of the shape is computed at the first access to <see cref="Wpl1"/>). Before, the
        /// minimum elastic modulus</returns>
        protected virtual double CalculateWpl1()
        {
            switch (PrincipalFromXY())
            {
                case PrincipalAxes.AlongX:
                case PrincipalAxes.AlongMinusX:
                    return _wplX;
                case PrincipalAxes.AlongY:
                case PrincipalAxes.AlongMinusY:
                    return _wplY;
                default:
                    return double.NaN;
            }
        }

        /// <summary>
        /// Calculate the plastic modulus respect to the axis 2
        /// </summary>
        /// <returns>The plastic modulus respect to the axis 2: <see cref="WplY"/> or <see cref="WplX"/> when X and Y are principal, otherwise
        /// <see cref="double.NaN"/> (the exact plastic modulus of the shape is computed at the first access to <see cref="Wpl2"/>). Before, the
        /// minimum elastic modulus</returns>
        protected virtual double CalculateWpl2()
        {
            switch (PrincipalFromXY())
            {
                case PrincipalAxes.AlongX:
                case PrincipalAxes.AlongMinusX:
                    return _wplY;
                case PrincipalAxes.AlongY:
                case PrincipalAxes.AlongMinusY:
                    return _wplX;
                default:
                    return double.NaN;
            }
        }

        /// <summary>
        /// The exact plastic modulus of the shape
        /// </summary>
        /// <param name="angle">The direction of the bending axis from X (radians)</param>
        /// <returns>The plastic modulus of the shape for the bending about the axis through the centroid with direction <paramref name="angle"/>,
        /// 0 if the section has no shape</returns>
        private protected virtual double PlasticModulus(double angle)
        {
            Shape2d shape = GetPlasticShape();
            return shape is null || _centroid is null ? 0.0 : SectionHelper.CalculatePlasticModulus(shape, _centroid, angle);
        }

        /// <summary>
        /// The region of the exact plastic moduli: the shape of the section. The rolled and welded sections return their exact outline, with
        /// the fillets and the welds that their shape (used for the meshes) does not have
        /// </summary>
        /// <returns>The region</returns>
        internal virtual Shape2d GetPlasticShape() => Shape;

        /// <summary>Snapshots of the section boundary including worked corners. Existing calculations continue
        /// to use the polygonal shape; use ToShape on a snapshot to choose a different mesh tolerance.</summary>
        public virtual System.Collections.Generic.IReadOnlyList<SectionCurveOutline> GetCurveOutlines() =>
            SectionCurveExtensions.FromShape(GetPlasticShape());

        /// <summary>
        /// The extreme distance of the vertices of the shape from an axis through the centroid
        /// </summary>
        /// <param name="angle">The direction of the axis from X (radians)</param>
        /// <param name="maximum">True for the maximum, false for the minimum</param>
        /// <returns>The minimum (<paramref name="maximum"/> false) or the maximum coordinate y1 of the vertices of the shape, perpendicular to the
        /// axis through the centroid with direction <paramref name="angle"/>: -(x - xc) sin + (y - yc) cos (the distance with sign from the axis)</returns>
        private double ExtremeDistance(double angle, bool maximum)
        {
            double cosTeta = Math.Cos(angle);
            double sinTeta = Math.Sin(angle);
            Polygon3d fill = Shape.Fill;

            double extreme = maximum ? double.MinValue : double.MaxValue;
            for (int c = 0; c < fill.Count; c++)
            {
                double w = (fill[c].Y - Centroid.Y) * cosTeta - (fill[c].X - Centroid.X) * sinTeta;
                extreme = maximum ? Math.Max(extreme, w) : Math.Min(extreme, w);
            }

            return extreme;
        }

        /// <summary>
        /// The extreme coordinate of the vertices of the shape along an axis through the centroid
        /// </summary>
        /// <param name="angle">The direction of the axis from X (radians)</param>
        /// <param name="maximum">True for the maximum, false for the minimum</param>
        /// <returns>The minimum (<paramref name="maximum"/> false) or the maximum coordinate x1 of the vertices of the shape along the axis through
        /// the centroid with direction <paramref name="angle"/>: (x - xc) cos + (y - yc) sin (the distance with sign from the perpendicular axis)</returns>
        private double ExtremeCoordinate(double angle, bool maximum)
        {
            double cosTeta = Math.Cos(angle);
            double sinTeta = Math.Sin(angle);
            Polygon3d fill = Shape.Fill;

            double extreme = maximum ? double.MinValue : double.MaxValue;
            for (int c = 0; c < fill.Count; c++)
            {
                double u = (fill[c].X - Centroid.X) * cosTeta + (fill[c].Y - Centroid.Y) * sinTeta;
                extreme = maximum ? Math.Max(extreme, u) : Math.Min(extreme, u);
            }

            return extreme;
        }

        /// <summary>
        /// Calculate the elastic modulus respect to the axis 1 of the fibre with the minimum coordinate y1 (from the X and Y moduli when X and Y are
        /// principal)
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected virtual double CalculateWel1Min()
        {
            switch (PrincipalFromXY())
            {
                case PrincipalAxes.AlongX: return _welXMin;
                case PrincipalAxes.AlongMinusY: return _welYMin;
                case PrincipalAxes.AlongY: return _welYMax;
                case PrincipalAxes.AlongMinusX: return _welXMax;
                default: return _j11 / Math.Abs(ExtremeDistance(_angleX1, false));
            }
        }

        /// <summary>
        /// Calculate the elastic modulus respect to the axis 1 of the fibre with the maximum coordinate y1 (from the X and Y moduli when X and Y are
        /// principal)
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected virtual double CalculateWel1Max()
        {
            switch (PrincipalFromXY())
            {
                case PrincipalAxes.AlongX: return _welXMax;
                case PrincipalAxes.AlongMinusY: return _welYMax;
                case PrincipalAxes.AlongY: return _welYMin;
                case PrincipalAxes.AlongMinusX: return _welXMin;
                default: return _j11 / Math.Abs(ExtremeDistance(_angleX1, true));
            }
        }

        /// <summary>
        /// Calculate the elastic modulus respect to the axis 2 of the fibre with the minimum coordinate x1 (from the X and Y moduli when X and Y are
        /// principal)
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected virtual double CalculateWel2Min()
        {
            switch (PrincipalFromXY())
            {
                case PrincipalAxes.AlongX: return _welYMin;
                case PrincipalAxes.AlongMinusY: return _welXMax;
                case PrincipalAxes.AlongY: return _welXMin;
                case PrincipalAxes.AlongMinusX: return _welYMax;
                default: return _j22 / Math.Abs(ExtremeCoordinate(_angleX1, false));
            }
        }

        /// <summary>
        /// Calculate the elastic modulus respect to the axis 2 of the fibre with the maximum coordinate x1 (from the X and Y moduli when X and Y are
        /// principal)
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected virtual double CalculateWel2Max()
        {
            switch (PrincipalFromXY())
            {
                case PrincipalAxes.AlongX: return _welYMax;
                case PrincipalAxes.AlongMinusY: return _welXMin;
                case PrincipalAxes.AlongY: return _welXMax;
                case PrincipalAxes.AlongMinusX: return _welYMin;
                default: return _j22 / Math.Abs(ExtremeCoordinate(_angleX1, true));
            }
        }

        /// <summary>
        /// Calculate the plastic modulus respect to Y
        /// </summary>
        /// <returns><see cref="double.NaN"/>: the exact plastic modulus of the shape is computed at the first access to <see cref="WplY"/>
        /// (before, <see cref="Wel2Max"/>)</returns>
        protected virtual double CalculateWplY()
        {
            return double.NaN;
        }

        /// <summary>
        /// Calculate the plastic modulus respect to X
        /// </summary>
        /// <returns><see cref="double.NaN"/>: the exact plastic modulus of the shape is computed at the first access to <see cref="WplX"/>
        /// (before, the minimum elastic modulus respect to the axis 1)</returns>
        protected virtual double CalculateWplX()
        {
            return double.NaN;
        }

        // The moduli respect to X and Y use Jxx and Jyy (before, J11 and J22: wrong when the principal axes are rotated or when J11 was Jyy)

        /// <summary>
        /// Calculate the elastic modulus respect to X of the top fibre: Jxx / distance
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected virtual double CalculateWelXMax()
        {
            return _jxx / Math.Abs(ExtremeDistance(0.0, true));
        }

        /// <summary>
        /// Calculate the elastic modulus respect to X of the bottom fibre: Jxx / distance
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected virtual double CalculateWelXMin()
        {
            return _jxx / Math.Abs(ExtremeDistance(0.0, false));
        }

        /// <summary>
        /// Calculate the elastic modulus respect to Y of the right fibre: Jyy / distance
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected virtual double CalculateWelYMax()
        {
            return _jyy / Math.Abs(ExtremeCoordinate(0.0, true));
        }

        /// <summary>
        /// Calculate the elastic modulus respect to Y of the left fibre (before, of the right one): Jyy / distance
        /// </summary>
        /// <returns>The elastic modulus</returns>
        protected virtual double CalculateWelYMin()
        {
            return _jyy / Math.Abs(ExtremeCoordinate(0.0, false));
        }
        /// <summary>
        /// Tell if the section is symmetric with respect to the X axis (false in the base class)
        /// </summary>
        /// <returns>True if symmetric</returns>
        protected virtual bool CalculateIsSymmetricAlongXLocalAxis()
        {
            return false;
        }

        /// <summary>
        /// Tell if the section is symmetric with respect to the Y axis (false in the base class)
        /// </summary>
        /// <returns>True if symmetric</returns>
        protected virtual bool CalculateIsSymmetricAlongYLocalAxis()
        {
            return false;
        }

        /// <summary>
        /// Creates the shape of the section (the given one in the base class)
        /// </summary>
        /// <returns>The shape</returns>
        protected virtual Shape2d GetShape()
        {
            return _shape;
        }

        /// <summary>
        /// Generates a mesh of the shape; if the mesher fails, the initial mesh (without refinement) is returned
        /// </summary>
        /// <param name="meshSize">The size of the elements (0: the size set by <see cref="SetMeshSize(double)"/>)</param>
        /// <param name="initialMeshOnly">True for the initial mesh only</param>
        /// <param name="recombine">True to recombine the triangles in quadrangles</param>
        /// <param name="refine">True to refine the mesh</param>
        /// <returns>The new mesh</returns>
        public virtual Mesh GetMesh(double meshSize = 0, bool initialMeshOnly = false, bool recombine = false, bool refine = false)
        {
            if (meshSize == 0)
                meshSize = _meshSize;

            try
            {
                return SectionHelper.GenerateMesh(GetShape(), meshSize, initialMeshOnly, recombine, refine);
            }
            catch (Exception) { }

            return SectionHelper.GenerateMesh(GetShape(), meshSize, true, recombine, refine);
        }

        #endregion

        #region Public override method

        /// <summary>
        /// Serializes the definition, without running mechanical calculations. Version 5 keeps all deferred properties
        /// as NaN/null regardless of whether the runtime cache has been populated; older versions are still readable.
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 5;
            info.AddValue("SectionVersion", version);

            //info.AddValue("Material", _material); // Removed in version==3.
            info.AddValue("Area", _area);
            info.AddValue("Jxx", _jxx);
            info.AddValue("Jyy", _jyy);
            info.AddValue("Jxy", _jxy);
            info.AddValue("Jp", _jp);
            info.AddValue("Jt", _jt);
            info.AddValue("Jw", _jw);
            info.AddValue("J11", _j11);
            info.AddValue("J22", _j22);
            info.AddValue("WPL1", _wpl1);
            info.AddValue("WPL2", _wpl2);
            info.AddValue("WEL1Max", _wel1Max);
            info.AddValue("WEL1Min", _wel1Min);
            info.AddValue("WEL2Max", _wel2Max);
            info.AddValue("WEL2Min", _wel2Min);
            info.AddValue("WPLX", _wplX);
            info.AddValue("WPLY", _wplY);
            info.AddValue("WELXMax", _welXMax);
            info.AddValue("WELXMin", _welXMin);
            info.AddValue("WELYMax", _welYMax);
            info.AddValue("WELYMin", _welYMin);
            info.AddValue("Centroid", _centroid, typeof(Point2d));
            info.AddValue("ShearCenter", _shearCenter, typeof(Point2d));
            info.AddValue("IsSymmetricAlongXLocalAxis", _isSymmetricAlongXLocalAxis, typeof(bool));
            info.AddValue("IsSymmetricAlongYLocalAxis", _isSymmetricAlongYLocalAxis, typeof(bool));
            info.AddValue("AngleX1", _angleX1);
            info.AddValue("Shape2d", _shape);
        }

        /// <summary>
        /// Equality of the definition and name, independent of runtime calculations. Deferred properties compare as NaN;
        /// their source geometry is compared as well, without solving torsion or integrating plastic moduli.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is a section with the same properties</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return obj is Section section &&
                   _area == section._area &&
                   _jxx == section._jxx &&
                   _jyy == section._jyy &&
                   _jxy == section._jxy &&
                   _jp == section._jp &&
                   _jt.Equals(section._jt) &&
                   _jw.Equals(section._jw) &&
                   _j11 == section._j11 &&
                   _j22 == section._j22 &&
                   _wpl1.Equals(section._wpl1) &&
                   _wpl2.Equals(section._wpl2) &&
                   _wel1Max == section._wel1Max &&
                   _wel1Min == section._wel1Min &&
                   _wel2Max == section._wel2Max &&
                   _wel2Min == section._wel2Min &&
                   _wplX.Equals(section._wplX) &&
                   _wplY.Equals(section._wplY) &&
                   _welXMax == section._welXMax &&
                   _welXMin == section._welXMin &&
                   _welYMax == section._welYMax &&
                   _welYMin == section._welYMin &&
                   _angleX1 == section._angleX1 &&
                   _centroid == section._centroid &&
                   SamePoint(_shearCenter, section._shearCenter) &&
                   (!HasDeferredProperties || Equals(GetPlasticShape(), section.GetPlasticShape())) &&
                   _isSymmetricAlongXLocalAxis == section._isSymmetricAlongXLocalAxis &&
                   _isSymmetricAlongYLocalAxis == section._isSymmetricAlongYLocalAxis &&
                   base.Equals(obj);
        }

        private bool HasDeferredProperties => double.IsNaN(_jt) || double.IsNaN(_jw) || _shearCenter is null
            || double.IsNaN(_wpl1) || double.IsNaN(_wpl2) || double.IsNaN(_wplX) || double.IsNaN(_wplY);

        /// <summary>
        /// The hash code of the name and of the properties
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -17;
                hashCode = hashCode * -23 + base.GetHashCode();
                hashCode = hashCode * -23 + _area.GetHashCode();
                hashCode = hashCode * -23 + _jxx.GetHashCode();
                hashCode = hashCode * -23 + _jyy.GetHashCode();
                hashCode = hashCode * -23 + _jxy.GetHashCode();
                hashCode = hashCode * -23 + _jp.GetHashCode();
                // the torsion constant, the warping constant and the shear centre are not used: they can be computed at the first access
                hashCode = hashCode * -23 + _j11.GetHashCode();
                hashCode = hashCode * -23 + _j22.GetHashCode();
                // the plastic moduli are not used: they can be not calculated yet (NaN)
                hashCode = hashCode * -23 + _wel1Max.GetHashCode();
                hashCode = hashCode * -23 + _wel1Min.GetHashCode();
                hashCode = hashCode * -23 + _wel2Max.GetHashCode();
                hashCode = hashCode * -23 + _wel2Min.GetHashCode();
                hashCode = hashCode * -23 + _angleX1.GetHashCode();
                hashCode = hashCode * -23 + _centroid.GetHashCode();
                hashCode = hashCode * -23 + _isSymmetricAlongXLocalAxis.GetHashCode();
                hashCode = hashCode * -23 + _isSymmetricAlongYLocalAxis.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality of two points that can be null or have NaN coordinates (a shear centre not available)
        /// </summary>
        private static bool SamePoint(Point2d a, Point2d b)
        {
            if (a is null || b is null)
                return a is null && b is null;
            return a == b || a.X.Equals(b.X) && a.Y.Equals(b.Y);
        }

        /// <summary>
        /// How a property of the section is obtained: <see cref="PropertyAvailability.NotAvailable"/> if its value is not defined or is a
        /// placeholder that must not be used, otherwise the method declared by the section (exact, numerical, approximate formula or given)
        /// </summary>
        /// <param name="property">The property</param>
        /// <returns>The availability (the torsion properties computed at the first access are computed)</returns>
        public PropertyAvailability GetAvailability(SectionProperty property)
        {
            PropertyAvailability declared = DeclaredAvailability(property);
            return declared == PropertyAvailability.NotAvailable || !HasValue(property) ? PropertyAvailability.NotAvailable : declared;
        }

        /// <summary>
        /// How the section obtains a property, without checking its value (see <see cref="GetAvailability"/>). Base class: the section given
        /// by its values (no region) has the moduli not available and the other properties given; the generic shape has the properties exact
        /// on its boundary and the torsion properties solved with the finite elements
        /// </summary>
        /// <param name="property">The property</param>
        /// <returns>The declared availability</returns>
        protected virtual PropertyAvailability DeclaredAvailability(SectionProperty property)
        {
            if (GetPlasticShape() is null)
                return property == SectionProperty.ElasticModuli || property == SectionProperty.PlasticModuli
                    ? PropertyAvailability.NotAvailable
                    : PropertyAvailability.Given;

            return Declared(property, PropertyAvailability.Numerical, PropertyAvailability.Numerical, PropertyAvailability.Numerical);
        }

        /// <summary>
        /// The declared availability of a section with the geometric properties exact (area, centroid, moments of inertia, elastic and plastic
        /// moduli) and the given availability of the torsion properties
        /// </summary>
        /// <param name="property">The property</param>
        /// <param name="torsionConstant">The availability of the torsion constant</param>
        /// <param name="warpingConstant">The availability of the warping constant</param>
        /// <param name="shearCenter">The availability of the shear centre</param>
        /// <returns>The availability of <paramref name="property"/></returns>
        private protected static PropertyAvailability Declared(SectionProperty property, PropertyAvailability torsionConstant,
            PropertyAvailability warpingConstant, PropertyAvailability shearCenter)
        {
            switch (property)
            {
                case SectionProperty.TorsionConstant: return torsionConstant;
                case SectionProperty.WarpingConstant: return warpingConstant;
                case SectionProperty.ShearCenter: return shearCenter;
                default: return PropertyAvailability.Exact;
            }
        }

        /// <summary>
        /// True if the value of a property is a finite number (all the values of the group)
        /// </summary>
        private bool HasValue(SectionProperty property)
        {
            bool Finite(double v) => !double.IsNaN(v) && !double.IsInfinity(v);
            bool FinitePoint(Point2d p) => !(p is null) && Finite(p.X) && Finite(p.Y);

            switch (property)
            {
                case SectionProperty.Area: return Finite(_area) && _area > 0;
                case SectionProperty.Centroid: return FinitePoint(_centroid);
                case SectionProperty.MomentsOfInertia: return Finite(_jxx) && Finite(_jyy) && Finite(_jxy) && Finite(_j11) && Finite(_j22);
                case SectionProperty.ElasticModuli: return Finite(Wel1) && Finite(Wel2) && Finite(WelX) && Finite(WelY);
                case SectionProperty.PlasticModuli: return Finite(Wpl1) && Finite(Wpl2) && Finite(WplX) && Finite(WplY);
                case SectionProperty.TorsionConstant: return Finite(Jt);
                case SectionProperty.WarpingConstant: return Finite(Jw);
                case SectionProperty.ShearCenter: return FinitePoint(ShearCenter);
                default: return false;
            }
        }

        /// <summary>
        /// The points that define the section (not implemented in the base class)
        /// </summary>
        /// <returns>The points</returns>
        /// <exception cref="NotImplementedException">In the base class</exception>
        public virtual Point2d[] GetSectionPoints()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the working of the corners from the type of the section (fillet for rolled, chamfer for welded; not implemented in the base class)
        /// </summary>
        /// <param name="sectionType">The type of the section</param>
        /// <exception cref="NotImplementedException">In the base class</exception>
        public virtual void SetEdgeTypeFromSteelType(SectionTypes sectionType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are equal</returns>
        public static bool operator ==(Section left, Section right)
        {
            // before, NullReferenceException when left was null
            if (ReferenceEquals(left, right))
                return true;
            if (left is null || right is null)
                return false;
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are different</returns>
        public static bool operator !=(Section left, Section right)
        {
            return !(left == right);
        }

        #endregion
    }
}
