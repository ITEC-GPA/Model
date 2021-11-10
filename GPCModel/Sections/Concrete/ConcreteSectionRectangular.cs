using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Elements;
using GPC.Model.FEM.Materials;
using GPC.Model.Materials;

namespace GPC.Model.Sections.Concrete
{
    [Serializable]
    public class ConcreteSectionRectangular : SectionRectangular, IConcreteSection
    {
        #region Variables

        protected ReinforcedConcreteRebar[] _rebars;
        protected Mesh _mesh;

        #endregion

        #region Properties

        public ReinforcedConcreteRebar[] Rebars => _rebars;

        public ConcreteMaterial ConcreteMaterial => (ConcreteMaterial)_material;

        public Shape Shape => GetShape();

        public Mesh Mesh
        {
            get
            {
                if (_mesh == null)
                    _mesh = GetReinforcedConcreteSection().Mesh;
                return _mesh;
            }
        }

        #endregion

        #region Public Constructors

        public ConcreteSectionRectangular(double height, double width, ConcreteMaterial material, ReinforcedConcreteRebar[] rebars, string name = "")
            : base(height, width, material, name)
        {
            _rebars = rebars;
            _mesh = GenerateMesh();
        }

        public ConcreteSectionRectangular(SectionRectangular section, ReinforcedConcreteRebar[] rebars)
            : this(section.Height, section.Width, (ConcreteMaterial)section.Material, rebars, section.Name)
        {
            if (section.Material.GetType() != ConcreteMaterial.GetType())
                throw new ArgumentException("Material must be a ConcreteMaterial");
        }

        #endregion


        public Shape GetShape()
        {
            return new Shape(new Polygon3d(new Point3d[] { new Point3d(0, 0, 0), new Point3d(Width, 0, 0), new Point3d(Width, Height, 0), new Point3d(0, Height, 0) }));
        }

        protected ReinforcedConcreteSection GetReinforcedConcreteSection()
        {
            return new ReinforcedConcreteSection(new ShapeEx(GetShape(), ConcreteMaterial), Rebars, Name);
        }

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
            return ConcreteSectionHelper.GenerateMesh(Shape, size);
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

        #endregion
    }
}
