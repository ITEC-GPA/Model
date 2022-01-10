using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Elements;
using GPC.Model.Materials;
using GPC.Utilities.Extensions;

namespace GPC.Model.Sections.Concrete
{
    [Serializable]
    public class ReinforcedConcreteSection : Section, IConcreteSection
    {

        protected readonly ShapeEx _shapeEx;
        protected readonly RebarCollection _rebars;


        public ShapeEx ShapeEx => _shapeEx;

        public IEnumerable<ReinforcedConcreteRebar> Rebars => _rebars;

        public ConcreteMaterial ConcreteMaterial => (ConcreteMaterial)_material;

        public override Shape2d Shape => _shapeEx;

        public double AreaRebars => _rebars.Select(i => i.Area).Sum();
        public int RebarsCount => _rebars.Count;


        #region Public Constructors

        protected ReinforcedConcreteSection(ReinforcedConcreteSection reinforcedConcreteSection)
            : base(reinforcedConcreteSection.Material, reinforcedConcreteSection.Name)
        {
            if (reinforcedConcreteSection is null)
            {
                throw new ArgumentNullException(nameof(reinforcedConcreteSection));
            }

            _shapeEx = reinforcedConcreteSection.ShapeEx;
            _rebars = new RebarCollection();

            SetMechanicalProperties();

        }

        public ReinforcedConcreteSection(ShapeEx shapeEx, string name = "")
            : base(shapeEx.Material, name)
        {
            _shapeEx = shapeEx ?? throw new ArgumentNullException(nameof(shapeEx));
            _rebars = new RebarCollection();

            SetMechanicalProperties();
        }

        protected ReinforcedConcreteSection(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            _shapeEx = (ShapeEx)info.GetValue("ShapeEx", typeof(ShapeEx));
            _rebars = (RebarCollection)info.GetValue("RebarCollection", typeof(RebarCollection));
        }

        #endregion


        #region Public Methods

        #region Rebars

        /// <inheritdoc cref="AddRebar(ReinforcedConcreteRebar, out int)"/>
        public bool AddRebar(ReinforcedConcreteRebar rebar)
        {
            return AddRebar(rebar, out _);
        }

        /// <summary>
        /// Add a <paramref name="rebar"/> into the section.
        /// </summary>
        /// <remarks>
        /// <para>If a rebar with the same id already exist in the collection, <paramref name="rebar"/> will replace that rebar</para>
        /// <para>If <paramref name="rebar"/> ID is lower than 1, this will be replaced with the maximum id + 1</para>
        /// </remarks>
        /// <returns>The <see cref="ModelObjectId.Id"/> of the rebar</returns>
        public bool AddRebar(ReinforcedConcreteRebar rebar, out int id)
        {
            if (_rebars.Contains(rebar))
            {
                // stessa posizione, torniamo falso

                id = IDUNASSIGNED;

                return false;
            }
            else
            {
                if (rebar.Id < 1)
                    rebar.Id = _rebars.MaxId + 1;

                _rebars.Add(rebar);
                id = rebar.Id;

                return true;
            }
        }

        /// <inheritdoc cref="AddRebar(ReinforcedConcreteRebar, out int)"/>
        public bool[] AddRebars(IEnumerable<ReinforcedConcreteRebar> rebars, out int[] ids)
        {
            List<int> id = new List<int>();
            List<bool> bools = new List<bool>();

            foreach (var item in rebars)
            {
                bools.Add(AddRebar(item, out int _id));
                id.Add(_id);
            }

            ids = id.ToArray();
            return bools.ToArray();
        }

        /// <inheritdoc cref="AddRebar(ReinforcedConcreteRebar, out int)"/>
        public bool[] AddRebars(IEnumerable<ReinforcedConcreteRebar> rebars)
        {
            return AddRebars(rebars, out _);
        }

        /// <inheritdoc cref="UniqueIdCollection{T}.Remove(T)"/>
        public bool RemoveRebar(ReinforcedConcreteRebar rebar)
        {
            return _rebars.Remove(rebar);
        }

        /// <inheritdoc cref="UniqueIdCollection{T}.Remove(int)"/>
        public bool RemoveRebar(int rebarId)
        {
            return _rebars.Remove(rebarId);
        }

        /// <inheritdoc cref="UniqueIdCollection{T}.RemoveRange(IEnumerable{T})"/>
        public bool RemoveRebars(IEnumerable<ReinforcedConcreteRebar> rebars)
        {
            return _rebars.RemoveRange(rebars);
        }
                
