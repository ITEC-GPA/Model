using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Elements;
using GPC.Model.Materials;
using GPC.Model.Maths.GaussIntegrations;

namespace GPC.Model.Sections.Concrete
{
    public class ReinforcedConcreteSection : Section, IConcreteSection
    {
        #region Variables

        protected readonly ShapeEx _shapeEx;
        protected readonly ReinforcedConcreteRebar[] _rebars;
        protected Mesh _mesh;

        #endregion


        #region Properties

        public ShapeEx ShapeEx => _shapeEx;

        public ReinforcedConcreteRebar[] Rebars => _rebars;

        public ConcreteMaterial ConcreteMaterial => (ConcreteMaterial)_material;

        public Mesh Mesh
        {
            get
            {
                if (_mesh == null)
                    _mesh = GenerateMesh();
                return _mesh;
            }
        }

        public Shape2d Shape => _shapeEx;

        #endregion


        #region Public Constructors

        public ReinforcedConcreteSection(ShapeEx shapeEx, ReinforcedConcreteRebar[] rebars, string name = "")
            : base(name)
        {
            _shapeEx = shapeEx ?? throw new ArgumentNullException(nameof(shapeEx));
            _rebars = rebars ?? throw new ArgumentNullException(nameof(rebars));
            _material = shapeEx.Material;

            #region Input check

            Plane pln = _shapeEx.GetPlane();

            foreach (ReinforcedConcreteRebar rebar in _rebars)
            {
                if (!pln.IsPointOnPlane(rebar.Position))
                    throw new ArgumentException("Rebars must be on the plane of the section");
                if (!_shapeEx.IsPointInside(rebar.Position))
                    throw new ArgumentException("Rebars must be inside the section");
            }

            #endregion

            SetMechanicalProperties();
        }

        public ReinforcedConcreteSection(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            _shapeEx = (ShapeEx)info.GetValue("ShapeEx", typeof(ShapeEx));
            _rebars = (ReinforcedConcreteRebar[])info.GetValue("ReinforcedConcreteRebar", typeof(ReinforcedConcreteRebar[]));
        }

        #endregion


        #region Field Serialization

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ShapeEx", _shapeEx);
            info.AddValue("ReinforcedConcreteRebar", _rebars);
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Return all homogenized mechanical properties with default value of homogenized factor n
        /// </summary>
        /// <param name="areaH">The homogeneized area</param>
        /// <param name="SxH">The first moment of area calculated respect input X-axis of the homogeneized section</param>
        /// <param name="SyH">The first moment of area calculated respect input Y-axis of the homogeneized section</param>
        /// <param name="centroidH">The centroid of homogeneized section</param>
        /// <param name="JxxH">The first moment of area calculated respect X-axis passing throw the centroid of the homogeneized section</param>
        /// <param name="JyyH">The first moment of area calculated respect Y-axis passing throw the centroid of the homogeneized section</param>
        /// <param name="JxyH"></param>
        /// <param name="JpH"></param>
        /// <param name="J11H">The first moment of area calculated respect the first principal axis 
        /// passing throw the centroid of only concrete section of the homogeneized section</param>
        /// <param name="J22H">The first moment of area calculated respect the second principal axis 
        /// passing throw the centroid of only concrete section of the homogeneized section</param>
        /// <param name="angleX">The angle of rotation of the principal axis respect the X-Axis</param>
        public void GetHomogeneizedMechanicalProperties(out double areaH, out double SxH, out double SyH, out Point3d centroidH,
            out double JxxH, out double JyyH, out double JxyH, out double JpH, out double J11H, out double J22H, out double angleX)
        {
            ConcreteSectionHelper.GetHomogeneizedMechanicalProperties(Mesh, Centroid, Rebars, ConcreteMaterial, Area, Jxx, Jyy, Jxy,
                out areaH, out SxH, out SyH, out centroidH, out JxxH, out JyyH, out JxyH, out JpH, out J11H, out J22H, out angleX);
        }

