using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Elements;
using GPC.Model.Materials;
using GPC.Model.Sections.Rebar;

namespace GPC.Model.Sections.Concrete
{
    public class ConcreteSectionCircular : SectionCircular, IConcreteSection
    {

        protected ReinforcedConcreteRebar[] _rebars;


        public ReinforcedConcreteRebar[] Rebars => _rebars;

        public ConcreteMaterial ConcreteMaterial => (ConcreteMaterial)_material;

        public double AreaRebars => _rebars.Select(i => i.Area).Sum();


        public ConcreteSectionCircular(double diameter, ConcreteMaterial material, ReinforcedConcreteRebar[] rebars, string name = "")
            : base(diameter, material, name)
        {
            _rebars = rebars;
            _mesh = GenerateMesh();
        }

        public ConcreteSectionCircular(double diameter, ConcreteMaterial material, double concreteCover, int numberOfRebars, IRebarSection rebarSection, double epsilonP = 0.0, string name = "")
            : base(diameter, material, name)
        {
            _rebars = ConcreteSectionHelper.SetRadialRebars(Diameter, concreteCover, numberOfRebars, rebarSection, Centroid, epsilonP);
            _mesh = GenerateMesh();
        }


        #region Protected Methods


        public virtual double CalculateN(ReinforcedConcreteRebar rebar)
        {
            return ConcreteSectionHelper.CalculateN(rebar, ConcreteMaterial);
        }

        public virtual double CalculateN(int rebar)
        {
            return ConcreteSectionHelper.CalculateN(rebar, Rebars, ConcreteMaterial);
        }

        #endregion

        #region Public Methods


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
            ConcreteSectionHelper.CalculateHomogeneizedInertiaMoments(Rebars, Centroid, centroidH, ConcreteMaterial, Jxx, Jyy, Jxy, Area, out var JxxH, out var JyyH, out var JxyH, out var JpH);

            double J11H = SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
            double J22H = SectionHelper.CalculateJ22(JxxH, JyyH, JxyH);
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
            ConcreteSectionHelper.CalculateHomogeneizedInertiaMoments(n, Rebars, Centroid, centroidH, Jxx, Jyy, Jxy, Area, out var JxxH, out var JyyH, out var JxyH, out var JpH);

            double J11H = SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
            double J22H = SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
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
            return ConcreteSectionHelper.GetHomogenizedCentroid(Area * Diameter / 2.0, Area * Diameter / 2.0, Rebars, ConcreteMaterial, Area, out SxHomog, out SyHomog);
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
            return ConcreteSectionHelper.GetHomogenizedCentroid(n, Rebars, Area * Diameter / 2.0, Area * Diameter / 2.0, Area, out SxHomog, out SyHomog);
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

        public ReinforcedConcreteSection ToReinforcedConcreteSection()
        {
            return new ReinforcedConcreteSection(new ShapeEx(GetShape(), ConcreteMaterial), Rebars, Name);
        }


        #endregion


    }
}