        public bool ClearRebars()
        {
            try
            {
                _rebars.Clear();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <returns><see langword="null"/> if item not found</returns>
        /// <inheritdoc cref="UniqueIdCollection{T}.GetById(int)"/>
        public ReinforcedConcreteRebar GetRebarById(int rebarId)
        {
            try
            {
                return _rebars.GetById(rebarId);
            }
            catch
            {
                return null;
            }
        }

        public ReinforcedConcreteRebar[] GetRebars()
        {
            return _rebars.ToArray();
        }
                
        /// <inheritdoc cref="GetRebarById(int)"/>
        public ReinforcedConcreteRebar[] GetRebarById(IEnumerable<int> rebarIds)
        {
            try
            {
                List<ReinforcedConcreteRebar> rebars = new List<ReinforcedConcreteRebar>();

                foreach (var item in rebarIds)
                {
                    rebars.Add(GetRebarById(item));
                }

                return rebars.ToArray();
            }
            catch
            {
                return null;
            }
        }

		#endregion


		#region Concrete Mechanical properties

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
            ConcreteSectionHelper.CalculateHomogeneizedInertiaMoments(_rebars.ToArray(), Centroid, centroidH, ConcreteMaterial, Jxx, Jyy, Jxy, Area, out var JxxH, out var JyyH, out var JxyH, out var JpH);

            var J11H = SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
            var J22H = SectionHelper.CalculateJ22(JxxH, JyyH, JxyH);
            double angleX = SectionHelper.CalculateAngle(J11H, J22H, JxxH, JyyH, JxyH);

            return (GetHomogenizedArea(), SxH, SyH, centroidH, JxxH, JyyH, JxyH, JpH, J11H, J22H, angleX);
        }

        /// <summary>
        /// The centroid of the homogenized section with default value of homogenized factor n
        /// </summary>
        /// <param name="SxHomog">The first moment of area respect X-Axis</param>
        /// <param name="SyHomog">The first moment of area respect Y-Axis</param>
        /// <returns>The centroid</returns>
        public Point2d GetHomogenizedCentroid(out double SxHomog, out double SyHomog)
        {
            return ConcreteSectionHelper.GetHomogenizedCentroid(Mesh, _rebars.ToArray(), ConcreteMaterial, 
                Area, out SxHomog, out SyHomog);
        }

        /// <summary>
        /// The homogenized area with default value of homogenized factor n
        /// </summary>
        /// <returns>The homogenized area</returns>
        public double GetHomogenizedArea()
        {
            return ConcreteSectionHelper.GetHomogenizedArea(_rebars.ToArray(), ConcreteMaterial, Area);
        }

        public double GetHomogeneizedJ11()
        {
            return ConcreteSectionHelper.GetHomogeneizedJ11(Mesh, Centroid, _rebars.ToArray(), ConcreteMaterial, 
                Area, Jxx, Jyy, Jxy);
        }

        public double GetHomogeneizedJ22()
        {
            return ConcreteSectionHelper.GetHomogeneizedJ22(Mesh, Centroid, _rebars.ToArray(), ConcreteMaterial, 
                Area, Jxx, Jyy, Jxy);
        }

		#region Phi factor

		/// <summary>
		/// Return all homogenized mechanical properties with homogeneized factor <paramref name="phi"/>
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
            GetHomogeneizedMechanicalProperties(double phi)
        {
            if(_rebars.Count > 0)
			{
                Point2d centroidH = GetHomogenizedCentroid(phi, out var SxH, out var SyH);

                // NOTA: ci siamo ricondotti a momenti d'inerzia rispetto al baricentro della sezione di solo calcestruzzo
                ConcreteSectionHelper.CalculateHomogeneizedInertiaMoments(phi, ConcreteMaterial, _rebars.ToArray(), Centroid,
                    centroidH, Jxx, Jyy, Jxy, Area, out var JxxH, out var JyyH, out var JxyH, out var JpH);

                double J11H = SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
                double J22H = SectionHelper.CalculateJ22(JxxH, JyyH, JxyH);
                double angleX = SectionHelper.CalculateAngle(J11H, J22H, JxxH, JyyH, JxyH);

                return (GetHomogenizedArea(phi), SxH, SyH, centroidH, JxxH, JyyH, JxyH, JpH, J11H, J22H, angleX);
            }
            else
			{
                return (0, 0, 0, new Point2d(), 0, 0, 0, 0, 0, 0, 0);
			}
        }

        /// <summary>
        /// The centroid of the homogenized section with homogenized factor <paramref name="phi"/>
        /// </summary>
        /// <param name="phi">The homogenized factor</param>
        /// <param name="SxHomog">The first moment of area respect X-Axis</param>
        /// <param name="SyHomog">The first moment of area respect Y-Axis</param>
        /// <returns></returns>
        public Point2d GetHomogenizedCentroid(double phi, out double SxHomog, out double SyHomog)
        {
            return ConcreteSectionHelper.GetHomogenizedCentroid(phi, Mesh, _rebars.ToArray(), ConcreteMaterial,
                Area, out SxHomog, out SyHomog);
        }

        /// <summary>
        /// The homogenized area with homogenized factor <paramref name="phi"/>
        /// </summary>
        /// <param name="phi"></param>
        /// <returns>The homogenized area</returns>
        public double GetHomogenizedArea(double phi)
        {
            return ConcreteSectionHelper.GetHomogenizedArea(phi, _rebars.ToArray(), ConcreteMaterial, Area);
        }

        public double GetHomogeneizedJ11(double phi)
        {
            return ConcreteSectionHelper.GetHomogeneizedJ11(phi, Centroid, Mesh, _rebars.ToArray(), ConcreteMaterial, 
                Area, Jxx, Jyy, Jxy);
        }

        public double GetHomogeneizedJ22(double phi)
        {
            return ConcreteSectionHelper.GetHomogeneizedJ22(phi, Centroid, Mesh, _rebars.ToArray(), ConcreteMaterial, 
                Area, Jxx, Jyy, Jxy);
        }

        #endregion

        #endregion

        public ReinforcedConcreteSection ToReinforcedConcreteSection()
        {
            return new ReinforcedConcreteSection(this);
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
        protected override void SetMechanicalProperties()
        {
            _area = CalculateArea();

            ConcreteSectionHelper.CalculateStaticMoments(Mesh, out double Sx, out double Sy);

            _centroid = SectionHelper.CalculateCentroid(Sx, Sy, _area);

            ConcreteSectionHelper.CalculateInertiaMoments(Mesh, _centroid, out double Jxx, out double Jyy, out double Jxy, out double _);

            _j11 = SectionHelper.CalculateJ11(Jxx, Jyy, Jxy);
            _j22 = SectionHelper.CalculateJ22(Jxx, Jyy, Jxy);
            _jxx = Jxx;
            _jyy = Jyy;
            _jxy = Jxy;
            _jp = _jxx + _jyy;
            _angleX1 = SectionHelper.CalculateAngle(_j11, _j22, Jxx, Jyy, Jxy);

            _jw = 0; //TODO: implementare metodi di calcolo della sezione calcolo JW/JT
            _jt = 0; //TODO: implementare metodi di calcolo della sezione calcolo JW/JT
            _shearCenter = _centroid; //TODO: Implementare calcolo shear center

            _wel1Max = CalculateWel1Max();
            _wel1Min = CalculateWel1Min();
            _wel2Max = CalculateWel2Max();
            _wel2Min = CalculateWel2Min();
            _welXMax = CalculateWelXMax();
            _welXMin = CalculateWelXMin();
            _welYMax = CalculateWelYMax();
            _welYMin = CalculateWelYMin();
            _wpl1 = CalculateWpl1();
            _wpl2 = CalculateWpl2();

            _isSymmetricAlongXLocalAxis = false; //TODO calcolare se � simmetrica
            _isSymmetricAlongYLocalAxis = false;
        }

        protected override double CalculateArea()
        {
            return ShapeEx.GetArea();
        }

        public virtual double CalculateN(ReinforcedConcreteRebar rebar)
        {
            return ConcreteSectionHelper.CalculateN(rebar, ConcreteMaterial);
        }

        /// <returns>0 if <paramref name="rebarId"/> not found</returns>
        public virtual double CalculateN(int rebarId)
        {

            try
            {
                ReinforcedConcreteRebar rebar = _rebars.GetById(rebarId);
                return ConcreteSectionHelper.CalculateN(rebar, ConcreteMaterial);
            }
            catch (KeyNotFoundException)
            {
                return 0;
            }
        }

        protected override double CalculateWpl2()
        {
            return 0;
        }

        protected override double CalculateWpl1()
        {
            return 0;
        }

        protected override double CalculateWel2Max()
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

            return _j22 / Math.Abs(dmaxConcrete);
        }

        protected override double CalculateWel2Min()
        {
            double cosTeta = Math.Cos(_angleX1 + Math.PI / 2.0);
            double sinTeta = Math.Sin(_angleX1 + Math.PI / 2.0);

            double dmaxConcrete = double.MaxValue;

            for (int c = 0; c < ShapeEx.Fill.Count; c++)
            {
                double w1 = (ShapeEx.Fill[c].Y - Centroid.Y) * cosTeta - (ShapeEx.Fill[c].X - Centroid.X) * sinTeta;

                if (w1 <= dmaxConcrete)
                {
                    dmaxConcrete = w1;
                }
            }

            return _j22 / Math.Abs(dmaxConcrete);
        }

        protected override double CalculateWel1Max()
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

            return _j11 / Math.Abs(dmaxConcrete);
        }