        /// <summary>
        /// Return all homogenized mechanical properties with homogeneized factor <paramref name="n"/>
        /// </summary>
        /// <param name="n">The homogeneized factor</param>
        /// <param name="areaH">The homogeneized area</param>
        /// <param name="SxH">The first moment of area calculated respect input X-axis of the homogeneized section</param>
        /// <param name="SyH">The first moment of area calculated respect input Y-axis of the homogeneized section</param>
        /// <param name="centroidH">The centroid of homogeneized section</param>
        /// <param name="JxxH">The first moment of area calculated respect X-axis passing throw the centroid of the homogeneized section</param>
        /// <param name="JyyH">The first moment of area calculated respect Y-axis passing throw the centroid of the homogeneized section</param>
        /// <param name="JxyH"></param>
        /// <param name="JpH"></param>
        /// <param name="J11H">The first moment of area calculated respect the first principal axis 
        /// passing throw the centroid of only concrete section of the homogeneized section</param>
        /// <param name="J22H">The first moment of area calculated respect the second principal axis 
        /// passing throw the centroid of only concrete section of the homogeneized section</param>
        /// <param name="angleX">The angle of rotation of the principal axis respect the X-Axis</param>
        public void GetHomogeneizedMechanicalProperties(double n, out double areaH, out double SxH, out double SyH, out Point3d centroidH,
            out double JxxH, out double JyyH, out double JxyH, out double JpH, out double J11H, out double J22H, out double angleX)
        {
            ConcreteSectionHelper.GetHomogeneizedMechanicalProperties(n, Mesh, Rebars, Centroid, Area, Jxx, Jyy, Jxy,
                out areaH, out SxH, out SyH, out centroidH, out JxxH, out JyyH, out JxyH, out JpH, out J11H, out J22H, out angleX);
        }

        /// <summary>
        /// The centroid of the homogenized section with default value of homogenized factor n
        /// </summary>
        /// <param name="SxHomog">The first moment of area respect X-Axis</param>
        /// <param name="SyHomog">The first moment of area respect Y-Axis</param>
        /// <returns>The centroid</returns>
        public Point3d GetHomogenizedCentroid(out double SxHomog, out double SyHomog)
        {
            return ConcreteSectionHelper.GetHomogenizedCentroid(Mesh, Rebars, ConcreteMaterial, Area, out SxHomog, out SyHomog);
        }

        /// <summary>
        /// The centroid of the homogenized section with homogenized factor <paramref name="n"/>
        /// </summary>
        /// <param name="n">The homogenized factor</param>
        /// <param name="SxHomog">The first moment of area respect X-Axis</param>
        /// <param name="SyHomog">The first moment of area respect Y-Axis</param>
        /// <returns></returns>
        public Point3d GetHomogenizedCentroid(double n, out double SxHomog, out double SyHomog)
        {
            return ConcreteSectionHelper.GetHomogenizedCentroid(n, Mesh, Rebars, Area, out SxHomog, out SyHomog);
        }

        /// <summary>
        /// The homogenized area with default value of homogenized factor n
        /// </summary>
        /// <returns>The homogenized area</returns>
        public double GetHomogenizedArea()
        {
            return ConcreteSectionHelper.GetHomogenizedArea(Rebars, ConcreteMaterial, Area);
        }

        /// <summary>
        /// The homogenized area with homogenized factor <paramref name="n"/>
        /// </summary>
        /// <param name="n"></param>
        /// <returns>The homogenized area</returns>
        public double GetHomogenizedArea(double n)
        {
            return ConcreteSectionHelper.GetHomogenizedArea(n, Rebars, Area);
        }

        public double GetHomogeneizedJ11(double n)
        {
            return ConcreteSectionHelper.GetHomogeneizedJ11(n, Centroid, Mesh, Rebars, Area, Jxx, Jyy, Jxy);
        }

        public double GetHomogeneizedJ11()
        {
            return ConcreteSectionHelper.GetHomogeneizedJ11(Mesh, Centroid, Rebars, ConcreteMaterial, Area, Jxx, Jyy, Jxy);
        }

        public double GetHomogeneizedJ22(double n)
        {
            return ConcreteSectionHelper.GetHomogeneizedJ22(n, Centroid, Mesh, Rebars, Area, Jxx, Jyy, Jxy);
        }

        public double GetHomogeneizedJ22()
        {
            return ConcreteSectionHelper.GetHomogeneizedJ22(Mesh, Centroid, Rebars, ConcreteMaterial, Area, Jxx, Jyy, Jxy);
        }

        #endregion


        #region Protected Methods

        /// <summary>
        /// Internal method to set the mechanical properties to the section
        /// </summary>
        protected virtual void SetMechanicalProperties()
        {
            _area = CalculateArea();

            CalculateStaticMoments(Mesh, out double Sx, out double Sy);
            _centroid = CalculateCentroid(Sx, Sy, Area);

            CalculateInertiaMoments(Mesh, _centroid, out double Jxx, out double Jyy, out double Jxy, out double _);
            _jxx = Jxx;
            _jyy = Jyy;
            _jxy = Jxy;
            _angleX1 = CalculateAngle(Jxx, Jyy, Jxy);
            _j11 = CalculateJ11(Jxx, Jyy, Jxy);
            _j22 = CalculateJ22(Jxx, Jyy, Jxy);

            //TODO: implementare metodi di calcolo della sezione
            _jw = CalculateJw();
            _jt = CalculateJt();
            _shearCenter = CalculateShearCenter();
            _wel1 = CalculateWel1();
            _wel2 = CalculateWel2();
            _wpl1 = CalculateWpl1();
            _wpl2 = CalculateWpl2();
        }

