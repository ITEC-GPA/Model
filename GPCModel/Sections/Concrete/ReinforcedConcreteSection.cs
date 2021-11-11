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

        protected readonly ShapeEx _shapeEx;
        protected readonly ReinforcedConcreteRebar[] _rebars;


        public ShapeEx ShapeEx => _shapeEx;

        public ReinforcedConcreteRebar[] Rebars => _rebars;

        public ConcreteMaterial ConcreteMaterial => (ConcreteMaterial)_material;

        public override Shape2d Shape => _shapeEx;

        public double AreaRebars => _rebars.Select(i => i.Area).Sum();


        #region Public Constructors

        protected ReinforcedConcreteSection(ReinforcedConcreteSection reinforcedConcreteSection)
            : base(reinforcedConcreteSection.Name)
        {
            _shapeEx = reinforcedConcreteSection.ShapeEx;
            _rebars = reinforcedConcreteSection.Rebars;
            _material = reinforcedConcreteSection.Material;

            SetMechanicalProperties();
        }

        public ReinforcedConcreteSection(ShapeEx shapeEx, ReinforcedConcreteRebar[] rebars, string name = "")
            : base(name)
        {
            _shapeEx = shapeEx ?? throw new ArgumentNullException(nameof(shapeEx));
            _rebars = rebars ?? throw new ArgumentNullException(nameof(rebars));
            _material = shapeEx.Material;


            SetMechanicalProperties();
        }

        public ReinforcedConcreteSection(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            _shapeEx = (ShapeEx)info.GetValue("ShapeEx", typeof(ShapeEx));
            _rebars = (ReinforcedConcreteRebar[])info.GetValue("ReinforcedConcreteRebar", typeof(ReinforcedConcreteRebar[]));
        }

        #endregion

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ShapeEx", _shapeEx);
            info.AddValue("ReinforcedConcreteRebar", _rebars);
        }


        #region Public Methods

        public ReinforcedConcreteSection ToReinforcedConcreteSection()
        {
            return new ReinforcedConcreteSection(this);
        }


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

            var centroidH = GetHomogenizedCentroid(out var SxH, out var SyH);

            // NOTA: ci siamo ricondotti a momenti d'inerzia rispetto al baricentro della sezione di solo calcestruzzo
            ConcreteSectionHelper.CalculateHomogeneizedInertiaMoments(Rebars, Centroid, centroidH, ConcreteMaterial, Jxx, Jyy, Jxy, Area, out var JxxH, out var JyyH, out var JxyH, out var JpH);

            var J11H = SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
            var J22H = SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
            var angleX = SectionHelper.CalculateAngle(JxxH, JyyH, JxyH);

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

            var centroidH = GetHomogenizedCentroid(out var SxH, out var SyH);

            // NOTA: ci siamo ricondotti a momenti d'inerzia rispetto al baricentro della sezione di solo calcestruzzo
            ConcreteSectionHelper.CalculateHomogeneizedInertiaMoments(Rebars, Centroid, centroidH, ConcreteMaterial, Jxx, Jyy, Jxy, Area, out var JxxH, out var JyyH, out var JxyH, out var JpH);

            var J11H = SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
            var J22H = SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
            var angleX = SectionHelper.CalculateAngle(JxxH, JyyH, JxyH);

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
            return ConcreteSectionHelper.GetHomogenizedCentroid(Mesh, Rebars, ConcreteMaterial, Area, out SxHomog, out SyHomog);
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


        protected override Shape2d GetShape()
        {
            return _shapeEx;
        }


        /// <summary>
        /// Internal method to set the mechanical properties to the section
        /// </summary>
        protected virtual void SetMechanicalProperties()
        {
            _area = CalculateArea();

            ConcreteSectionHelper.CalculateStaticMoments(Mesh, out double Sx, out double Sy);

            _centroid = SectionHelper.CalculateCentroid(Sx, Sy, _area);

            ConcreteSectionHelper.CalculateInertiaMoments(Mesh, _centroid, out double Jxx, out double Jyy, out double Jxy, out double Jp);

            _jxx = Jxx;
            _jyy = Jyy;
            _jxy = Jxy;
            _angleX1 = SectionHelper.CalculateAngle(Jxx, Jyy, Jxy); ;
            _j11 = SectionHelper.CalculateJ11(Jxx, Jyy, Jxy);
            _j22 = SectionHelper.CalculateJ22(Jxx, Jyy, Jxy);


            _jw = 0; //TODO: implementare metodi di calcolo della sezione calcolo JW/JT
            _jt = 0; //TODO: implementare metodi di calcolo della sezione calcolo JW/JT

            _shearCenter = _centroid; //TODO: Implementare calcolo shear center

            _wel1 = CalculateWel1();
            _wel2 = CalculateWel2();
            _wpl1 = CalculateWpl1();
            _wpl2 = CalculateWpl2();
        }

        protected double CalculateArea()
        {
            return ShapeEx.GetArea();
        }


        public virtual double CalculateN(ReinforcedConcreteRebar rebar)
        {
            return ConcreteSectionHelper.CalculateN(rebar, ConcreteMaterial);
        }

        public virtual double CalculateN(int rebar)
        {
            return ConcreteSectionHelper.CalculateN(rebar, Rebars, ConcreteMaterial);
        }

        protected double CalculateWpl2()
        {
            return 0;
        }

        protected double CalculateWpl1()
        {
            return 0;
        }

        protected double CalculateWel2()
        {
            double cosTeta = Math.Cos(_angleX1 + Math.PI / 2.0);
            double sinTeta = Math.Sin(_angleX1 + Math.PI / 2.0);

            double dmaxConcrete = double.MinValue;

            for (int c = 0; c < ShapeEx.Fill.Count; c++)
            {
                double w1 = (ShapeEx.Fill[c].Y - Centroid.Y) * cosTeta - (ShapeEx.Fill[c].X - Centroid.X) * sinTeta;

                if (w1 >= dmaxConcrete)
                {
                    dmaxConcrete = w1;
                }
            }

            return _j22 / dmaxConcrete;
        }

        protected double CalculateWel1()
        {
            double cosTeta = Math.Cos(_angleX1);
            double sinTeta = Math.Sin(_angleX1);

            double dmaxConcrete = double.MinValue;

            for (int c = 0; c < ShapeEx.Fill.Count; c++)
            {
                double w1 = (ShapeEx.Fill[c].Y - Centroid.Y) * cosTeta - (ShapeEx.Fill[c].X - Centroid.X) * sinTeta;

                if (w1 >= dmaxConcrete)
                {
                    dmaxConcrete = w1;
                }
            }


            return _j11 / dmaxConcrete;
        }


        #endregion

    }
}
