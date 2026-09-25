using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Sections.Rebar;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Concrete
{
    /// <summary>
    /// A rebar of a concrete section: bar section, position and prestress strain
    /// </summary>
    [Serializable]
    public class ReinforcedConcreteRebar : ModelObjectId, ISerializable
    {
        #region Variables

        /// <summary>
        /// The bar section
        /// </summary>
        protected IRebarSection _rebarSection;
        /// <summary>
        /// The position
        /// </summary>
        protected Point2d _position;
        /// <summary>
        /// The prestress strain
        /// </summary>
        protected double _epsilonP;

        #endregion

        #region Properties

        /// <summary>
        /// The area of the bar
        /// </summary>
        public double Area => _rebarSection.Area;

        /// <summary>
        /// The material of the bar
        /// </summary>
        public SteelMaterial RebarMaterial
        {
            get => _rebarSection.RebarMaterial;
            set => _rebarSection.RebarMaterial = value;
        }

        /// <summary>
        /// The bar section
        /// </summary>
        public IRebarSection RebarSection => _rebarSection;

        /// <summary>
        /// The position
        /// </summary>
        public Point2d Position => _position;

        /// <summary>
        /// The prestress strain (from the prestress stress of the constructor)
        /// </summary>
        public double EpsilonP => _epsilonP;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates the rebar
        /// </summary>
        /// <param name="section">The bar section</param>
        /// <param name="position">The position</param>
        /// <param name="sigmaP">The prestress stress (0 to fu)</param>
        /// <param name="id">The id</param>
        /// <param name="name">The name</param>
        /// <param name="guid">The guid</param>
        /// <exception cref="ArgumentNullException">If <paramref name="section"/> or <paramref name="position"/> is null</exception>
        /// <exception cref="ArgumentException">If <paramref name="sigmaP"/> is negative or bigger than fu</exception>
        public ReinforcedConcreteRebar(IRebarSection section, Point2d position, double sigmaP, int id, string name, Guid guid)
            : base(id, name, guid)
        {
            _rebarSection = section ?? throw new ArgumentNullException(nameof(section));
            _position = position ?? throw new ArgumentNullException(nameof(position));
            if (sigmaP < 0.0)
                throw new ArgumentException("SigmaP cannot be lower than 0");
            if (sigmaP > RebarMaterial.Fu)
                throw new ArgumentException("SigmaP cannot be greater than Fu");

            _epsilonP = GetEpsilonP(sigmaP);
        }

        /// <summary>
        /// Creates the rebar with a new guid
        /// </summary>
        /// <param name="section">The bar section</param>
        /// <param name="position">The position</param>
        /// <param name="sigmaP">The prestress stress (0 to fu)</param>
        /// <param name="id">The id</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentNullException">If <paramref name="section"/> or <paramref name="position"/> is null</exception>
        /// <exception cref="ArgumentException">If <paramref name="sigmaP"/> is negative or bigger than fu</exception>
        public ReinforcedConcreteRebar(IRebarSection section, Point2d position, double sigmaP = 0.0, int id = IDUNASSIGNED, string name = "")
            : this(section, position, sigmaP, id, name, Guid.NewGuid())
        {

        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected ReinforcedConcreteRebar(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _rebarSection = (IRebarSection)info.GetValue("RebarSection", typeof(IRebarSection));
            _position = (Point2d)info.GetValue("Position", typeof(Point2d));
            _epsilonP = info.GetDouble("EpsilonP");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The strain of a stress on the bilinear diagram of the material: linear up to (fyk, εy), then linear up to (fu, εu)
        /// </summary>
        /// <param name="sigmaP">The stress</param>
        /// <returns>The strain</returns>
        /// <exception cref="ArgumentException">If <paramref name="sigmaP"/> is negative</exception>
        protected double GetEpsilonP(double sigmaP)
        {
            if (sigmaP < 0.0)
                throw new ArgumentException("SigmaP must be greater than 0");

            if (sigmaP == 0.0)
                return 0.0;

            if (sigmaP <= RebarMaterial.Fyk)
                return GPC.Utilities.Maths.Interpolation.GetLinearInterpolation(0.0, RebarMaterial.Fyk, 0.0,
                    RebarMaterial.StrainYTension, sigmaP);
            else // before, StrainYTension + the interpolation at sigmaP - Fyk: a jump at Fyk (the yield strain was counted twice)
                return GPC.Utilities.Maths.Interpolation.GetLinearInterpolation(RebarMaterial.Fyk, RebarMaterial.Fu,
                    RebarMaterial.StrainYTension, RebarMaterial.StrainUTension, sigmaP);
        }

        #endregion

        #region Equals - hashcode - Operators

        /// <summary>
        /// Equality of the name, of the bar section, of the position and of the prestress strain
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal rebar</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return obj is ReinforcedConcreteRebar rebar &&
                base.Equals(obj) &&
                _rebarSection.Equals(rebar._rebarSection) &&
                _position.Equals(rebar._position) &&
                _epsilonP == rebar._epsilonP;
        }

        /// <summary>
        /// The hash code of the name, of the bar section, of the position and of the prestress strain
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _rebarSection.GetHashCode();
                hashCode = hashCode * -17 + _position.GetHashCode();
                hashCode = hashCode * -17 + _epsilonP.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Serializes the rebar
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("RebarSection", _rebarSection, typeof(IRebarSection));
            info.AddValue("Position", _position, typeof(Point2d));
            info.AddValue("EpsilonP", _epsilonP);
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>; a null <paramref name="left"/> throws <see cref="NullReferenceException"/>)
        /// </summary>
        /// <param name="left">The first rebar</param>
        /// <param name="right">The second rebar</param>
        /// <returns>True if the rebars are equal</returns>
        public static bool operator ==(ReinforcedConcreteRebar left, ReinforcedConcreteRebar right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="left">The first rebar</param>
        /// <param name="right">The second rebar</param>
        /// <returns>True if the rebars are different</returns>
        public static bool operator !=(ReinforcedConcreteRebar left, ReinforcedConcreteRebar right)
        {
            return !(left == right);
        }

        #endregion

        //[Serializable]
        //public class ReinforcedConcreteRebarComparer : IEqualityComparer<ReinforcedConcreteRebar>
        //{
        //	/// <returns>
        //	/// <para> true if both <paramref name="x"/> and <paramref name="y"/> are null </para>
        //	/// </returns>
        //	/// <remarks> Only <see cref="ModelObjectId.Id"/> is used as equality parameter </remarks>
        //	bool IEqualityComparer<ReinforcedConcreteRebar>.Equals(ReinforcedConcreteRebar x, ReinforcedConcreteRebar y)
        //	{
        //		if (ReferenceEquals(x, y))
        //			return true;

        //		if (x == null || y == null)
        //			return false;

        //		if (x.Position.Equals(y.Position))
        //			return true;

        //		return false;
        //	}

        //	/// <remarks> Only <see cref="ModelObjectId.Id"/> is used as equality parameter </remarks>
        //	int IEqualityComparer<ReinforcedConcreteRebar>.GetHashCode(ReinforcedConcreteRebar obj)
        //	{
        //		unchecked
        //		{
        //			return -391 * obj.Position.GetHashCode();
        //		}
        //	}
        //}
    }
}