        protected double CalculateArea()
        {
            return ShapeEx.GetArea();
        }

        protected void CalculateStaticMoments(Mesh mesh, out double Sx, out double Sy)
        {
            ConcreteSectionHelper.CalculateStaticMoments(mesh, out Sx, out Sy);
        }

        protected void CalculateInertiaMoments(Mesh mesh, Point3d centroid, out double Jxx, out double Jyy, out double Jxy, out double Jp)
        {
            ConcreteSectionHelper.CalculateInertiaMoments(mesh, centroid, out Jxx, out Jyy, out Jxy, out Jp);
        }

        protected void CalculateHomogeneizedInertiaMoments(Point3d centroid, double Jxx, double Jyy, double Jxy,
            out double JxxHomogenized, out double JyyHomogenized, out double JxyHomogenized, out double JpHomogenized)
        {
            ConcreteSectionHelper.CalculateHomogeneizedInertiaMoments(Rebars, Centroid, centroid, ConcreteMaterial, Jxx, Jyy, Jxy, Area,
                out JxxHomogenized, out JyyHomogenized, out JxyHomogenized, out JpHomogenized);

            // NOTA: ci siamo ricondotti a momenti d'inerzia rispetto al baricentro della sezione di solo calcestruzzo
        }

        protected void CalculateHomogeneizedInertiaMoments(double n, Point3d centroid, double Jxx, double Jyy, double Jxy,
            out double JxxHomogenized, out double JyyHomogenized, out double JxyHomogenized, out double JpHomogenized)
        {
            ConcreteSectionHelper.CalculateHomogeneizedInertiaMoments(n, Rebars, Centroid, centroid, Jxx, Jyy, Jxy, Area,
                out JxxHomogenized, out JyyHomogenized, out JxyHomogenized, out JpHomogenized);

            // NOTA: ci siamo ricondotti a momenti d'inerzia rispetto al baricentro della sezione di solo calcestruzzo
        }

        protected Point2d CalculateCentroid(double Sx, double Sy, double area)
        {
            return ConcreteSectionHelper.CalculateCentroid(Sx, Sy, area);
        }

        protected double CalculateAngle(double Jxx, double Jyy, double Jxy)
        {
            return ConcreteSectionHelper.CalculateAngle(Jxx, Jyy, Jxy);
        }

        protected double CalculateJ11(double Jxx, double Jyy, double Jxy)
        {
            return ConcreteSectionHelper.CalculateJ11(Jxx, Jyy, Jxy);
        }

        protected double CalculateJ22(double Jxx, double Jyy, double Jxy)
        {
            return ConcreteSectionHelper.CalculateJ22(Jxx, Jyy, Jxy);
        }

        /// <summary>
        /// Generate the mesh of the section. If <paramref name="size"/> not set, size is set as the default value of the minimum of the bounding box size divided by 2.
        /// </summary>
        /// <param name="size">The mesh size</param>
        /// <returns></returns>
        protected Mesh GenerateMesh(double size = -1)
        {
            return ConcreteSectionHelper.GenerateMesh(ShapeEx, size);
        }

        public virtual double CalculateN(ReinforcedConcreteRebar rebar)
        {
            return ConcreteSectionHelper.CalculateN(rebar, ConcreteMaterial);
        }

        public virtual double CalculateN(int rebar)
        {
            return ConcreteSectionHelper.CalculateN(rebar, Rebars, ConcreteMaterial);
        }

        protected virtual void CalculateIntegralInertiaMoment(MeshFace face, Point3d centroid, out double jxx, out double jyy, out double jxy)
        {
            ConcreteSectionHelper.CalculateIntegralInertiaMoment(Mesh, face, centroid, out jxx, out jyy, out jxy);
        }

        protected double CalculateWpl2()
        {
            return 0;
            throw new NotImplementedException();
        }

        protected double CalculateWpl1()
        {
            return 0;
            throw new NotImplementedException();
        }

        protected double CalculateWel2()
        {
            return 0;
            throw new NotImplementedException();
        }

        protected double CalculateWel1()
        {
            return 0;
            throw new NotImplementedException();
        }

        protected double CalculateJt()
        {
            return 0;
            throw new NotImplementedException();
        }

        protected Point2d CalculateShearCenter()
        {
            return new Point2d(0, 0);
            throw new NotImplementedException();
        }

        protected double CalculateJw()
        {
            return 0;
            throw new NotImplementedException();
        }

        #endregion

    }
}
