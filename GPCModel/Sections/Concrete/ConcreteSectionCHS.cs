using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Materials;
using GPC.Model.Sections.Rebar;

namespace GPC.Model.Sections.Concrete
{
    public class ConcreteSectionCHS : SectionCHS, IConcreteSection
    {
        #region Variables

        protected ReinforcedConcreteRebar[] _rebars;
        protected Mesh _mesh;

        #endregion

        #region Properties

        public ReinforcedConcreteRebar[] Rebars => _rebars;

        public ConcreteMaterial ConcreteMaterial => (ConcreteMaterial)_material;

        public Shape2d Shape => GetShape();

        public Mesh Mesh
        {
            get
            {
                if (_mesh == null)
                    _mesh = GenerateMesh();
                return _mesh;
            }
        }

        #endregion

        #region Public Constructors

        public ConcreteSectionCHS(double diameter, double thickness, ConcreteMaterial material, ReinforcedConcreteRebar[] rebars, string name = "")
            : base(diameter, thickness, material, name)
        {
            _rebars = rebars;
            _mesh = GenerateMesh();
        }

        public ConcreteSectionCHS(SectionCHS sectionCHS, ReinforcedConcreteRebar[] rebars)
            : base(sectionCHS)
        {
            _rebars = rebars;
            _mesh = GenerateMesh();

            if (sectionCHS.Material.GetType() != ConcreteMaterial.GetType())
                throw new ArgumentException("Material must be a ConcreteMaterial");
        }

        public ConcreteSectionCHS(double diameter, double thickness, ConcreteMaterial material, double externalConcreteCover,
            int externalNumberOfRebars, IRebarSection externalRebarSection, double internalConcreteCover,
            int internalNumberOfRebars, IRebarSection internalRebarSection, double externalEpsilonP = 0.0, double internalEpsilonP = 0.0, string name = "")
            : base(diameter, thickness, material, name)
        {
            List<ReinforcedConcreteRebar> externalRebars = SetRadialRebars(externalConcreteCover, externalNumberOfRebars, externalRebarSection, externalEpsilonP).ToList();
            List<ReinforcedConcreteRebar> internalRebars = SetRadialRebars(internalConcreteCover, internalNumberOfRebars, internalRebarSection, internalEpsilonP).ToList();

            externalRebars.AddRange(internalRebars);

            _rebars = externalRebars.ToArray();
            _mesh = GenerateMesh();
        }

        public ConcreteSectionCHS(double diameter, double thickness, ConcreteMaterial material, double concreteCover,
            int numberOfRebars, IRebarSection rebarSection, double epsilonP = 0.0, string name = "")
            : base(diameter, thickness, material, name)
        {
            _rebars = SetRadialRebars(concreteCover, numberOfRebars, rebarSection, epsilonP);
            _mesh = GenerateMesh();
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
            areaH = GetHomogenizedArea();
            centroidH = GetHomogenizedCentroid(out SxH, out SyH);
            CalculateHomogeneizedInertiaMoments(centroidH, Jxx, Jyy, Jxy, out JxxH, out JyyH, out JxyH, out JpH);
            J11H = CalculateJ11(JxxH, JyyH, JxyH);
            J22H = CalculateJ22(JxxH, JyyH, JxyH);
            angleX = CalculateAngle(JxxH, JyyH, JxyH);
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
            areaH = GetHomogenizedArea(n);
            centroidH = GetHomogenizedCentroid(n, out SxH, out SyH);
            CalculateHomogeneizedInertiaMoments(n, centroidH, Jxx, Jyy, Jxy, out JxxH, out JyyH, out JxyH, out JpH);
            J11H = CalculateJ11(JxxH, JyyH, JxyH);
            J22H = CalculateJ22(JxxH, JyyH, JxyH);
            angleX = CalculateAngle(JxxH, JyyH, JxyH);
        }

        /// <summary>
        /// The centroid of the homogenized section with default value of homogenized factor n
        /// </summary>
        /// <param name="SxHomog">The first moment of area respect X-Axis</param>
        /// <param name="SyHomog">The first moment of area respect Y-Axis</param>
        /// <returns>The centroid</returns>
        public Point3d GetHomogenizedCentroid(out double SxHomog, out double SyHomog)
        {
            return ConcreteSectionHelper.GetHomogenizedCentroid(Area * Diameter / 2.0, Area * Diameter / 2.0,
                Rebars, ConcreteMaterial, Area, out SxHomog, out SyHomog);
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
            return ConcreteSectionHelper.GetHomogenizedCentroid(n, Rebars, Area * Diameter / 2.0, Area * Diameter / 2.0,
                Area, out SxHomog, out SyHomog);
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

        public virtual double CalculateN(ReinforcedConcreteRebar rebar)
        {
            return ConcreteSectionHelper.CalculateN(rebar, ConcreteMaterial);
        }

        public virtual double CalculateN(int rebar)
        {
            return ConcreteSectionHelper.CalculateN(rebar, Rebars, ConcreteMaterial);
        }

        #endregion

        #region Protected Methods

        protected ReinforcedConcreteRebar[] SetRadialRebars(double concreteCover, int numberOfRebars, IRebarSection rebarSection, double epsilonP = 0.0)
        {
            return ConcreteSectionHelper.SetRadialRebars(Centroid, Diameter, concreteCover, numberOfRebars, rebarSection, epsilonP);
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

        protected double CalculateJ11(double Jxx, double Jyy, double Jxy)
        {
            return ConcreteSectionHelper.CalculateJ11(Jxx, Jyy, Jxy);
        }

        protected double CalculateJ22(double Jxx, double Jyy, double Jxy)
        {
            return ConcreteSectionHelper.CalculateJ22(Jxx, Jyy, Jxy);
        }

        protected double CalculateAngle(double Jxx, double Jyy, double Jxy)
        {
            return ConcreteSectionHelper.CalculateAngle(Jxx, Jyy, Jxy);
        }


        protected Mesh GenerateMesh(double size = -1)
        {
            return ConcreteSectionHelper.GenerateMesh(Shape, size);
        }

        #endregion
    }
}
