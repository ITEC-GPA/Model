using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Collections;
using GPC.Model.ElementProperties;
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
    /// <summary>
    /// A reinforced concrete section: a concrete shape with rebars and, for composite sections, steel sections. The properties of the shape are the ones of the concrete only; the homogenized ones include the rebars and the steel sections
    /// </summary>
    [Serializable]
    public partial class ReinforcedConcreteSection : BeamProperty, IConcreteSection, IEquatable<ReinforcedConcreteSection>, ISerializable
    {
        #region Variables

        /// <summary>
        /// Concrete cross-section shape.
        /// </summary>
        protected readonly ISectionShape _sectionShape;

        /// <summary>
        /// Concrete material.
        /// </summary>
        protected ConcreteMaterial _concreteMaterial;

        /// <summary>
        /// The cached mesh of the concrete
        /// </summary>
        protected Mesh _mesh;

        /// <summary>
        /// The lock of the mesh cache
        /// </summary>
        [NonSerialized]
        private readonly object _meshSync = new object();

        /// <summary>
        /// The size of the cached mesh
        /// </summary>
        [NonSerialized]
        private double _cachedMeshSize;

        /// <summary>
        /// The option "initial mesh only" of the cached mesh
        /// </summary>
        [NonSerialized]
        private bool _cachedInitialMeshOnly;

        /// <summary>
        /// The option "recombine" of the cached mesh
        /// </summary>
        [NonSerialized]
        private bool _cachedRecombine;

        /// <summary>
        /// The option "refine" of the cached mesh
        /// </summary>
        [NonSerialized]
        private bool _cachedRefine;

        /// <summary>
        /// True if the mesh is cached
        /// </summary>
        [NonSerialized]
        private bool _hasCachedMesh;

        /// <summary>
        /// The mesh size set by <see cref="SetMeshSize(double)"/> (0: automatic)
        /// </summary>
        [NonSerialized]
        private double _configuredMeshSize;

        /// <summary>
        /// Rebars list.
        /// </summary>
        protected readonly UniqueIdCollection<ReinforcedConcreteRebar> _rebars;

        /// <summary>
        /// Optional, steel cross-section within concrete section.
        /// </summary>
        protected readonly List<SteelSectionPosition> _steelSections;

        #endregion

        #region Properties from shape - Only the part made of concrete

        /// <summary>
        ///
        /// </summary>
        public double Area => _sectionShape.Area;

        /// <summary>
        ///
        /// </summary>
        public double R11 => _sectionShape.R11;

        /// <summary>
        ///
        /// </summary>
        public double R22 => _sectionShape.R22;

        /// <summary>
        ///
        /// </summary>
        public double Rxx => _sectionShape.Rxx;

        /// <summary>
        ///
        /// </summary>
        public double Ryy => _sectionShape.Ryy;

        /// <summary>
        ///
        /// </summary>
        public double Rxy => _sectionShape.Rxy;

        /// <summary>
        ///
        /// </summary>
        public Point2d Centroid => _sectionShape.Centroid;

        /// <summary>
        ///
        /// </summary>
        public Point2d ShearCenter => _sectionShape.ShearCenter;

        /// <summary>
        ///
        /// </summary>
        public double J11 => _sectionShape.J11;

        /// <summary>
        ///
        /// </summary>
        public double J22 => _sectionShape.J22;

        /// <summary>
        ///
        /// </summary>
        public double AngleX1 => _sectionShape.AngleX1;

        /// <summary>
        ///
        /// </summary>
        public double Jxx => _sectionShape.Jxx;

        /// <summary>
        ///
        /// </summary>
        public double Jyy => _sectionShape.Jyy;

        /// <summary>
        ///
        /// </summary>
        public double Jxy => _sectionShape.Jxy;

        /// <summary>
        ///
        /// </summary>
        public double Jp => _sectionShape.Jp;

        /// <summary>
        ///
        /// </summary>
        public double Jt => _sectionShape.Jt;

        /// <summary>
        ///
        /// </summary>
        public double Jw => _sectionShape.Jw;

        /// <summary>
        ///
        /// </summary>
        public double Wpl1 => _sectionShape.Wpl1;

        /// <summary>
        ///
        /// </summary>
        public double Wpl2 => _sectionShape.Wpl2;

        /// <summary>
        ///
        /// </summary>
        public double Wel1 => _sectionShape.Wel1;

        /// <summary>
        ///
        /// </summary>
        public double Wel2 => _sectionShape.Wel2;

        /// <summary>
        ///
        /// </summary>
        public bool IsSymmetricAlongXLocalAxis => _sectionShape.IsSymmetricAlongXLocalAxis;

        /// <summary>
        ///
        /// </summary>
        public bool IsSymmetricAlongYLocalAxis => _sectionShape.IsSymmetricAlongYLocalAxis;

        /// <summary>
        ///
        /// </summary>
        public bool IsDoubleSymmetric => _sectionShape.IsDoubleSymmetric;

        /// <summary>
        ///
        /// </summary>
        public double Height => _sectionShape.Height;

        /// <summary>
        ///
        /// </summary>
        public double Width => _sectionShape.Width;

        /// <summary>
        ///
        /// </summary>
        public double Wel1Min => _sectionShape.Wel1Min;

        /// <summary>
        ///
        /// </summary>
        public double Wel1Max => _sectionShape.Wel1Max;

        /// <summary>
        ///
        /// </summary>
        public double Wel2Min => _sectionShape.Wel2Min;

        /// <summary>
        ///
        /// </summary>
        public double Wel2Max => _sectionShape.Wel2Max;

        /// <summary>
        ///
        /// </summary>
        public double WelXMin => _sectionShape.WelXMin;

        /// <summary>
        ///
        /// </summary>
        public double WelXMax => _sectionShape.WelXMax;

        /// <summary>
        ///
        /// </summary>
        public double WelYMin => _sectionShape.WelYMin;

        /// <summary>
        ///
        /// </summary>
        public double WelYMax => _sectionShape.WelYMax;

        /// <summary>
        ///
        /// </summary>
        public double WelX => _sectionShape.WelX;

        /// <summary>
        ///
        /// </summary>
        public double WelY => _sectionShape.WelY;

        /// <summary>
        /// The material property should not be used, it is only for backward compatibility, to be able to read the material in serializations of old files (it throws <see cref="NotImplementedException"/>).
        /// </summary>
        public Material Material => throw new NotImplementedException();

        /// <summary>
        ///
        /// </summary>
        public ThinWallSection.ThinWall[] ThinWalls => _sectionShape.ThinWalls;

        #endregion

        #region Properties

        /// <summary>
        ///
        /// </summary>
        public Mesh Mesh
        {
            get
            {
                lock (_meshSync)
                {
                    double size = _configuredMeshSize;
                    if (size <= 0)
                    {
                        Point2d bBox = ConcreteShape.Get2dBoundingBox().Size;
                        size = Math.Min(Math.Max(bBox.X, bBox.Y) / 5.0, Math.Min(bBox.X, bBox.Y));
                    }

                    return GetMesh(size);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public ISectionShape SectionShape => _sectionShape;

        /// <summary>
        ///
        /// </summary>
        public IEnumerable<ReinforcedConcreteRebar> Rebars => _rebars.Values.AsEnumerable();

        /// <summary>
        ///
        /// </summary>
        public ConcreteMaterial ConcreteMaterial
        {
            get => _concreteMaterial;
            set => _concreteMaterial = value;
        }

        /// <summary>
        /// The shape of the concrete: the region of the concrete shape, including the area of the steel sections inside it (the homogenized
        /// properties subtract it)
        /// </summary>
        public Shape2d ConcreteShape => _sectionShape.Shape;

        /// <summary>
        /// The shape of the concrete as member of <see cref="ISectionShape"/>: <see cref="ConcreteShape"/>. Before, the public property Shape
        /// (an ambiguous name: also the type <see cref="GPC.Geometry.Shape"/>); through the interface it is still available
        /// </summary>
        Shape2d ISectionShape.Shape => ConcreteShape;

        /// <summary>
        ///
        /// </summary>
        public double AreaRebars => _rebars.Select(i => i.Value.Area).Sum();

        /// <summary>
        ///
        /// </summary>
        public int RebarsCount => _rebars.Count;

        /// <summary>
        ///
        /// </summary>
        public IList<SteelSectionPosition> SteelSections => _steelSections;

        private ConcreteShearData _shearData;
        /// <summary>
        /// Shear reinforcement and explicit shear-resisting data per direction, with provenance (null: not given).
        /// Physical data of the section: they enter its revision, so a change invalidates the checks that use them.
        /// </summary>
        public ConcreteShearData ShearData { get => _shearData; set => _shearData = value; }

        private ConcreteTorsionData _torsionData;
        /// <summary>
        /// Torsion-resisting profile and longitudinal bars for torsion, with provenance (null: not given). The links are those of
        /// <see cref="ShearData"/>. Part of the section revision like the shear data.
        /// </summary>
        public ConcreteTorsionData TorsionData { get => _torsionData; set => _torsionData = value; }

        private ConcreteCrackData _crackData;
        /// <summary>Crack-control data (exposure, sensitivity, cover, bond, bar layout), with provenance (null: not given). Part of the section revision.</summary>
        public ConcreteCrackData CrackData { get => _crackData; set => _crackData = value; }

        private ConcreteDetailingData _detailingData;
        /// <summary>Detailing data (covers, widths, aggregate, lap zone, confirmations), with provenance (null: not given). Part of the section revision.</summary>
        public ConcreteDetailingData DetailingData { get => _detailingData; set => _detailingData = value; }

        private ConcreteDurabilityData _durabilityData;
        /// <summary>Durability data (exposure classes, design life, cover modifiers), with provenance (null: not given). Part of the section revision.</summary>
        public ConcreteDurabilityData DurabilityData { get => _durabilityData; set => _durabilityData = value; }

        /// <summary>
        ///
        /// </summary>
        public bool IsCompositeSteelConcrete => _steelSections.Count > 0;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates the section and calculates the properties of the shape
        /// </summary>
        /// <param name="sectionShape">The concrete shape</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <param name="rebars">The rebars (optional)</param>
        /// <param name="steelSectionPositions">The steel sections (optional): it is calculated if they are inside the concrete</param>
        /// <exception cref="ArgumentNullException">If <paramref name="concreteMaterial"/> is null (a null <paramref name="sectionShape"/> throws <see cref="NullReferenceException"/>)</exception>
        public ReinforcedConcreteSection(ISectionShape sectionShape, ConcreteMaterial concreteMaterial, UniqueIdCollection<ReinforcedConcreteRebar> rebars = null,
            List<SteelSectionPosition> steelSectionPositions = null)
            : base(sectionShape.Name)
        {
            _sectionShape = sectionShape ?? throw new ArgumentNullException(nameof(_sectionShape));
            _concreteMaterial = concreteMaterial ?? throw new ArgumentNullException(nameof(_concreteMaterial));
            _rebars = rebars ?? new UniqueIdCollection<ReinforcedConcreteRebar>();
            _steelSections = steelSectionPositions ?? new List<SteelSectionPosition>();
            SetSteelSectionIsInside();

            SetMechanicalProperties();
        }

        /// <summary>
        /// Creates the section of a generic concrete shape, without rebars
        /// </summary>
        /// <param name="shape">The concrete shape</param>
        /// <param name="material">The concrete</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentNullException">If <paramref name="material"/> is not a concrete</exception>
        public ReinforcedConcreteSection(Shape2d shape, Material material, string name = "")
            : base(name)
        {
            _name = name;
            _sectionShape = new Section(shape);
            _concreteMaterial = material as ConcreteMaterial ?? throw new ArgumentNullException(nameof(material));
            _rebars = new UniqueIdCollection<ReinforcedConcreteRebar>();
            _steelSections = new List<SteelSectionPosition>();
            SetSteelSectionIsInside();

            SetMechanicalProperties();
        }

        /// <summary>
        /// Add a typical mixed section for bridges, with a rectangular concrete section above an H-shaped steel profile.
        /// One or two rows of reinforcing bars placed according to the concrete cover can be added.
        /// </summary>
        /// <param name="concreteWidth">Concrete base width.</param>
        /// <param name="concreteHeight">Height of concrete rectangle.</param>
        /// <param name="concreteMaterial">Concrete material.</param>
        /// <param name="rebarsSectionTop">Cross section of the upper reinforcing bars. Null value for not inserting bars.</param>
        /// <param name="rebarsPitchTop">The pitch of the upper reinforcing bars.</param>
        /// <param name="rebarsCoverTop">Upper reinforcement bar covers (to the center of the bars).</param>
        /// <param name="rebarsSectionBottom">Cross section of the lower reinforcing bars. Null value for not inserting bars.</param>
        /// <param name="rebarsPitchBottom">The pitch of the lower reinforcing bars.</param>
        /// <param name="steelShapeH">Steel H-shape profile. Null value for not inserting steel profile.</param>
        /// <param name="steelMaterial">Steel material of steel H-shape profile.</param>
        /// <param name="rebarsCoverBottom">Lower reinforcement bar covers (0: the upper one).</param>
        /// <param name="steelEccentricity">Horizontal eccentricity (in the X direction) of the steel
        /// section (its barycenter) with respect to the barycenter of the concrete part.</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentNullException">If <paramref name="concreteMaterial"/> is null</exception>
        public ReinforcedConcreteSection(double concreteWidth, double concreteHeight, ConcreteMaterial concreteMaterial,
            IRebarSection rebarsSectionTop, double rebarsPitchTop, double rebarsCoverTop,
            IRebarSection rebarsSectionBottom, double rebarsPitchBottom,
            SectionH steelShapeH, SteelMaterial steelMaterial, double rebarsCoverBottom = 0.0, double steelEccentricity = 0.0, string name = "")
            : this(concreteWidth, concreteHeight, concreteMaterial, rebarsSectionTop, rebarsPitchTop, rebarsCoverTop, rebarsSectionBottom,
                  rebarsPitchBottom, steelShapeH, steelMaterial, rebarsCoverBottom, steelEccentricity, name, true)
        {
        }

        /// <summary>
        /// A typical mixed section for bridges with any steel section below the slab (e.g. <see cref="SectionHDoubleBottomFlange"/>): the
        /// same as the constructor with a <see cref="SectionH"/>. The steel is centred on its widest part, with its top at the bottom of
        /// the slab
        /// </summary>
        /// <param name="concreteWidth">Concrete base width.</param>
        /// <param name="concreteHeight">Height of concrete rectangle.</param>
        /// <param name="concreteMaterial">Concrete material.</param>
        /// <param name="rebarsSectionTop">Cross section of the upper reinforcing bars. Null value for not inserting bars.</param>
        /// <param name="rebarsPitchTop">The pitch of the upper reinforcing bars.</param>
        /// <param name="rebarsCoverTop">Upper reinforcement bar covers (to the center of the bars).</param>
        /// <param name="rebarsSectionBottom">Cross section of the lower reinforcing bars. Null value for not inserting bars.</param>
        /// <param name="rebarsPitchBottom">The pitch of the lower reinforcing bars.</param>
        /// <param name="steelShape">Steel section, with the origin at the bottom left corner of its bounding box. Null value for not
        /// inserting steel profile.</param>
        /// <param name="steelMaterial">Steel material.</param>
        /// <param name="rebarsCoverBottom">Lower reinforcement bar covers (0: the upper one).</param>
        /// <param name="steelEccentricity">Horizontal eccentricity (in the X direction) of the steel section with respect to the barycenter of
        /// the concrete part.</param>
        /// <param name="name">The name</param>
        /// <returns>The new section</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="concreteMaterial"/> is null</exception>
        public static ReinforcedConcreteSection CreateBridgeSection(double concreteWidth, double concreteHeight, ConcreteMaterial concreteMaterial,
            IRebarSection rebarsSectionTop, double rebarsPitchTop, double rebarsCoverTop,
            IRebarSection rebarsSectionBottom, double rebarsPitchBottom,
            Section steelShape, SteelMaterial steelMaterial, double rebarsCoverBottom = 0.0, double steelEccentricity = 0.0, string name = "") =>
            new ReinforcedConcreteSection(concreteWidth, concreteHeight, concreteMaterial, rebarsSectionTop, rebarsPitchTop, rebarsCoverTop,
                rebarsSectionBottom, rebarsPitchBottom, steelShape, steelMaterial, rebarsCoverBottom, steelEccentricity, name, true);

        private ReinforcedConcreteSection(double concreteWidth, double concreteHeight, ConcreteMaterial concreteMaterial,
            IRebarSection rebarsSectionTop, double rebarsPitchTop, double rebarsCoverTop,
            IRebarSection rebarsSectionBottom, double rebarsPitchBottom,
            Section steelShapeH, SteelMaterial steelMaterial, double rebarsCoverBottom, double steelEccentricity, string name, bool bridge)
            : base(name)
        {
            if (rebarsCoverBottom == 0)
                rebarsCoverBottom = rebarsCoverTop;

            // Assignments.
            _sectionShape = new SectionRectangular(concreteHeight, concreteWidth);
            _concreteMaterial = concreteMaterial ?? throw new ArgumentNullException(nameof(_concreteMaterial));

            // Top rebars.
            _rebars = new UniqueIdCollection<ReinforcedConcreteRebar>();
            AddRebars(rebarsSectionTop, rebarsPitchTop, concreteHeight - rebarsCoverTop);

            // Bottom rebars.
            AddRebars(rebarsSectionBottom, rebarsPitchBottom, rebarsCoverBottom);

            void AddRebars(IRebarSection rebarsSection, double rebarsPitch, double rebarsPosY0)
            {
                if (!(rebarsSection is null) && rebarsPitch > 0.0)
                {
                    // Evaluates the number of bars that can be inserted.
                    int rebarsIntervals = (int)Math.Truncate(concreteWidth / rebarsPitch);
                    if (rebarsPitch * rebarsIntervals + rebarsSection.Diameter >= concreteWidth)
                        rebarsIntervals -= 1;

                    double rebarsWidth = rebarsPitch * rebarsIntervals;
                    double rebarsPosX0 = 0.5 * (concreteWidth - rebarsWidth);
                    for (int i = 0; i <= rebarsIntervals; i++)
                        AddRebar(new ReinforcedConcreteRebar(rebarsSection, new Point2d(rebarsPosX0 + i * rebarsPitch, rebarsPosY0)));
                }
            }

            // Add steel section.
            _steelSections = new List<SteelSectionPosition>();
            if (steelShapeH != null && steelMaterial != null)
                _steelSections.Add(new SteelSectionPosition(new SteelSection(steelShapeH, steelMaterial),
                    Point2d.Origin, 0.0,
                    new Point2d(0.5 * concreteWidth - 0.5 * steelShapeH.Width + steelEccentricity, -steelShapeH.Height))
                { IsInsideConcrete = false });
        }

        /// <summary>
        /// Deserialization constructor (from the version 3 the material and the steel sections)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
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

            _sectionShape = (ISectionShape)info.GetValue("SectionShape", typeof(ISectionShape));
            _rebars = (UniqueIdCollection<ReinforcedConcreteRebar>)info.GetValue("RebarCollection", typeof(UniqueIdCollection<ReinforcedConcreteRebar>));
            _steelSections = new List<SteelSectionPosition>();
            if (version > 2)
            {
                _concreteMaterial = (ConcreteMaterial)info.GetValue("ConcreteMaterial", typeof(ConcreteMaterial));

                int steelSectionsCount = info.GetInt32("SteelSectionsCount");
                if (steelSectionsCount > 0)
                    for (int i = 0; i < steelSectionsCount; i++)
                        _steelSections.Add((SteelSectionPosition)info.GetValue($"SteelSectionPosition{i}", typeof(SteelSectionPosition)));
            }
            else
            {
                // Retrieve material from old sections that contain material.
                // 2023-09-04: It never passes here, consider removing this "else" possibility.
                var shapeAsSection = _sectionShape as Section;
                if (shapeAsSection != null)
                    _concreteMaterial = shapeAsSection._material as ConcreteMaterial ?? throw new ArgumentNullException(nameof(_concreteMaterial));
            }
            if (version > 3)
                foreach (SerializationEntry entry in info)
                {
                    if (entry.Name == "ShearData") _shearData = (ConcreteShearData)info.GetValue("ShearData", typeof(ConcreteShearData));
                    else if (entry.Name == "TorsionData") _torsionData = (ConcreteTorsionData)info.GetValue("TorsionData", typeof(ConcreteTorsionData));
                    else if (entry.Name == "CrackData") _crackData = (ConcreteCrackData)info.GetValue("CrackData", typeof(ConcreteCrackData));
                    else if (entry.Name == "DetailingData") _detailingData = (ConcreteDetailingData)info.GetValue("DetailingData", typeof(ConcreteDetailingData));
                    else if (entry.Name == "DurabilityData") _durabilityData = (ConcreteDurabilityData)info.GetValue("DurabilityData", typeof(ConcreteDurabilityData));
                }
        }

        #endregion

        #region Public Methods

        #region Rebars

        /// <summary>
        /// Add a rebar into the section (see <see cref="AddRebar(ReinforcedConcreteRebar, out int)"/>)
        /// </summary>
        /// <param name="rebar">The rebar</param>
        /// <returns>True if the rebar has been added</returns>
        public bool AddRebar(ReinforcedConcreteRebar rebar)
        {
            return AddRebar(rebar, out _);
        }

        /// <summary>
        /// Add a <paramref name="rebar"/> into the section.
        /// </summary>
        /// <param name="rebar">The rebar</param>
        /// <param name="id">The <see cref="ModelObjectId.Id"/> of the rebar; <see cref="ModelObjectId.IDUNASSIGNED"/> if not added</param>
        /// <remarks>
        /// <para>If a rebar with the same id already exist in the collection, <paramref name="rebar"/> will replace that rebar</para>
        /// <para>If <paramref name="rebar"/> ID is lower than 1, this will be replaced with the maximum id + 1</para>
        /// </remarks>
        /// <returns>False if an equal rebar is already present</returns>
        public bool AddRebar(ReinforcedConcreteRebar rebar, out int id)
        {
            if (_rebars.ContainsValue(rebar))
            {
                // stessa posizione, torniamo falso

                id = IDUNASSIGNED;

                return false;
            }
            else
            {
                if (rebar.Id < 1)
                    rebar.Id = Math.Max(_rebars.MaxId, _rebars.Count == 0 ? 0 : _rebars.Keys.Max()) + 1;

                _rebars.Add(rebar);
                id = rebar.Id;

                return true;
            }
        }

        /// <summary>
        /// Add rebars into the section (see <see cref="AddRebar(ReinforcedConcreteRebar, out int)"/>)
        /// </summary>
        /// <param name="rebars">The rebars</param>
        /// <param name="ids">The ids of the rebars</param>
        /// <returns>For each rebar, true if it has been added</returns>
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

        /// <summary>
        /// Add rebars into the section (see <see cref="AddRebar(ReinforcedConcreteRebar, out int)"/>)
        /// </summary>
        /// <param name="rebars">The rebars</param>
        /// <returns>For each rebar, true if it has been added</returns>
        public bool[] AddRebars(IEnumerable<ReinforcedConcreteRebar> rebars)
        {
            return AddRebars(rebars, out _);
        }

        /// <summary>
        /// Removes a rebar (all the rebars equal to it)
        /// </summary>
        /// <param name="rebar">The rebar</param>
        /// <returns>True if the rebars have been removed</returns>
        public bool RemoveRebar(ReinforcedConcreteRebar rebar)
        {
            return _rebars.Remove(rebar);
        }

        /// <summary>
        /// Removes the rebar with an id
        /// </summary>
        /// <param name="rebarId">The id</param>
        /// <returns>True if the rebar has been removed</returns>
        public bool RemoveRebar(int rebarId)
        {
            return _rebars.Remove(rebarId);
        }

        /// <summary>
        /// Removes rebars
        /// </summary>
        /// <param name="rebars">The rebars</param>
        /// <returns>False at the first rebar not found</returns>
        public bool RemoveRebars(IEnumerable<ReinforcedConcreteRebar> rebars)
        {
            return _rebars.RemoveRange(rebars);
        }

        /// <summary>
        /// Removes all the rebars
        /// </summary>
        /// <returns>True if the rebars have been removed</returns>
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

        /// <summary>
        /// The rebar with an id
        /// </summary>
        /// <param name="rebarId">The id</param>
        /// <returns>The rebar; null if not found</returns>
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

        /// <summary>
        /// The rebars
        /// </summary>
        /// <returns>A new array with the rebars</returns>
        public ReinforcedConcreteRebar[] GetRebars()
        {
            return _rebars.Values.ToArray();
        }

        /// <summary>
        /// The rebars with the given ids
        /// </summary>
        /// <param name="rebarIds">The ids</param>
        /// <returns>The rebars (null for the ids not found)</returns>
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

        /// <summary>
        /// Adds rebars on a circle around the centroid (see <see cref="ConcreteSectionHelper.SetRadialRebars"/>)
        /// </summary>
        /// <param name="diameter">The diameter of the section</param>
        /// <param name="concreteCover">The cover (to the center of the bars)</param>
        /// <param name="numberOfRebars">The number of rebars</param>
        /// <param name="rebarSection">The bar section</param>
        /// <param name="epsilonP">The prestress STRESS of the rebars</param>
        /// <returns>Always true</returns>
        public bool AddRadialRebars(double diameter, double concreteCover, int numberOfRebars, IRebarSection rebarSection, double epsilonP = 0.0)
        {
            return _rebars.AddRange(ConcreteSectionHelper.SetRadialRebars(diameter, concreteCover, numberOfRebars, rebarSection, Centroid, epsilonP));
        }

        /// <summary>
        /// For each rebar, if it is inside the concrete shape
        /// </summary>
        /// <returns>The map between the INDEX of the rebar (not its id) and true if it is inside</returns>
        public Dictionary<int, bool> GetRebarIsInsideAssociation()
        {
            Dictionary<int, bool> kvp = new Dictionary<int, bool>();

            var rebarsArray = _rebars.ToArray();

            for (int i = 0; i < rebarsArray.Length; i++)
            {
                if (ConcreteShape.IsPointInside(rebarsArray[i].Value.Position))
                    kvp.Add(i, true);
                else
                    kvp.Add(i, false);
            }

            return kvp;
        }

        #endregion

        #region Steel sections

        /// <summary>
        /// Add a steel section (see <see cref="AddSteelSection(SteelSectionPosition, out int)"/>)
        /// </summary>
        /// <param name="steelSection">The steel section</param>
        /// <returns>True if the steel section has been added</returns>
        public bool AddSteelSection(SteelSectionPosition steelSection)
        {
            return AddSteelSection(steelSection, out _);
        }

        /// <summary>
        /// Add a steel section into the section.
        /// </summary>
        /// <param name="steelSection">The steel section</param>
        /// <param name="id">The Id of the steel section; <see cref="ModelObjectId.IDUNASSIGNED"/> if not added</param>
        /// <remarks>
        /// <para>If a steel section with the same id already exist in the collection, steelSection should replace that steel section (the
        /// sections with the same id are removed only if they are more than one: with one, the new section is added as a duplicate).</para>
        /// <para>If steel section ID is lower than 1, this will be replaced with the maximum id + 1.</para>
        /// </remarks>
        /// <returns>False if the steel section is already present</returns>
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

        /// <summary>
        /// Removes a steel section
        /// </summary>
        /// <param name="steelSection">The steel section</param>
        /// <returns>True if the steel section has been removed</returns>
        public bool RemoveSteelSection(SteelSectionPosition steelSection)
        {
            return _steelSections.Remove(steelSection);
        }

        /// <summary>
        /// Removes the steel sections with an id
        /// </summary>
        /// <param name="steelSectionId">The id</param>
        /// <returns>True if at least one steel section has been removed</returns>
        public bool RemoveSteelSection(int steelSectionId)
        {
            return _steelSections.RemoveAll(s => s.Id == steelSectionId) > 0;
        }

        /// <summary>
        /// For all steel sections, save whether it is entirely inside the concrete section (<see cref="SteelSectionPosition.IsInsideConcrete"/>),
        /// from the exact overlap of its outline with the concrete (see <see cref="SteelSectionPosition.ConcreteOverlapArea"/>). Before, the whole
        /// section was inside when the middle line of its first thin wall was inside, and the sections without thin walls (e.g.
        /// <see cref="SectionBuiltUp"/>) threw <see cref="NotImplementedException"/>. If the overlap can not be computed, the old rule
        /// </summary>
        public void SetSteelSectionIsInside()
        {
            foreach (var steelSection in _steelSections)
            {
                ConcreteOverlap overlap = steelSection.UpdateConcreteOverlap(ConcreteShape);
                if (overlap != null)
                {
                    steelSection.IsInsideConcrete = overlap.IsInside;
                    continue;
                }

                ThinWallSection.ThinWall[] thinWalls;
                try
                {
                    thinWalls = steelSection.Section.ThinWalls;
                }
                catch (NotImplementedException)
                {
                    thinWalls = null;
                }

                if (thinWalls != null && thinWalls.Length > 0)
                {
                    var thinwall = thinWalls[0];
                    var midLine = thinwall.GetMiddleLine();
                    var globStartPoint = steelSection.PositionToGlobal(midLine[0]);
                    var globEndPoint = steelSection.PositionToGlobal(midLine[1]);
                    steelSection.IsInsideConcrete = ConcreteShape.IsLineInside(new Line2d(globStartPoint, globEndPoint));
                }
                else
                {
                    // There are no thinwalls. Maybe is this a circular bar?
                    // Use centerid.
                    var centroidPoint = steelSection.Section.Centroid;
                    var globCentroidPoint = steelSection.PositionToGlobal(centroidPoint);
                    steelSection.IsInsideConcrete = ConcreteShape.IsPointInside(globCentroidPoint);
                }
            }
        }

        /// <summary>
        /// Computes the overlap of each steel section with the concrete, for the homogenized properties (reused while the positions do not change)
        /// </summary>
        private void UpdateSteelOverlaps()
        {
            foreach (SteelSectionPosition steelSection in _steelSections)
                steelSection.UpdateConcreteOverlap(ConcreteShape);
        }

        /// <summary>
        /// The torsion of the composite section solved with the finite elements (see <see cref="Section.CalculateTorsionProperties"/>): the concrete
        /// without the area of the steel sections inside it and the exact outlines of the steel sections, homogenized to the concrete with the
        /// ratios of the shear moduli Gs / Gc (torsion constant: Gc It is the torsional stiffness) and of the elastic moduli Es / Ec (shear centre
        /// and warping constant). The closed cells made by the concrete and the steel (e.g. an open steel box closed by the slab) are solved as
        /// such. The rebars are not considered. The <see cref="Jt"/> of the section is the one of the concrete only
        /// </summary>
        /// <param name="phi">The creep coefficient of the concrete: Ec / (1 + phi)</param>
        /// <param name="meshSize">The size of the elements (not positive: the default, see <see cref="SectionTorsionProperties.MeshSize"/>)</param>
        /// <returns>The torsion properties in the concrete; not solved if the concrete without the steel can not be computed or the solver fails</returns>
        public SectionTorsionProperties CalculateHomogenizedTorsionProperties(double phi = 0.0, double meshSize = 0)
        {
            double ec = ConcreteMaterial.ElasticModulusCompression / (1.0 + phi);
            double gc = ec / (2.0 * (1.0 + ConcreteMaterial.Ni));
            if (!(ec > 0))
                return new SectionTorsionProperties("the elastic modulus of the concrete is not positive");

            var regions = new List<TorsionRegion>();
            var steelOutlines = new List<Shape2d>();
            foreach (SteelSectionPosition steelSection in _steelSections)
            {
                SteelMaterial steel = steelSection.Section.SteelMaterial;
                double es = steel.ElasticModulusTension, gs = es / (2.0 * (1.0 + steel.Ni));
                foreach (Shape2d outline in steelSection.GetGlobalOutlines())
                {
                    regions.Add(new TorsionRegion(outline, gs / gc, es / ec));
                    steelOutlines.Add(outline);
                }
            }

            UpdateSteelOverlaps();
            bool overlaps = _steelSections.Any(s => s.ConcreteOverlap is null ? s.IsInsideConcrete : !s.ConcreteOverlap.IsOutside);
            if (!overlaps)
            {
                regions.Insert(0, new TorsionRegion(ConcreteShape));
            }
            else
            {
                if (!GPC.Geometry.Shape.Difference(ConcreteOverlap.Flatten(ConcreteShape).ToArray(), steelOutlines.Cast<GPC.Geometry.Shape>().ToArray(),
                    out GPC.Geometry.Shape[] concrete) || concrete is null)
                    return new SectionTorsionProperties("the concrete without the steel sections can not be computed");
                regions.InsertRange(0, concrete.Select(c => new TorsionRegion(ConcreteOverlap.ToShape2d(c))));
            }

            return SectionTorsion.Calculate(regions, meshSize);
        }

        #endregion

        #region Concrete Mechanical properties

        /// <summary>
        /// Return all homogenized mechanical properties with default value of homogenized factor n (Es / Ec)
        /// </summary>
        /// <returns>
        /// <para>areaH: The homogeneized area.</para>
        /// <para>SxH: first moment of area calculated respect input X-axis of the homogeneized section.</para>
        /// <para>SyH: first moment of area calculated respect input Y-axis of the homogeneized section.</para>
        /// <para>centroidH: The centroid of homogeneized section.</para>
        /// <para>JxxH: The second moment of area calculated respect X-axis passing throw the centroid of the homogeneized section.</para>
        /// <para>JyyH: The second moment of area calculated respect Y-axis passing throw the centroid of the homogeneized section.</para>
        /// <para>JxyH: The product of inertia respect to the same axes.</para>
        /// <para>JpH: The polar moment of inertia.</para>
        /// <para>J11H: The second moment of area calculated respect the first principal axis passing throw the centroid of the homogeneized section</para>
        /// <para>J22H: The second moment of area calculated respect the second principal axis passing throw the centroid of the homogeneized section</para>
        /// <para>AngleX: The angle of rotation of the principal axis respect the X-Axis</para>
        /// </returns>
        public (double areaH, double SxH, double SyH, Point2d centroidH, double JxxH, double JyyH, double JxyH, double JpH, double J11H, double J22H, double angleX)
            GetHomogeneizedMechanicalProperties()
        {
            UpdateSteelOverlaps();
            var centroidH = GetHomogenizedCentroid(out var SxH, out var SyH);

            // NOTA: ci siamo ricondotti a momenti d'inerzia rispetto al baricentro della sezione di solo calcestruzzo
            ConcreteSectionHelper.CalculateHomogeneizedInertiaMoments(_rebars.Values.ToArray(), Centroid, centroidH, ConcreteMaterial,
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
            UpdateSteelOverlaps();
            return ConcreteSectionHelper.GetHomogenizedCentroid(Centroid, _rebars.Values.ToArray(), ConcreteMaterial,
                Area, out SxHomog, out SyHomog, _steelSections);
        }

        /// <summary>
        /// The homogenized area with default value of homogenized factor n
        /// </summary>
        /// <returns>The homogenized area</returns>
        public double GetHomogenizedArea()
        {
            UpdateSteelOverlaps();
            return ConcreteSectionHelper.GetHomogenizedArea(_rebars.Values.ToArray(), ConcreteMaterial, Area, _steelSections);
        }

        /// <summary>
        /// The homogenized moment of inertia about the principal axis 1 (n = Es / Ec), the same of <see cref="GetHomogeneizedMechanicalProperties()"/>
        /// (before, without the steel sections)
        /// </summary>
        /// <returns>The moment of inertia</returns>
        public double GetHomogeneizedJ11()
        {
            UpdateSteelOverlaps();
            return ConcreteSectionHelper.GetHomogeneizedJ11(Centroid, _rebars.Values.ToArray(), ConcreteMaterial,
                Area, Jxx, Jyy, Jxy, _steelSections);
        }

        /// <summary>
        /// The homogenized moment of inertia about the principal axis 2 (n = Es / Ec), the same of <see cref="GetHomogeneizedMechanicalProperties()"/>
        /// (before, without the steel sections)
        /// </summary>
        /// <returns>The moment of inertia</returns>
        public double GetHomogeneizedJ22()
        {
            UpdateSteelOverlaps();
            return ConcreteSectionHelper.GetHomogeneizedJ22(Centroid, _rebars.Values.ToArray(), ConcreteMaterial,
                Area, Jxx, Jyy, Jxy, _steelSections);
        }

        /// <summary>
        /// Calculate the static moment of the concrete in X-Y plane (integrated on the mesh)
        /// </summary>
        /// <returns>The static moments respect to X and Y</returns>
        public (double Sx, double Sy) CalculateStaticMoments()
        {
            SectionHelper.CalculateStaticMoments(Mesh, out double Sx, out double Sy);
            return (Sx, Sy);
        }

        #region Phi factor

        /// <summary>
        /// Return all homogenized mechanical properties with the creep coefficient <paramref name="phi"/> (n = Es / (Ec / (1 + phi))). Without rebars
        /// and steel sections the properties of the concrete, as <see cref="GetHomogeneizedMechanicalProperties()"/> (before, all the values were
        /// zero)
        /// </summary>
        /// <param name="phi">The creep coefficient</param>
        /// <returns>
        /// <para>areaH: The homogeneized area.</para>
        /// <para>SxH: first moment of area calculated respect input X-axis of the homogeneized section.</para>
        /// <para>SyH: first moment of area calculated respect input Y-axis of the homogeneized section.</para>
        /// <para>centroidH: The centroid of homogeneized section.</para>
        /// <para>JxxH: The second moment of area calculated respect X-axis passing throw the centroid of the homogeneized section.</para>
        /// <para>JyyH: The second moment of area calculated respect Y-axis passing throw the centroid of the homogeneized section.</para>
        /// <para>JxyH: The product of inertia respect to the same axes.</para>
        /// <para>JpH: The polar moment of inertia.</para>
        /// <para>J11H: The second moment of area calculated respect the first principal axis passing throw the centroid of the homogeneized section</para>
        /// <para>J22H: The second moment of area calculated respect the second principal axis passing throw the centroid of the homogeneized section</para>
        /// <para>AngleX: The angle of rotation of the principal axis respect the X-Axis</para>
        /// </returns>
        public (double areaH, double SxH, double SyH, Point2d centroidH, double JxxH, double JyyH, double JxyH, double JpH, double J11H, double J22H, double angleX)
            GetHomogeneizedMechanicalProperties(double phi)
        {
            UpdateSteelOverlaps();
            Point2d centroidH = GetHomogenizedCentroid(phi, out var SxH, out var SyH);

            // NOTA: ci siamo ricondotti a momenti d'inerzia rispetto al baricentro della sezione di solo calcestruzzo
            ConcreteSectionHelper.CalculateHomogeneizedInertiaMoments(phi, ConcreteMaterial, _rebars.Values.ToArray(), Centroid,
                centroidH, Jxx, Jyy, Jxy, Area, out var JxxH, out var JyyH, out var JxyH, out var JpH, _steelSections);

            double J11H = SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
            double J22H = SectionHelper.CalculateJ22(JxxH, JyyH, JxyH);
            double angleX = SectionHelper.CalculateAngle(J11H, J22H, JxxH, JyyH, JxyH);

            return (GetHomogenizedArea(phi), SxH, SyH, centroidH, JxxH, JyyH, JxyH, JpH, J11H, J22H, angleX);
        }

        /// <summary>
        /// The centroid of the homogenized section with the creep coefficient <paramref name="phi"/>
        /// </summary>
        /// <param name="phi">The creep coefficient</param>
        /// <param name="SxHomog">The first moment of area respect X-Axis</param>
        /// <param name="SyHomog">The first moment of area respect Y-Axis</param>
        /// <returns>The centroid</returns>
        public Point2d GetHomogenizedCentroid(double phi, out double SxHomog, out double SyHomog)
        {
            UpdateSteelOverlaps();
            return ConcreteSectionHelper.GetHomogenizedCentroid(phi, Centroid, _rebars.Values.ToArray(), ConcreteMaterial,
                Area, out SxHomog, out SyHomog, _steelSections);
        }

        /// <summary>
        /// The homogenized area with the creep coefficient <paramref name="phi"/>
        /// </summary>
        /// <param name="phi">The creep coefficient</param>
        /// <returns>The homogenized area</returns>
        public double GetHomogenizedArea(double phi)
        {
            UpdateSteelOverlaps();
            return ConcreteSectionHelper.GetHomogenizedArea(phi, _rebars.Values.ToArray(), ConcreteMaterial, Area, _steelSections);
        }

        /// <summary>
        /// The homogenized moment of inertia about the principal axis 1 with the creep coefficient, the same of
        /// <see cref="GetHomogeneizedMechanicalProperties(double)"/> (before, without the steel sections)
        /// </summary>
        /// <param name="phi">The creep coefficient</param>
        /// <returns>The moment of inertia</returns>
        public double GetHomogeneizedJ11(double phi)
        {
            UpdateSteelOverlaps();
            return ConcreteSectionHelper.GetHomogeneizedJ11(phi, Centroid, _rebars.Values.ToArray(), ConcreteMaterial,
                Area, Jxx, Jyy, Jxy, _steelSections);
        }

        /// <summary>
        /// The homogenized moment of inertia about the principal axis 2 with the creep coefficient, the same of
        /// <see cref="GetHomogeneizedMechanicalProperties(double)"/> (before, without the steel sections)
        /// </summary>
        /// <param name="phi">The creep coefficient</param>
        /// <returns>The moment of inertia</returns>
        public double GetHomogeneizedJ22(double phi)
        {
            UpdateSteelOverlaps();
            return ConcreteSectionHelper.GetHomogeneizedJ22(phi, Centroid, _rebars.Values.ToArray(), ConcreteMaterial,
                Area, Jxx, Jyy, Jxy, _steelSections);
        }

        #endregion

        #region Utility

        /// <summary>
        /// Inverse of CalculateHomogenizedFactorN.
        /// Calculate phi from a required n (e.g., n=15): phi = n Ec / Es - 1.
        /// </summary>
        /// <param name="n">Required homogenization coefficient.</param>
        /// <param name="steelMaterial">The steel</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <returns>The creep coefficient</returns>
        public static double CalculateHomogenizedFactorPhi(in double n, in SteelMaterial steelMaterial, in ConcreteMaterial concreteMaterial)
        {
            return n * concreteMaterial.ElasticModulusCompression / steelMaterial.ElasticModulusTension - 1.0;
        }

        #endregion

        #endregion

        #endregion

        #region Protected Methods

        /// <summary>
        /// The homogenization factor of a rebar: Es / Ec
        /// </summary>
        /// <param name="rebar">The rebar</param>
        /// <returns>The factor</returns>
        public virtual double CalculateN(ReinforcedConcreteRebar rebar)
        {
            return ConcreteSectionHelper.CalculateN(rebar, ConcreteMaterial);
        }

        /// <summary>
        /// The homogenization factor of a steel section: Es / Ec
        /// </summary>
        /// <param name="steelSection">The steel section</param>
        /// <returns>The factor</returns>
        public virtual double CalculateN(SteelSectionPosition steelSection)
        {
            return ConcreteSectionHelper.CalculateN(steelSection, ConcreteMaterial);
        }

        /// <summary>
        /// The homogenization factor of the rebar with an id: Es / Ec
        /// </summary>
        /// <param name="rebarId">The id of the rebar</param>
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

        /// <summary>
        /// Serializes the section (version 4: optional shear data)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 4;
            info.AddValue("ReinforcedConcreteSectionVersion", version);
            // Written only when present: sections without shear data keep the version-3 content and revision.
            if (_shearData != null) info.AddValue("ShearData", _shearData, typeof(ConcreteShearData));
            // Same rule for the torsion data (read by the version-4 loop on the entries).
            if (_torsionData != null) info.AddValue("TorsionData", _torsionData, typeof(ConcreteTorsionData));
            if (_crackData != null) info.AddValue("CrackData", _crackData, typeof(ConcreteCrackData));
            if (_detailingData != null) info.AddValue("DetailingData", _detailingData, typeof(ConcreteDetailingData));
            if (_durabilityData != null) info.AddValue("DurabilityData", _durabilityData, typeof(ConcreteDurabilityData));

            info.AddValue("SectionShape", _sectionShape);
            info.AddValue("ConcreteMaterial", _concreteMaterial);
            info.AddValue("RebarCollection", _rebars, typeof(UniqueIdCollection<ReinforcedConcreteRebar>));

            info.AddValue("SteelSectionsCount", _steelSections != null ? _steelSections.Count : 0);
            if (_steelSections != null)
                for (int i = 0; i < _steelSections.Count; i++)
                    info.AddValue($"SteelSectionPosition{i}", _steelSections[i], typeof(SteelSectionPosition));
        }

        /// <summary>
        /// Equality with another section (see <see cref="Equals(ReinforcedConcreteSection)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal section</returns>
        public override bool Equals(object obj) => Equals(obj as ReinforcedConcreteSection);

        /// <summary>
        /// The hash code of the name, of the shape, of the rebars and of the list of the steel sections (as instance: equal sections can have different hash codes)
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _sectionShape.GetHashCode();
                hashCode = hashCode * -17 + _rebars.GetHashCodeScrambled();
                hashCode = hashCode * -17 + _steelSections.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality of the shape, of the rebars (in any order), of the steel sections and of the shear data (the material and the name are not compared)
        /// </summary>
        /// <param name="other">The section to compare</param>
        /// <returns>True if the sections are equal</returns>
        public bool Equals(ReinforcedConcreteSection other)
        {
            if (other == null) return false;

            return _sectionShape.Equals(other._sectionShape) &&
                _rebars.ScrambledEquals(other._rebars) &&
                _steelSections.SequenceEqual(other._steelSections) &&
                Equals(_shearData, other._shearData) &&
                Equals(_torsionData, other._torsionData) &&
                Equals(_crackData, other._crackData) &&
                Equals(_detailingData, other._detailingData) &&
                Equals(_durabilityData, other._durabilityData);
        }

        /// <summary>
        /// The points of the concrete shape
        /// </summary>
        /// <returns>The points</returns>
        public Point2d[] GetSectionPoints()
        {
            return _sectionShape.GetSectionPoints();
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="sectionType">The type of the section</param>
        /// <exception cref="NotImplementedException">Always</exception>
        public void SetEdgeTypeFromSteelType(Section.SectionTypes sectionType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates the properties of the concrete shape and discards the cached mesh
        /// </summary>
        public void SetMechanicalProperties()
        {
            _sectionShape.SetMechanicalProperties();
            lock (_meshSync)
            {
                _mesh = null;
                _hasCachedMesh = false;
            }
        }

        /// <summary>
        /// The mesh of the concrete shape, cached for the same options
        /// </summary>
        /// <param name="meshSize">The size of the elements (0: the size set by <see cref="SetMeshSize(double)"/>)</param>
        /// <param name="initialMeshOnly">True for the initial mesh only</param>
        /// <param name="recombine">True to recombine the triangles in quadrangles</param>
        /// <param name="refine">True to refine the mesh</param>
        /// <returns>The mesh</returns>
        public Mesh GetMesh(double meshSize = 0, bool initialMeshOnly = false, bool recombine = false, bool refine = false)
        {
            lock (_meshSync)
            {
                double effectiveMeshSize = meshSize > 0 ? meshSize : _configuredMeshSize;

                if (_hasCachedMesh && _mesh != null &&
                    _cachedMeshSize.Equals(effectiveMeshSize) &&
                    _cachedInitialMeshOnly == initialMeshOnly &&
                    _cachedRecombine == recombine &&
                    _cachedRefine == refine)
                {
                    return _mesh;
                }

                _mesh = _sectionShape.GetMesh(effectiveMeshSize, initialMeshOnly, recombine, refine);
                _cachedMeshSize = effectiveMeshSize;
                _cachedInitialMeshOnly = initialMeshOnly;
                _cachedRecombine = recombine;
                _cachedRefine = refine;
                _hasCachedMesh = true;
                return _mesh;
            }
        }

        /// <summary>
        /// Sets the size of the mesh elements and discards the cached mesh
        /// </summary>
        /// <param name="size">The size (not positive: automatic)</param>
        public void SetMeshSize(double size)
        {
            lock (_meshSync)
            {
                _configuredMeshSize = size > 0 ? size : 0;
                _mesh = null;
                _hasCachedMesh = false;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(ReinforcedConcreteSection)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are equal</returns>
        public static bool operator ==(ReinforcedConcreteSection left, ReinforcedConcreteSection right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(ReinforcedConcreteSection)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are different</returns>
        public static bool operator !=(ReinforcedConcreteSection left, ReinforcedConcreteSection right)
        {
            return !(left == right);
        }

        #endregion
    }
}
