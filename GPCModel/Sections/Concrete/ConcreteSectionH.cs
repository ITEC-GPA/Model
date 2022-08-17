using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Elements;
using GPC.Model.Fem.Materials;
using GPC.Model.Materials;

namespace GPC.Model.Sections.Concrete
{
	public class ConcreteSectionH : SectionH, IConcreteSection
    {
        #region Variables

        protected readonly UniqueIdCollection<ReinforcedConcreteRebar> _rebars;

        #endregion

        #region Properties

        public IEnumerable<ReinforcedConcreteRebar> Rebars => _rebars;

        public ConcreteMaterial ConcreteMaterial => (ConcreteMaterial)_material;

        public double AreaRebars => _rebars.Select(i => i.Area).Sum();

        public int RebarsCount => _rebars.Count;

        #endregion

        #region Public Constructors

        public ConcreteSectionH(double height, double thicknessWeb, double topFlangeLength, double topFlangeThickness, double bottomFlangeLength,
            double bottomFlangeThickness, ConcreteMaterial material, string name = "")
            : base(height, thicknessWeb, topFlangeLength, topFlangeThickness, bottomFlangeLength, bottomFlangeThickness, material, name)
        {
            _rebars = new UniqueIdCollection<ReinforcedConcreteRebar>();
        }

        public ConcreteSectionH(SectionH section)
            : this(section.Height, section.ThicknessWeb, section.LenghtTopFlange, section.ThicknessTopFlange,
                  section.LenghtBottomFlange, section.ThicknessBottomFlange, (ConcreteMaterial)section.Material, section.Name)
        {
            if (section.Material.GetType() != ConcreteMaterial.GetType())
                throw new ArgumentException("Material must be a ConcreteMaterial");
        }

        protected ConcreteSectionH(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("ConcreteSectionHVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            _rebars = (UniqueIdCollection<ReinforcedConcreteRebar>)info.GetValue("Rebars", typeof(UniqueIdCollection<ReinforcedConcreteRebar>));
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

        #region Mechanical properties

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
            ConcreteSectionHelper.CalculateHomogeneizedInertiaMoments(_rebars.ToArray(), Centroid, centroidH, ConcreteMaterial,
                Jxx, Jyy, Jxy, Area, out var JxxH, out var JyyH, out var JxyH, out var JpH);

            double J11H = SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
            double J22H = SectionHelper.CalculateJ22(JxxH, JyyH, JxyH);
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
            Point2d centroidH = GetHomogenizedCentroid(phi, out var SxH, out var SyH);

            // NOTA: ci siamo ricondotti a momenti d'inerzia rispetto al baricentro della sezione di solo calcestruzzo
            ConcreteSectionHelper.CalculateHomogeneizedInertiaMoments(phi, ConcreteMaterial, _rebars.ToArray(), Centroid, centroidH, Jxx, Jyy, Jxy, Area,
                out var JxxH, out var JyyH, out var JxyH, out var JpH);

            double J11H = SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
            double J22H = SectionHelper.CalculateJ22(JxxH, JyyH, JxyH);
            double angleX = SectionHelper.CalculateAngle(J11H, J22H, JxxH, JyyH, JxyH);

            return (GetHomogenizedArea(phi), SxH, SyH, centroidH, JxxH, JyyH, JxyH, JpH, J11H, J22H, angleX);
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

        public virtual double CalculateN(ReinforcedConcreteRebar rebar)
        {
            return ConcreteSectionHelper.CalculateN(rebar, ConcreteMaterial);
        }

        public virtual double CalculateN(int rebar)
        {
            return ConcreteSectionHelper.CalculateN(_rebars.GetById(rebar), ConcreteMaterial);
        }

        #endregion

        #endregion

        public ReinforcedConcreteSection ToReinforcedConcreteSection()
        {
            var section = new ReinforcedConcreteSection(new ShapeEx(GetShape(), ConcreteMaterial), Name);
            section.AddRebars(_rebars);

            return section;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 2;
            info.AddValue("ConcreteSectionHVersion", version);

            info.AddValue("Rebars", _rebars);
        }

        #endregion
    }
}
