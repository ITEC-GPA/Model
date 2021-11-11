using System;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Elements;
using GPC.Model.FEM.Materials;
using GPC.Model.FEM.Properties;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class Section : ElementProperty
    {
        #region Enumerator

        public enum FormedTypes
        {
            HotFinished,
            ColdFormed,
        }

        public enum SectionTypes
        {
            Rolled,
            Welded,
        }

        #endregion


        #region Variables

        protected Material _material;
        protected double _area;
        protected double _jxx;
        protected double _jyy;
        protected double _jxy;
        protected double _jt;
        protected double _jw;
        protected double _j11;
        protected double _j22;
        protected double _wpl1;
        protected double _wpl2;
        protected double _wel1;
        protected double _wel2;

        protected Point2d _shearCenter;
        protected Point2d _centroid;
        protected double _angleX1;

        protected bool _isSymmetricAlongXLocalAxis;
        protected bool _isSymmetricAlongYLocalAxis;

        protected Mesh _mesh;
        protected Shape2d _shape;

        #endregion


        #region Properties

        /// <summary>
        /// The <see cref="Materials"/> of the section 
        /// </summary>
        public Material Material => _material;

        /// <summary>
        /// The area of the section
        /// </summary>
        public double Area => _area;

        /// <summary>
        /// 
        /// </summary>
        public double Jt => _jt;

        /// <summary>
        /// 
        /// </summary>
        public double Jw => _jw;

        /// <summary>
        /// The first moment of inertia around the X-axis
        /// </summary>
        public double Jxx => _jxx;

        /// <summary>
        /// The first moment of inertia around the Y-axis
        /// </summary>
        public double Jyy => _jyy;

        /// <summary>
        /// 
        /// </summary>
        public double Jxy => _jxy;

        /// <summary>
        /// The first moment of inertia around the 1st principal axes
        /// </summary>
        public double J11 => _j11;

        /// <summary>
        /// The first moment of inertia around the 2nd principal axes
        /// </summary>
        public double J22 => _j22;

        /// <summary>
        /// The plastic modulus calculated respect the 1-principal axes
        /// </summary>
        public double Wpl1 => _wpl1;

        /// <summary>
        /// The plastic modulus calculated respect the 2-principal axes
        /// </summary>
        public double Wpl2 => _wpl2;

        /// <summary>
        /// The elastic modulus calculated respect the 1-principal axes
        /// </summary>
        public double Wel1 => _wel1;

        /// <summary>
        /// The elastic modulus calculated respect the 2-principal axes
        /// </summary>
        public double Wel2 => _wel2;

        /// <summary>
        /// The centroid of the section
        /// </summary>
        public Point2d Centroid => _centroid;

        /// <summary>
        /// The shear center of the section
        /// </summary>
        public Point2d ShearCenter => _shearCenter;

        /// <summary>
        /// The angle of rotation of the principal axis
        /// </summary>
        public double AngleX1 => _angleX1;

        /// <summary>
        /// The radius of gyration respect the axis 2
        /// </summary>
        public double R11 => Math.Sqrt(J22 / Area);

        /// <summary>
        /// The radius of gyration respect the axis 1
        /// </summary>
        public double R22 => Math.Sqrt(J11 / Area);

        /// <summary>
        /// Is true if the section is symmetric along Y-axis
        /// </summary>
        public bool IsSymmetricAlongXLocalAxis => _isSymmetricAlongXLocalAxis;

        /// <summary>
        /// Is true if the section is symmetric along Z-axis
        /// </summary>
        public bool IsSymmetricAlongYLocalAxis => _isSymmetricAlongYLocalAxis;

        /// <summary>
        /// Is true if the section is symmetric along Z-axis and the Y-axis
        /// </summary>
        public bool IsDoubleSymmetric => (IsSymmetricAlongXLocalAxis && IsSymmetricAlongYLocalAxis);

        public virtual Mesh Mesh
        {
            get
            {
                if (_mesh is null)
                    _mesh = GenerateMesh();
                return _mesh;
            }
        }

        public virtual Shape2d Shape
        {
            get
            {
                if (_shape is null)
                    _shape = GetShape();
                return _shape;
            }
        }

        #endregion


        #region Public Constructors

        protected Section(string name)
            : base(name)
        { }

        protected Section(Material material, string name)
            : base(name)
        {
            _material = material;
        }

        /// <summary>
        /// The default constructor of generic section
        /// </summary>
        /// <param name="material">The <see cref="Materials"/> of the section </param>
        /// <param name="area">The area</param>
        /// <param name="j11">The moment of inertia around the first principal axis</param>
        /// <param name="j22">The moment of inertia around the second principal axis</param>
        /// <param name="jt"></param>
        /// <param name="jw"></param>
        /// <param name="centroid">The centroid of the section</param>
        /// <param name="shearCenter">The shear center of the section</param>
        /// <param name="angle">The angle of rotation of the principal axis</param>
        /// <param name="name">The name of the section</param>
        /// <exception cref="ArgumentException">If the input data are not correct</exception>
        /// <remarks>Axis convention: X-axes is the Y-axes for Eurocode and Y-axes is the Z-axes for Eurocode
        /// If the X-axes is principal, the first moment of inertia is J11, If the Y-axes is principal, the first moment of inertia is J22</remarks>
        public Section(Material material, double area, double j11, double j22, double jt, double jw, Point2d centroid, Point3d shearCenter, double angle, string name)
            : base(name)
        {
            _material = material;
            _area = area < 0 ? throw new ArgumentException($"Area cannot be lower than zero") : area;
            _jxx = j11 < 0 ? throw new ArgumentException($"Moment of Inertia J11 cannot be lower than zero") : j11;
            _jyy = j22 < 0 ? throw new ArgumentException($"Moment of Inertia J22 cannot be lower than zero") : j22;
            _jt = jt < 0 ? throw new ArgumentException($"Moment of Inertia Jt cannot be lower than zero") : jt;
            _jw = jw < 0 ? throw new ArgumentException($"Moment of Inertia Jw cannot be lower than zero") : jw;
            _centroid = centroid;
            _shearCenter = shearCenter;
            _angleX1 = angle;
        }

        public Section(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _area = info.GetDouble("Area");
            _jt = info.GetDouble("Jt");
            _jw = info.GetDouble("Jw");
            _j11 = info.GetDouble("J11");
            _j22 = info.GetDouble("J22");
            _centroid = (Point2d)info.GetValue("Centroid", typeof(Point2d));
            _shearCenter = (Point2d)info.GetValue("ShearCenter", typeof(Point2d));
            _angleX1 = info.GetDouble("AngleX1");
        }

        #endregion


        #region Public virtual material method


        public virtual double GetE()
        {
            return _material.E;
        }

        public virtual double GetNi()
        {
            return _material.Ni;
        }

        public virtual double GetShearModule()
        {
            return _material.GetShearModule();
        }

        public virtual double GetDensity()
        {
            return _material.Density;
        }

        public virtual double GetAlphaThermalExpansion()
        {
            return _material.AlfaThermalExpansion;
        }

        public IsotropicFemMaterial GetIsotropicFemMaterial()
        {
            return _material.GetIsotropicFemMaterial();
        }

        public virtual double GetMinSigma(double N, double M1, double M2)
        {
            double sigmap1 = N / Area - M1 / Wel1 + M2 / Wel2;
            double sigmap2 = N / Area - M1 / Wel1 - M2 / Wel2;
            double sigmap3 = N / Area + M1 / Wel1 + M2 / Wel2;
            double sigmap4 = N / Area + M1 / Wel1 - M2 / Wel2;

            return GetMin(new double[] { sigmap1, sigmap2, sigmap3, sigmap4 });
        }

        public virtual double GetMaxSigma(double N, double M1, double M2)
        {
            double sigmap1 = N / Area - M1 / Wel1 + M2 / Wel2;
            double sigmap2 = N / Area - M1 / Wel1 - M2 / Wel2;
            double sigmap3 = N / Area + M1 / Wel1 + M2 / Wel2;
            double sigmap4 = N / Area + M1 / Wel1 - M2 / Wel2;

            return GetMax(new double[] { sigmap1, sigmap2, sigmap3, sigmap4 });
        }

        private double GetMax(double[] array)
        {
            double startValue = array.First();

            for (int i = 0; i < array.Count(); i++)
            {
                if (array[i] > startValue)
                    startValue = array[i];
            }

            return startValue;
        }

        private double GetMin(double[] array)
        {
            double startValue = array.First();

            for (int i = 0; i < array.Count(); i++)
            {
                if (array[i] < startValue)
                    startValue = array[i];
            }

            return startValue;
        }

        #endregion


        protected virtual Shape2d GetShape()
        {
            return null;
        }

        protected virtual Mesh GenerateMesh()
        {
            return SectionHelper.GenerateMesh(GetShape());
        }

        #region Public override method

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Area", _area);
            info.AddValue("Jt", _jt);
            info.AddValue("Jw", _jw);
            info.AddValue("J11", _j11);
            info.AddValue("J22", _j22);
            info.AddValue("Centroid", _centroid, typeof(Point2d));
            info.AddValue("ShearCenter", _shearCenter, typeof(Point2d));
            info.AddValue("AngleX1", _angleX1);
        }

        #endregion

    }
}
