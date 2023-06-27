using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Fem.Properties;
using GPC.Model.Materials;
using GPC.Model.Sections.Rebar;
using GPC.Model.Sections.Steel;
using GPC.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Concrete
{
    [Serializable]
    public class ReinforcedConcreteSection : ElementProperty, IConcreteSection
    {
        #region Variables

        protected readonly ISection _sectionShape;
        protected Mesh _mesh;
        protected readonly RebarCollection _rebars;
        protected readonly List<SteelSectionPosition> _steelSections;

        #endregion

        #region Properties from shape

        public double Area => _sectionShape.Area;

        public double R11 => _sectionShape.R11;

        public double R22 => _sectionShape.R22;

        public double Rxy => _sectionShape.Rxy;

        public Point2d Centroid => _sectionShape.Centroid;

        public Point2d ShearCenter => _sectionShape.ShearCenter;

        public double J11 => _sectionShape.J11;

        public double J22 => _sectionShape.J22;

        public double AngleX1 => _sectionShape.AngleX1;

        public double Jxx => _sectionShape.Jxx;

        public double Jyy => _sectionShape.Jyy;

        public double Jxy => _sectionShape.Jxy;

        public double Jp => _sectionShape.Jp;

        public double Jt => _sectionShape.Jt;

        public double Jw => _sectionShape.Jw;

        public double Wpl1 => _sectionShape.Wpl1;

        public double Wpl2 => _sectionShape.Wpl2;

        public double Wel1 => _sectionShape.Wel1;

        public double Wel2 => _sectionShape.Wel2;

        public bool IsSymmetricAlongXLocalAxis => _sectionShape.IsSymmetricAlongXLocalAxis;

        public bool IsSymmetricAlongYLocalAxis => _sectionShape.IsSymmetricAlongYLocalAxis;

        public bool IsDoubleSymmetric => _sectionShape.IsDoubleSymmetric;

        #endregion

        #region Properties

        public Mesh Mesh
        {
            get
            {
                if (_mesh is null)
                {
                    Point2d bBox = Shape.Get2dBoundingBox().Size;
                    double size = Math.Min(Math.Max(bBox.X, bBox.Y) / 5.0, Math.Min(bBox.X, bBox.Y));
                    _mesh = _sectionShape.GetMesh(size);
                }
                return _mesh;
            }
        }

        public ISection SectionShape => _sectionShape;

        public IEnumerable<ReinforcedConcreteRebar> Rebars => _rebars;

        public ConcreteMaterial ConcreteMaterial => (ConcreteMaterial)_sectionShape.Material;

        public Shape2d Shape => _sectionShape.Shape;

        public double AreaRebars => _rebars.Select(i => i.Area).Sum();

        public int RebarsCount => _rebars.Count;

        public IList<SteelSectionPosition> SteelSections => _steelSections;

        #endregion

        #region Public Constructors

        //protected ReinforcedConcreteSection(ReinforcedConcreteSection reinforcedConcreteSection)
        //{
        //    if (reinforcedConcreteSection is null)
        //        throw new ArgumentNullException(nameof(reinforcedConcreteSection));

        //    _shapeEx = reinforcedConcreteSection.ShapeEx;
        //    _rebars = new RebarCollection();
        //    _steelSections = new List<SteelSectionPosition>();

        //    SetMechanicalProperties();
        //}

        public ReinforcedConcreteSection(ISection sectionShape, RebarCollection rebars = null,
            List<SteelSectionPosition> steelSectionPositions = null)
            : base(sectionShape.Name)
        {
            _sectionShape = sectionShape ?? throw new ArgumentNullException(nameof(_sectionShape));
            _rebars = rebars ?? new RebarCollection();
            _steelSections = steelSectionPositions ?? new List<SteelSectionPosition>();

            _sectionShape.SetMechanicalProperties();
        }

        public ReinforcedConcreteSection(Shape2d shape, Material material, string name = "")
            : base(name)
        {
            _name = name;
            _sectionShape = new Section(shape, material);
            _rebars = new RebarCollection();
            _steelSections = new List<SteelSectionPosition>();

            _sectionShape.SetMechanicalProperties();
        }

        protected ReinforcedConcreteSection(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("ReinforcedConcreteSectionVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            _sectionShape = (ISection)info.GetValue("SectionShape", typeof(ISection));
            _rebars = (RebarCollection)info.GetValue("RebarCollection", typeof(RebarCollection));
            _steelSections = new List<SteelSectionPosition>();
            if (version > 2)
            {
                int steelSectionsCount = info.GetInt32("SteelSectionsCount");
                if (steelSectionsCount > 0)
                    for (int i = 0; i < steelSectionsCount; i++)
                        _steelSections.Add((SteelSectionPosition)info.GetValue($"SteelSectionPosition{i}", typeof(SteelSectionPosition)));
            }
        }

        #endregion

        #region Public Methods

        #region Rebars

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

        public bool[] AddRebars(IEnumerable<ReinforcedConcreteRebar> rebars)
        {
            return AddRebars(rebars, out _);
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

        public bool AddRadialRebars(double diameter, double concreteCover, int numberOfRebars, IRebarSection rebarSection, double epsilonP = 0.0)
        {
            return _rebars.AddRange(ConcreteSectionHelper.SetRadialRebars(diameter, concreteCover, numberOfRebars, rebarSection, Centroid, epsilonP));
        }

        #endregion

        #region Steel sections

        public bool AddSteelSection(SteelSectionPosition steelSection)
        {
            return AddSteelSection(steelSection, out _);
        }

        /// <summary>
        /// Add a steel section into the section.
        /// </summary>
        /// <remarks>
        /// <para>If a steel section with the same id already exist in the collection, steelSection will replace that steel section.</para>
        /// <para>If steel section ID is lower than 1, this will be replaced with the maximum id + 1.</para>
        /// </remarks>
        /// <returns>The Id of the steel section.</returns>
        public bool AddSteelSection(SteelSectionPosition steelSection, out int id)
        {
            if (_steelSections.Contains(steelSection))
            {
                // Has already been assigned, returns false.
                id = IDUNASSIGNED;
                return false;
            }
            else
            {
                if (steelSection.Id < 1)
                {
                    // If steel section ID is lower than 1, this will be replaced with the maximum id + 1.
                    if (_steelSections.Count > 0)
                        steelSection.Id = _steelSections.Max(s => s.Id) + 1;
                    else
                        steelSection.Id = 1;

                    _steelSections.Add(steelSection);
                    id = steelSection.Id;
                    return true;
                }
                else
                {
                    // If a steel section with the same id already exist in the collection, steelSection will replace that steel section.
                    var sameIDs = _steelSections.Where(s => s.Id == steelSection.Id).ToList();
                    if (sameIDs.Count > 1)
                    {
                        foreach (var sameID in sameIDs)
                            _steelSections.Remove(sameID);
                    }
                    _steelSections.Add(steelSection);
                    id = steelSection.Id;
                    return true;
                }
            }
        }

        public bool RemoveSteelSection(SteelSectionPosition steelSection)
        {
            return _steelSections.Remove(steelSection);
        }

        public bool RemoveSteelSection(int steelSectionId)
        {
            return _steelSections.RemoveAll(s => s.Id == steelSectionId) > 0;
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
            ConcreteSectionHelper.CalculateHomogeneizedInertiaMoments(_rebars.ToArray(), Centroid, centroidH, ConcreteMaterial,
                Jxx, Jyy, Jxy, Area, out var JxxH, out var JyyH, out var JxyH, out var JpH, _steelSections);

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
                Area, out SxHomog, out SyHomog, _steelSections);
        }

        /// <summary>
        /// The homogenized area with default value of homogenized factor n
        /// </summary>
        /// <returns>The homogenized area</returns>
        public double GetHomogenizedArea()
        {
            return ConcreteSectionHelper.GetHomogenizedArea(_rebars.ToArray(), ConcreteMaterial, Area, _steelSections);
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
            if (_rebars.Count > 0)
            {
                Point2d centroidH = GetHomogenizedCentroid(phi, out var SxH, out var SyH);

                // NOTA: ci siamo ricondotti a momenti d'inerzia rispetto al baricentro della sezione di solo calcestruzzo
                ConcreteSectionHelper.CalculateHomogeneizedInertiaMoments(phi, ConcreteMaterial, _rebars.ToArray(), Centroid,
                    centroidH, Jxx, Jyy, Jxy, Area, out var JxxH, out var JyyH, out var JxyH, out var JpH, _steelSections);

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
                Area, out SxHomog, out SyHomog, _steelSections);
        }

        /// <summary>
        /// The homogenized area with homogenized factor <paramref name="phi"/>
        /// </summary>
        /// <param name="phi"></param>
        /// <returns>The homogenized area</returns>
        public double GetHomogenizedArea(double phi)
        {
            return ConcreteSectionHelper.GetHomogenizedArea(phi, _rebars.ToArray(), ConcreteMaterial, Area, _steelSections);
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

        #endregion

        #region Protected Methods

        public virtual double CalculateN(ReinforcedConcreteRebar rebar)
        {
            return ConcreteSectionHelper.CalculateN(rebar, ConcreteMaterial);
        }

        public virtual double CalculateN(SteelSectionPosition steelSection)
        {
            return ConcreteSectionHelper.CalculateN(steelSection, ConcreteMaterial);
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

        #endregion

        #region Equals, hascode, operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 3;
            info.AddValue("ReinforcedConcreteSectionVersion", version);

            info.AddValue("SectionShape", _sectionShape);
            info.AddValue("RebarCollection", _rebars, typeof(RebarCollection));

            info.AddValue("SteelSectionsCount", _steelSections != null ? _steelSections.Count : 0);
            if (_steelSections != null)
                for (int i = 0; i < _steelSections.Count; i++)
                    info.AddValue($"SteelSectionPosition{i}", _steelSections[i], typeof(SteelSectionPosition));
        }

        public override bool Equals(object obj)
        {
            return obj is ReinforcedConcreteSection section &&
                   base.Equals(obj) &&
                   _rebars.ScrambledEquals(section._rebars) &&
                   _steelSections.SequenceEqual(section._steelSections);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _rebars.GetHashCodeScrambled();
                hashCode = hashCode * -17 + _steelSections.GetHashCode();
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