        protected override double CalculateWel1Min()
        {
            double cosTeta = Math.Cos(_angleX1);
            double sinTeta = Math.Sin(_angleX1);

            double dminConcrete = double.MaxValue;

            for (int c = 0; c < ShapeEx.Fill.Count; c++)
            {
                double w1 = (ShapeEx.Fill[c].Y - Centroid.Y) * cosTeta - (ShapeEx.Fill[c].X - Centroid.X) * sinTeta;

                if (w1 <= dminConcrete)
                {
                    dminConcrete = w1;
                }
            }

            return _j11 / Math.Abs(dminConcrete);
        }

        protected override double CalculateWelYMax()
        {
            double cosTeta = Math.Cos(Math.PI / 2.0);
            double sinTeta = Math.Sin(Math.PI / 2.0);

            double dmaxConcrete = double.MinValue;

            for (int c = 0; c < ShapeEx.Fill.Count; c++)
            {
                double w1 = (ShapeEx.Fill[c].Y - Centroid.Y) * cosTeta - (ShapeEx.Fill[c].X - Centroid.X) * sinTeta;

                if (w1 >= dmaxConcrete)
                {
                    dmaxConcrete = w1;
                }
            }

            return _j22 / Math.Abs(dmaxConcrete);
        }

        protected override double CalculateWelYMin()
        {
            double cosTeta = Math.Cos(Math.PI / 2.0);
            double sinTeta = Math.Sin(Math.PI / 2.0);

            double dmaxConcrete = double.MaxValue;

            for (int c = 0; c < ShapeEx.Fill.Count; c++)
            {
                double w1 = (ShapeEx.Fill[c].Y - Centroid.Y) * cosTeta - (ShapeEx.Fill[c].X - Centroid.X) * sinTeta;

                if (w1 <= dmaxConcrete)
                {
                    dmaxConcrete = w1;
                }
            }

            return _j22 / Math.Abs(dmaxConcrete);
        }

