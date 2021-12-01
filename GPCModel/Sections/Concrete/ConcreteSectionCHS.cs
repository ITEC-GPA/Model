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

        protected readonly UniqueIdCollection<ReinforcedConcreteRebar> _rebars;

        #endregion

        #region Properties

        public IEnumerable<ReinforcedConcreteRebar> Rebars => _rebars;

        public ConcreteMaterial ConcreteMaterial => (ConcreteMaterial)_material;

        public double AreaRebars => _rebars.Select(i => i.Area).Sum();

        #endregion

        #region Public Constructors

        public ConcreteSectionCHS(double diameter, double thickness, ConcreteMaterial material, string name = "")
            : base(diameter, thickness, material, name)
        {
            _rebars = new UniqueIdCollection<ReinforcedConcreteRebar>();
            _mesh = GenerateMesh();
        }

        public ConcreteSectionCHS(SectionCHS sectionCHS)
            : base(sectionCHS)
        {
            _rebars = new UniqueIdCollection<ReinforcedConcreteRebar>();
            _mesh = GenerateMesh();

            if (sectionCHS.Material.GetType() != typeof(ConcreteMaterial))
                throw new ArgumentException("Material must be a ConcreteMaterial");
        }

        #endregion

        #region Public Methods

        #region Rebars
        public bool AddRadialRebars(double externalConcreteCover, int externalNumberOfRebars, IRebarSection externalRebarSection,
                                  double internalConcreteCover, int internalNumberOfRebars, IRebarSection internalRebarSection, 
                                  double externalEpsilonP = 0.0, double internalEpsilonP = 0.0)
        {

            bool retVal = false;
            if (externalRebarSection != null)
            {
                retVal = _rebars.AddRange(ConcreteSectionHelper.SetRadialRebars(Diameter, externalConcreteCover, externalNumberOfRebars, externalRebarSection, Centroid, externalEpsilonP));
            }

            if (!retVal)
                return retVal;

            if (internalRebarSection != null)
            {
                retVal = _rebars.AddRange(ConcreteSectionHelper.SetRadialRebars(Diameter, internalConcreteCover, internalNumberOfRebars, internalRebarSection, Centroid, internalEpsilonP));
            }
            
            return retVal;
        }

        public bool AddRadialRebars(double concreteCover, int numberOfRebars, IRebarSection rebarSection, double epsilonP = 0.0)
        {
            return _rebars.AddRange(ConcreteSectionHelper.SetRadialRebars(Diameter, concreteCover, numberOfRebars, rebarSection, Centroid, epsilonP));
        }


        public bool AddRebar(ReinforcedConcreteRebar rebar)
        {
            return _rebars.Add(rebar);
        }

        public bool AddRebars(IEnumerable<ReinforcedConcreteRebar> rebars)
        {
            return _rebars.AddRange(rebars);
        }

        public bool RemoveRebar(ReinforcedConcreteRebar rebar)
        {
            return _rebars.Remove(rebar);
        }

        public bool RemoveRebar(int rebarId)
        {
            return _rebars.Remove(rebarId);
        }

        public bool RemoveRebars(IEnumerable<ReinforcedConcreteRebar> rebars)
        {
            return _rebars.RemoveRange(rebars);
        }

        public ReinforcedConcreteRebar GetRebarById(int rebarId)
        {
            return _rebars.GetElementById(rebarId);
        }
        #endregion
        #region MechanicalProperties

        /// <summary>
        /// Return all homogenized mechanical properties with default value of homogenized factor n
        /// </summary>
        /// <returns>
        /// <para>areaH: The homogeneized area.</para>
        /// <para>SxHThe: first moment of area calculated respect input X-axis of the homogeneized section.</para>
        /// <para>SyHThe: first moment of area calculated respect input Y-axis of the homogeneized section.</para>
        /// <para>centroidH: The centroid of homogeneized section.</para>
        /// <para>JxxH: The first moment of area calculated respect X-axis passing throw the centroid of the homogeneized section.</para>
        /// <para>JyyH: The first moment of area calculated respect Y-axis passing throw the centroid of the homogeneized section.</para>
        /// <para>J11H: The first moment of area calculated respect the first principal axis 
        /// passing throw the centroid of only concrete section of the homogeneized section</para>
        /// <para>J22H: The first moment of area calculated respect the second principal axis 
        /// passing throw the centroid of only concrete section of the homogeneized section</para>
        /// <para>AngleX: The angle of rotation of the principal axis respect the X-Axis</para>
        /// </returns>
        public (double areaH, double SxH, double SyH, Point2d centroidH, double JxxH, double JyyH, double JxyH, double JpH, double J11H, double J22H, double angleX)
            GetHomogeneizedMechanicalProperties()
        {
            Point2d centroidH = GetHomogenizedCentroid(out var SxH, out var SyH);

            // NOTA: ci siamo ricondotti a momenti d'inerzia rispetto al baricentro della sezione di solo calcestruzzo
            ConcreteSectionHelper.CalculateHomogeneizedInertiaMoments(_rebars.ToArray(), Centroid, centroidH, ConcreteMaterial, Jxx, Jyy, Jxy, Area, out var JxxH, out var JyyH, out var JxyH, out var JpH);

            double J11H = SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
            double J22H = SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
            double angleX = SectionHelper.CalculateAngle(JxxH, JyyH, JxyH);

            return (GetHomogenizedArea(), SxH, SyH, centroidH, JxxH, JyyH, JxyH, JpH, J11H, J22H, angleX);
        }

        /// <summary>
        /// Return all homogenized mechanical properties with homogeneized factor <paramref name="n"/>
        /// </summary>
        /// <returns>
        /// <para>areaH: The homogeneized area.</para>
        /// <para>SxHThe: first moment of area calculated respect input X-axis of the homogeneized section.</para>
        /// <para>SyHThe: first moment of area calculated respect input Y-axis of the homogeneized section.</para>
        /// <para>centroidH: The centroid of homogeneized section.</para>
        /// <para>JxxH: The first moment of area calculated respect X-axis passing throw the centroid of the homogeneized section.</para>
        /// <para>JyyH: The first moment of area calculated respect Y-axis passing throw the centroid of the homogeneized section.</para>
        /// <para>J11H: The first moment of area calculated respect the first principal axis 
        /// passing throw the centroid of only concrete section of the homogeneized section</para>
        /// <para>J22H: The first moment of area calculated respect the second principal axis 
        /// passing throw the centroid of only concrete section of the homogeneized section</para>
        /// <para>AngleX: The angle of rotation of the principal axis respect the X-Axis</para>
        /// </returns>
        public (double areaH, double SxH, double SyH, Point2d centroidH, double JxxH, double JyyH, double JxyH, double JpH, double J11H, double J22H, double angleX)
            GetHomogeneizedMechanicalProperties(double n)
        {

            Point2d centroidH = GetHomogenizedCentroid(n, out var SxH, out var SyH);

            // NOTA: ci siamo ricondotti a momenti d'inerzia rispetto al baricentro della sezione di solo calcestruzzo
            ConcreteSectionHelper.CalculateHomogeneizedInertiaMoments(n, _rebars.ToArray(), Centroid, centroidH, Jxx, Jyy, Jxy, Area, out var JxxH, out var JyyH, out var JxyH, out var JpH);

            double J11H = SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
            double J22H = SectionHelper.CalculateJ22(JxxH, JyyH, JxyH);
            double angleX = SectionHelper.CalculateAngle(JxxH, JyyH, JxyH);

            return (GetHomogenizedArea(n), SxH, SyH, centroidH, JxxH, JyyH, JxyH, JpH, J11H, J22H, angleX);
        }


        /// <summary>
        /// The centroid of the homogenized section with default value of homogenized factor n
        /// </summary>
        /// <param name="SxHomog">The first moment of area respect X-Axis</param>
        /// <param name="SyHomog">The first moment of area respect Y-Axis</param>
        /// <returns>The centroid</returns>
        public Point2d GetHomogenizedCentroid(out double SxHomog, out double SyHomog)
        {
            return ConcreteSectionHelper.GetHomogenizedCentroid(Area * Diameter / 2.0, Area * Diameter / 2.0, _rebars.ToArray(), ConcreteMaterial, Area, out SxHomog, out SyHomog);
        }


        /// <summary>
        /// The centroid of the homogenized section with homogenized factor <paramref name="n"/>
        /// </summary>
        /// <param name="n">The homogenized factor</param>
        /// <param name="SxHomog">The first moment of area respect X-Axis</param>
        /// <param name="SyHomog">The first moment of area respect Y-Axis</param>
        /// <returns></returns>
        public Point2d GetHomogenizedCentroid(double n, out double SxHomog, out double SyHomog)
        {
            return ConcreteSectionHelper.GetHomogenizedCentroid(n, _rebars.ToArray(), Area * Diameter / 2.0, Area * Diameter / 2.0, Area, out SxHomog, out SyHomog);
        }

        /// <summary>
        /// The homogenized area with default value of homogenized factor n
        /// </summary>
        /// <returns>The homogenized area</returns>
        public double GetHomogenizedArea()
        {
            return ConcreteSectionHelper.GetHomogenizedArea(_rebars.ToArray(), ConcreteMaterial, Area);
        }

        /// <summary>
        /// The homogenized area with homogenized factor <paramref name="n"/>
        /// </summary>
        /// <param name="n"></param>
        /// <returns>The homogenized area</returns>
        public double GetHomogenizedArea(double n)
        {
            return ConcreteSectionHelper.GetHomogenizedArea(n, _rebars.ToArray(), Area);
        }

        public double GetHomogeneizedJ11(double n)
        {
            return ConcreteSectionHelper.GetHomogeneizedJ11(n, Centroid, Mesh, _rebars.ToArray(), Area, Jxx, Jyy, Jxy);
        }

        public double GetHomogeneizedJ11()
        {
            return ConcreteSectionHelper.GetHomogeneizedJ11(Mesh, Centroid, _rebars.ToArray(), ConcreteMaterial, Area, Jxx, Jyy, Jxy);
        }

        public double GetHomogeneizedJ22(double n)
        {
            return ConcreteSectionHelper.GetHomogeneizedJ22(n, Centroid, Mesh, _rebars.ToArray(), Area, Jxx, Jyy, Jxy);
        }

        public double GetHomogeneizedJ22()
        {
            return ConcreteSectionHelper.GetHomogeneizedJ22(Mesh, Centroid, _rebars.ToArray(), ConcreteMaterial, Area, Jxx, Jyy, Jxy);
        }

        public virtual double CalculateN(ReinforcedConcreteRebar rebar)
        {
            return ConcreteSectionHelper.CalculateN(rebar, ConcreteMaterial);
        }

        public virtual double CalculateN(int rebar)
        {
            return ConcreteSectionHelper.CalculateN(_rebars.GetElementById(rebar), ConcreteMaterial);
        }
        #endregion


        public ReinforcedConcreteSection ToReinforcedConcreteSection()
        {
            var section = new ReinforcedConcreteSection(new ShapeEx(GetShape(), ConcreteMaterial), Name);
            section.AddRebars(_rebars);

            return section;
        }

        #endregion

    }
}
