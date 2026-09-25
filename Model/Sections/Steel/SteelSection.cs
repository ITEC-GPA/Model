using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.ElementProperties;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Steel
{
    /// <summary>
    /// A steel section: a section shape with the steel and the type (rolled or welded, hot finished or cold formed); the properties are the ones of the shape
    /// </summary>
    [Serializable]
    public class SteelSection : BeamProperty, ISteelSection, ISerializable, IEquatable<SteelSection>
    {
        #region Varibles

        /// <summary>
        /// The shape
        /// </summary>
        protected readonly ISectionShape _sectionShape;
        /// <summary>
        /// Rolled or welded
        /// </summary>
        protected readonly Section.SectionTypes _sectionType;
        /// <summary>
        /// Hot finished or cold formed
        /// </summary>
        protected readonly Section.FormedTypes _formedType;
        /// <summary>
        /// The steel
        /// </summary>
        protected SteelMaterial _steelMaterial;

        #endregion

        #region Shape properties

        /// <summary>
        /// The elastic modulus respect to X, bottom fibre
        /// </summary>
        public double Height => _sectionShape.Height;

        /// <summary>
        /// The elastic modulus respect to Y, left fibre
        /// </summary>
        public double Width => _sectionShape.Width;

        /// <summary>
        /// The minimum elastic modulus respect to X
        /// </summary>
        public Shape2d Shape => _sectionShape.Shape;

        /// <summary>
        /// True if the shape is symmetric respect to X
        /// </summary>
        public double Area => _sectionShape.Area;

        /// <summary>
        /// True if the shape is symmetric respect to X and Y
        /// </summary>
        public double R11 => _sectionShape.R11;

        /// <summary>
        /// Rolled or welded
        /// </summary>
        public double R22 => _sectionShape.R22;

        /// <summary>
        /// The radius of gyration Rxx (before, it returned Rxy)
        /// </summary>
        public double Rxx => _sectionShape.Rxx;

        /// <summary>
        /// The radius of gyration Ryy (before, it returned Rxy)
        /// </summary>
        public double Ryy => _sectionShape.Ryy;

        /// <summary>
        /// The steel
        /// </summary>
        public double Rxy => _sectionShape.Rxy;

        /// <summary>
        /// The mesh of the shape
        /// </summary>
        public Point2d Centroid => _sectionShape.Centroid;

        /// <summary>
        ///
        /// </summary>
        public Point2d ShearCenter => _sectionShape.ShearCenter;

        /// <summary>
        ///
        /// </summary>
        public double AngleX1 => _sectionShape.AngleX1;

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
        public ThinWallSection.ThinWall[] ThinWalls => _sectionShape.ThinWalls;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates the section: sets the working of the corners from the type and calculates the properties of the shape
        /// </summary>
        /// <param name="sectionShape">The shape</param>
        /// <param name="steelMaterial">The steel</param>
        /// <param name="sectionType">Rolled or welded</param>
        /// <param name="formedType">Hot finished or cold formed</param>
        /// <exception cref="ArgumentNullException">If <paramref name="steelMaterial"/> is null (a null <paramref name="sectionShape"/> throws <see cref="NullReferenceException"/>)</exception>
        public SteelSection(ISectionShape sectionShape, SteelMaterial steelMaterial,
            Section.SectionTypes sectionType = Section.SectionTypes.Rolled,
            Section.FormedTypes formedType = Section.FormedTypes.HotFinished)
            : base(sectionShape.Name)
        {
            _sectionShape = sectionShape ?? throw new ArgumentNullException(nameof(_sectionShape));
            _steelMaterial = steelMaterial ?? throw new ArgumentNullException(nameof(_steelMaterial));
            _sectionType = sectionType;
            _formedType = formedType;
            SetEdgeTypeFromSteelType(sectionType);
            SetMechanicalProperties();
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SteelSection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version = info.GetInt32("SteelSectionVersion");

            _sectionShape = (ISectionShape)info.GetValue("SectionShape", typeof(ISectionShape));
            _steelMaterial = (SteelMaterial)info.GetValue("SteelMaterial", typeof(SteelMaterial));
            _sectionType = (Section.SectionTypes)info.GetValue("SectionType", typeof(Section.SectionTypes));
            _formedType = (Section.FormedTypes)info.GetValue("FormedType", typeof(Section.FormedTypes));
        }

        #endregion

        #region Steel section

        /// <summary>
        ///
        /// </summary>
        public Section.SectionTypes SectionType => _sectionType;

        /// <summary>
        ///
        /// </summary>
        public Section.FormedTypes FormedType => _formedType;

        /// <summary>
        ///
        /// </summary>
        public bool IsRolled => _sectionType == Section.SectionTypes.Rolled;

        /// <summary>
        ///
        /// </summary>
        public bool IsWelded => _sectionType == Section.SectionTypes.Welded;

        /// <summary>
        ///
        /// </summary>
        public bool IsHotFinished => _formedType == Section.FormedTypes.HotFinished;

        /// <summary>
        ///
        /// </summary>
        public bool IsColdFormed => _formedType == Section.FormedTypes.ColdFormed;

        /// <summary>
        ///
        /// </summary>
        public SteelMaterial SteelMaterial
        {
            get => _steelMaterial;
            set => _steelMaterial = value;
        }

        /// <summary>
        ///
        /// </summary>
        public ISectionShape SectionShape => _sectionShape;

        /// <summary>
        ///
        /// </summary>
        public Mesh Mesh => _sectionShape.Mesh;

        /// <summary>
        /// Serializes the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            double version = 1;
            info.AddValue("SteelSectionVersion", version);

            info.AddValue("SectionShape", _sectionShape);
            info.AddValue("SteelMaterial", _steelMaterial);
            info.AddValue("SectionType", _sectionType);
            info.AddValue("FormedType", _formedType);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The minimum elastic normal stress: N / A ± M1 / Wel1 ± M2 / Wel2 with the minimum moduli
        /// </summary>
        /// <param name="N">The axial force (positive: tension)</param>
        /// <param name="M1">The bending moment about the axis 1</param>
        /// <param name="M2">The bending moment about the axis 2</param>
        /// <returns>The minimum stress</returns>
        public double GetMinSigma(double N, double M1, double M2)
        {
            double wel1 = Math.Min(Wel1Max, Wel1Min);
            double wel2 = Math.Min(Wel2Max, Wel2Min);

            double sigmap1 = N / Area - M1 / wel1 + M2 / wel2;
            double sigmap2 = N / Area - M1 / wel1 - M2 / wel2;
            double sigmap3 = N / Area + M1 / wel1 + M2 / wel2;
            double sigmap4 = N / Area + M1 / wel1 - M2 / wel2;

            return (new double[] { sigmap1, sigmap2, sigmap3, sigmap4 }).Min();
        }

        /// <summary>
        /// The maximum elastic normal stress: N / A ± M1 / Wel1 ± M2 / Wel2 with the minimum moduli
        /// </summary>
        /// <param name="N">The axial force (positive: tension)</param>
        /// <param name="M1">The bending moment about the axis 1</param>
        /// <param name="M2">The bending moment about the axis 2</param>
        /// <returns>The maximum stress</returns>
        public double GetMaxSigma(double N, double M1, double M2)
        {
            double wel1 = Math.Min(Wel1Max, Wel1Min);
            double wel2 = Math.Min(Wel2Max, Wel2Min);

            double sigmap1 = N / Area - M1 / wel1 + M2 / wel2;
            double sigmap2 = N / Area - M1 / wel1 - M2 / wel2;
            double sigmap3 = N / Area + M1 / wel1 + M2 / wel2;
            double sigmap4 = N / Area + M1 / wel1 - M2 / wel2;

            return (new double[] { sigmap1, sigmap2, sigmap3, sigmap4 }).Max();
        }

        /// <summary>
        /// The points of the shape
        /// </summary>
        /// <returns>The points</returns>
        public Point2d[] GetSectionPoints()
        {
            return _sectionShape.GetSectionPoints();
        }

        /// <summary>
        /// Sets the working of the corners of the shape from the type of the section
        /// </summary>
        /// <param name="sectionType">The type of the section</param>
        public void SetEdgeTypeFromSteelType(Section.SectionTypes sectionType)
        {
            _sectionShape.SetEdgeTypeFromSteelType(sectionType);
        }

        /// <summary>
        /// Calculates the properties of the shape
        /// </summary>
        public void SetMechanicalProperties()
        {
            _sectionShape.SetMechanicalProperties();
        }

        /// <summary>
        /// Calculate the static moment of the section in X-Y plane (integrated on the mesh)
        /// </summary>
        /// <returns>The static moments respect to X and Y</returns>
        public (double Sx, double Sy) CalculateStaticMoments()
        {
            SectionHelper.CalculateStaticMoments(Mesh, out double Sx, out double Sy);
            return (Sx, Sy);
        }

        #endregion

        #region Comparer

        /// <summary>
        /// Equality with another section (see <see cref="Equals(SteelSection)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal section</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as SteelSection);
        }

        /// <summary>
        /// Equality of the shape and of the types (the material and the name are not compared)
        /// </summary>
        /// <param name="other">The section to compare</param>
        /// <returns>True if the sections are equal</returns>
        public bool Equals(SteelSection other)
        {
            return !(other is null) &&
                   EqualityComparer<ISectionShape>.Default.Equals(_sectionShape, other._sectionShape) &&
                   _sectionType == other._sectionType &&
                   _formedType == other._formedType;
        }

        /// <summary>
        /// The hash code of the shape and of the types
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            int hashCode = -1194127724;
            hashCode = hashCode * -1521134295 + EqualityComparer<ISectionShape>.Default.GetHashCode(_sectionShape);
            hashCode = hashCode * -1521134295 + _sectionType.GetHashCode();
            hashCode = hashCode * -1521134295 + _formedType.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// The mesh of the shape
        /// </summary>
        /// <param name="meshSize">The size of the elements (0: the size set by <see cref="SetMeshSize(double)"/>)</param>
        /// <param name="initialMeshOnly">True for the initial mesh only</param>
        /// <param name="recombine">True to recombine the triangles in quadrangles</param>
        /// <param name="refine">True to refine the mesh</param>
        /// <returns>The new mesh</returns>
        public Mesh GetMesh(double meshSize = 0, bool initialMeshOnly = false, bool recombine = false, bool refine = false)
        {
            return _sectionShape.GetMesh(meshSize, initialMeshOnly, recombine, refine);
        }

        /// <summary>
        /// Sets the size of the mesh elements of the shape
        /// </summary>
        /// <param name="size">The size</param>
        public void SetMeshSize(double size)
        {
            _sectionShape.SetMeshSize(size);
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(SteelSection)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are equal</returns>
        public static bool operator ==(SteelSection left, SteelSection right)
        {
            return EqualityComparer<SteelSection>.Default.Equals(left, right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(SteelSection)"/>)
        /// </summary>
        /// <param name="left">The first section</param>
        /// <param name="right">The second section</param>
        /// <returns>True if the sections are different</returns>
        public static bool operator !=(SteelSection left, SteelSection right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Temporary setter to avoid making the Name property settable.
        /// </summary>
        /// <param name="name">The new name</param>
        public void SetName(in string name)
        {
            _name = name;
        }

        #endregion
    }
}