        protected override double CalculateWelXMax()
        {
            double cosTeta = Math.Cos(0.0);
            double sinTeta = Math.Sin(0.0);

            double dmaxConcrete = double.MinValue;

            for (int c = 0; c < ShapeEx.Fill.Count; c++)
            {
                double w1 = (ShapeEx.Fill[c].Y - Centroid.Y) * cosTeta - (ShapeEx.Fill[c].X - Centroid.X) * sinTeta;

                if (w1 >= dmaxConcrete)
                {
                    dmaxConcrete = w1;
                }
            }

            return _j11 / Math.Abs(dmaxConcrete);
        }

        protected override double CalculateWelXMin()
        {
            double cosTeta = Math.Cos(0.0);
            double sinTeta = Math.Sin(0.0);

            double dminConcrete = double.MaxValue;

            for (int c = 0; c < ShapeEx.Fill.Count; c++)
            {
                double w1 = (ShapeEx.Fill[c].Y - Centroid.Y) * cosTeta - (ShapeEx.Fill[c].X - Centroid.X) * sinTeta;

                if (w1 <= dminConcrete)
                {
                    dminConcrete = w1;
                }
            }

            return _j11 / Math.Abs(dminConcrete);
        }

        #endregion


        #region Equals, hascode, operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ShapeEx", _shapeEx, typeof(ShapeEx));
            info.AddValue("ReinforcedConcreteRebar", _rebars, typeof(ReinforcedConcreteRebar[]));
        }

        public override bool Equals(object obj)
        {
            return obj is ReinforcedConcreteSection section &&
                   base.Equals(obj) &&
                   _shapeEx.Equals(section._shapeEx) &&
                   _rebars.ScrambledEquals(section._rebars);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _shapeEx.GetHashCode();
                hashCode = hashCode * -17 + _rebars.GetHashCodeScrambled();
                return hashCode;
            }
        }

        public static bool operator ==(ReinforcedConcreteSection left, ReinforcedConcreteSection right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(ReinforcedConcreteSection left, ReinforcedConcreteSection right)
        {
            return !(left == right);
        }

        #endregion
    }
}
