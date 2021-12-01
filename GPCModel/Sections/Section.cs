using System;
using System.Collections.Generic;
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
    [Serializable]
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

        /// <summary>
        /// Product of Inertia: Integral of xy dA
        /// </summary>
        protected double _jxy;

        /// <summary>
        /// Polar Moment of Inertia: Integral of x^2 + y^2 dA = Jxx + Jyy
        /// </summary>
        protected double _jp;
        protected double _jt;
        protected double _jw;
        protected double _j11;
        protected double _j22;

        protected double _wpl1;
        protected double _wpl2;
        protected double _wel1;
        protected double _wel2;
        protected double _wplX;
        protected double _wplY;
        protected double _welX;
        protected double _welY;

        protected Point2d _shearCenter;
        protected Point2d _centroid;
        protected double _angleX1;

        protected bool _isSymmetricAlongXLocalAxis;
        protected bool _isSymmetricAlongYLocalAxis;

        protected Mesh _mesh;
        protected Shape2d _shape;

        private double _meshSize;

        #endregion

        #region Properties

        /// <summary>
        /// The <see cref="Materials"/> of the section 
        /// </summary>
        public Material Material
        {
            get => _material;
            set => _material = value;
        }

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

        /// <inheritdoc cref="_jxy"/>
        public double Jxy => _jxy;

        /// <inheritdoc cref="_jp"/>
        public double Jp => _jp;

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
        /// The elastic modulus calculated respect the X axes
        /// </summary>
        public double WelX => _welX;

        /// <summary>
        /// The elastic modulus calculated respect the Y axes
        /// </summary>
        public double WelY => _welY;

        /// <summary>
        /// The plastic modulus calculated respect the X axes
        /// </summary>
        public double WplX => _wplX;

        /// <summary>
        /// The plastic modulus calculated respect the Y axes
        /// </summary>
        public double WplY => _wplY;

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
        {
        }

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

        protected Section(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
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
            _wel1 = info.GetDouble("WEL1");
            _wel2 = info.GetDouble("WEL2");
            _centroid = (Point2d)info.GetValue("Centroid", typeof(Point2d));
            _shearCenter = (Point2d)info.GetValue("ShearCenter", typeof(Point2d));
            _isSymmetricAlongXLocalAxis = (bool)info.GetValue("IsSymmetricAlongXLocalAxis", typeof(bool));
            _isSymmetricAlongYLocalAxis = (bool)info.GetValue("IsSymmetricAlongYLocalAxis", typeof(bool));
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

        /// <summary>
        /// Update the mesh size and regenerate the mesh with the new size
        /// </summary>
        /// <param name="size"></param>
        public void SetMeshSize(double size)
        {
            _meshSize = size > 0 ? size : 0;
            _mesh = GenerateMesh();
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

        #region Protected virtual methods

        /// <summary>
        /// Internal method to set the mechanical properties to the section
        /// </summary>
        protected virtual void SetMechanicalProperties()
        {
            _area = CalculateArea();

            _centroid = CalculateCentroid();
            _isSymmetricAlongXLocalAxis = CalculateIsSymmetricAlongXLocalAxis();
            _isSymmetricAlongYLocalAxis = CalculateIsSymmetricAlongYLocalAxis();

            _jxx = CalculateJxx();
            _jyy = CalculateJyy();
            _jxy = CalculateJxy();

            _angleX1 = CalculateAngle();
            _j11 = CalculateJ11();
            _j22 = CalculateJ22();

            _jp = _jxx + _jyy;
            _jt = CalculateJt();
            _jw = CalculateJw();
            
            _shearCenter = CalculateShearCenter();
            
            _wel1 = CalculateWel1();
            _wel2 = CalculateWel2();
            _wpl1 = CalculateWpl1();
            _wpl2 = CalculateWpl2();

            _welX = CalculateWelX();
            _welY = CalculateWelY();
            _wplX = CalculateWplX();
            _wplY = CalculateWplY();
        }


        /// <summary>
        /// Calculate the area of the section
        /// </summary>
        /// <returns>The value of the area</returns>
        protected virtual double CalculateArea()
        {
            return 0;
        }

        protected virtual double CalculateJ11()
        {
            return 0;
        }

        protected virtual double CalculateJ22()
        {
            return 0;
        }

        protected virtual double CalculateJxx()
        {
            return 0;
        }

        protected virtual double CalculateJyy()
        {
            return 0;
        }

        protected virtual double CalculateJxy()
        {
            return 0;
        }

        protected virtual double CalculateJt()
        {
            return 0;
        }

        protected virtual double CalculateJw()
        {
            return 0;
        }

        protected virtual double CalculateAngle()
        {
            return 0.0;
        }

        /// <summary>
        /// Calculate the centroid point of the section in X-Y plane 
        /// </summary>
        protected virtual Point2d CalculateCentroid()
        {
            return new Point2d();
        }

        protected virtual Point2d CalculateShearCenter()
        {
            return new Point2d();
        }

        protected virtual double CalculateWpl1()
        {
            return 0;
        }

        protected virtual double CalculateWpl2()
        {
            return 0;
        }

        protected virtual double CalculateWel1()
        {
            return 0;
        }

        protected virtual double CalculateWel2()
        {
            return 0;
        }

        protected virtual double CalculateWplX()
        {
            return CalculateWpl1();
        }

        protected virtual double CalculateWplY()
        {
            return CalculateWpl2();
        }

        protected virtual double CalculateWelX()
        {
            return CalculateWel1();
        }

        protected virtual double CalculateWelY()
        {
            return CalculateWel2();
        }

        protected virtual bool CalculateIsSymmetricAlongXLocalAxis()
        {
            return false;
        }

        protected virtual bool CalculateIsSymmetricAlongYLocalAxis()
        {
            return false;
        }

        protected virtual Shape2d GetShape()
        {
            return null;
        }

        protected virtual Mesh GenerateMesh()
        {
            return SectionHelper.GenerateMesh(GetShape(), _meshSize);
        }

        #endregion

        #region Public override method

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
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
            info.AddValue("WEL1", _wel1);
            info.AddValue("WEL2", _wel2);
            info.AddValue("Centroid", _centroid, typeof(Point2d));
            info.AddValue("ShearCenter", _shearCenter, typeof(Point2d));
            info.AddValue("IsSymmetricAlongXLocalAxis", _isSymmetricAlongXLocalAxis, typeof(bool));
            info.AddValue("IsSymmetricAlongYLocalAxis", _isSymmetricAlongYLocalAxis, typeof(bool));
            info.AddValue("AngleX1", _angleX1);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return obj is Section section &&
                   _material.Equals(section._material) &&
                   _area == section._area &&
                   _jxx == section._jxx &&
                   _jyy == section._jyy &&
                   _jxy == section._jxy &&
                   _jp == section._jp &&
                   _jt == section._jt &&
                   _jw == section._jw &&
                   _j11 == section._j11 &&
                   _j22 == section._j22 &&
                   _wpl1 == section._wpl1 &&
                   _wpl2 == section._wpl2 &&
                   _wel1 == section._wel1 &&
                   _wel2 == section._wel2 &&
                   _angleX1 == section._angleX1 &&
                   _centroid == section._centroid &&
                   _shearCenter == section._shearCenter &&
                   _isSymmetricAlongXLocalAxis == section._isSymmetricAlongXLocalAxis &&
                   _isSymmetricAlongYLocalAxis == section._isSymmetricAlongYLocalAxis &&
                   base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -17;
                hashCode = hashCode * -23 + base.GetHashCode();
                hashCode = hashCode * -23 + _material.GetHashCode();
                hashCode = hashCode * -23 + _area.GetHashCode();
                hashCode = hashCode * -23 + _jxx.GetHashCode();
                hashCode = hashCode * -23 + _jyy.GetHashCode();
                hashCode = hashCode * -23 + _jxy.GetHashCode();
                hashCode = hashCode * -23 + _jp.GetHashCode();
                hashCode = hashCode * -23 + _jt.GetHashCode();
                hashCode = hashCode * -23 + _jw.GetHashCode();
                hashCode = hashCode * -23 + _j11.GetHashCode();
                hashCode = hashCode * -23 + _j22.GetHashCode();
                hashCode = hashCode * -23 + _wpl1.GetHashCode();
                hashCode = hashCode * -23 + _wpl2.GetHashCode();
                hashCode = hashCode * -23 + _wel1.GetHashCode();
                hashCode = hashCode * -23 + _wel2.GetHashCode();
                hashCode = hashCode * -23 + _angleX1.GetHashCode();
                hashCode = hashCode * -23 + _centroid.GetHashCode();
                hashCode = hashCode * -23 + _shearCenter.GetHashCode();
                hashCode = hashCode * -23 + _isSymmetricAlongXLocalAxis.GetHashCode();
                hashCode = hashCode * -23 + _isSymmetricAlongYLocalAxis.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Section left, Section right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Section left, Section right)
        {
            return !(left == right);
        }

        #endregion
    }
}
