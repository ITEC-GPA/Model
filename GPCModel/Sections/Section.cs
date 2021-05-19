using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.FEM.Properties;
using GPC.Model.FEM.Materials;
using GPC.Model.Elements;

namespace GPC.Model.Sections
{
    public class Section : ElementProperty
    {
        #region Struct

        public struct ShapeMaterial
        {
            public Shape Shape { get; set; }
            public Material Material { get; set; }
        }

        #endregion


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
        protected double _jt;
        protected double _jw;
        protected double _sx;
        protected double _sy;

        protected Point2d _shearCenter;
        protected Point2d _centroid;
        protected double _angleX1;
        protected SectionTypes _sectionType;
        protected FormedTypes _formedType;

        public readonly bool _isSymmetricAlongXLocalAxis;
        public readonly bool _isSymmetricAlongYLocalAxis;
        public readonly bool _isDoubleSymmetric;

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
        /// The first moment of area around the X-axis
        /// </summary>
        public double Sx => _sx;

        /// <summary>
        /// The first moment of area around the Y-axis
        /// </summary>
        public double Sy => _sy;

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
        /// The radius of gyration respect the X-axis
        /// </summary>
        public double InertiaRadiusX => Math.Sqrt(Jxx / Area);

        /// <summary>
        /// The radius of gyration respect the Y-axis
        /// </summary>
        public double InertiaRadiusY => Math.Sqrt(Jyy / Area);

        /// <summary>
        /// The type of the section (rolled or welded)
        /// </summary>
        public SectionTypes SectionType => _sectionType;

        /// <summary>
        /// The type of the section (HotFinished or ColdFormed)
        /// </summary>
        public FormedTypes FormedType => _formedType;

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
        public bool IsDoubleSymmetric => _isDoubleSymmetric;
        

        #endregion


        #region Public Constructors

        protected Section(Material material, string name) : base(name)
        {
            _material = material;
        }

        /// <summary>
        /// The default constructor of generic section
        /// </summary>
        /// <param name="material">The <see cref="Materials"/> of the section </param>
        /// <param name="area">The area</param>
        /// <param name="sx">The first moment of area around the X-axis</param>
        /// <param name="sy">The first moment of area around the Y-axis</param>
        /// <param name="j11">The moment of inertia around the first principal axis</param>
        /// <param name="j22">The moment of inertia around the second principal axis</param>
        /// <param name="jt"></param>
        /// <param name="jw"></param>
        /// <param name="centroid">The centroid of the section</param>
        /// <param name="shearCenter">The shear center of the section</param>
        /// <param name="angle">The angle of rotation of the principal axis</param>
        /// <param name="name">The name of the section</param>
        /// <param name="formed">The formed types (cold formed or hot finished) - only for steel section</param>
        /// <param name="sectionType">The type of the section (Rolled or welded) - only for steel section</param>
        /// <exception cref="ArgumentException">If the input data are not correct</exception>
        public Section(Material material, double area, double sx, double sy, double j11, double j22, double jt, double jw, Point2d centroid, Point3d shearCenter, double angle, string name, 
                        FormedTypes formed = FormedTypes.ColdFormed, SectionTypes sectionType = SectionTypes.Rolled) 
            : base(name)
        {
            _material = material;
            _area = area < 0 ? throw new ArgumentException($"Area cannot be lower than zero") : area;
            _sx = sx < 0 ? throw new ArgumentException($"Moment of Area J11 cannot be lower than zero") : sx;
            _sy = sy < 0 ? throw new ArgumentException($"Moment of Area J11 cannot be lower than zero") : sy;
            _jxx = j11 < 0 ? throw new ArgumentException($"Moment of Inertia J11 cannot be lower than zero") : j11; 
            _jyy = j22 < 0 ? throw new ArgumentException($"Moment of Inertia J22 cannot be lower than zero") : j22; 
            _jt = jt < 0 ? throw new ArgumentException($"Moment of Inertia Jt cannot be lower than zero") : jt; 
            _jw = jw < 0 ? throw new ArgumentException($"Moment of Inertia Jw cannot be lower than zero") : jw;
            _formedType = formed;
            _sectionType = sectionType;
            _centroid = centroid;
            _shearCenter = shearCenter;
            _angleX1 = angle;
        }

        public Section(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _area = info.GetDouble("Area");
            _jt = info.GetDouble("Jt");
            _jw = info.GetDouble("Jw");
            _sx = info.GetDouble("Sx");
            _sy = info.GetDouble("Sy");
            _jxx = info.GetDouble("J11");
            _jyy = info.GetDouble("J22");
            _centroid = (Point2d)info.GetValue("Centroid", typeof(Point2d));
            _shearCenter = (Point2d)info.GetValue("ShearCenter", typeof(Point2d));
            _angleX1 = info.GetDouble("AngleX1");
        }

        #endregion


        #region Public Methods Specific

        public virtual ShapeMaterial[] GetShapes()
        {
            return null;
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

        #endregion


        #region Public override method

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Area", _area);
            info.AddValue("Jt", _jt);
            info.AddValue("Jw", _jw);
            info.AddValue("J11", _jxx);
            info.AddValue("J22", _jyy);
            info.AddValue("Centroid", _centroid, typeof(Point2d));
            info.AddValue("ShearCenter", _shearCenter, typeof(Point2d));
            info.AddValue("AngleX1", _angleX1);
        }

        #endregion

    }
}
