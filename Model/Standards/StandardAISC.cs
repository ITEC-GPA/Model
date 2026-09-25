using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
    /// <summary>
    /// This class collects all the coefficient of the AISC.
    /// Default values from ANSI/AISC 360-05.
    /// </summary>
    [Serializable]
    public abstract class StandardAISC : Standard, IEquatable<StandardAISC>, ISerializable
    {
        #region Variables

        /// <inheritdoc cref="PhiBending"/>
        protected double _phiBending;
        /// <inheritdoc cref="PhiCompression"/>
        protected double _phiCompression;
        /// <inheritdoc cref="PhiTensionYielding"/>
        protected double _phiTensionYielding;
        /// <inheritdoc cref="PhiTensionFracture"/>
        protected double _phiTensionFracture;
        /// <inheritdoc cref="PhiShear"/>
        protected double _phiShear;
        /// <inheritdoc cref="PhiShearShortWeberRolledI"/>
        protected double _phiShearShortWeberRolledI;
        /// <inheritdoc cref="PhiTorsion"/>
        protected double _phiTorsion;

        #endregion

        #region Properties

        /// <summary>
        /// Resistance factor for flexure, φ_b ... section F1.
        /// </summary>
        public double PhiBending { get => _phiBending; set => _phiBending = value; }

        /// <summary>
        /// Resistance factor for compression, φ_c ... section E1.
        /// </summary>
        public double PhiCompression { get => _phiCompression; set => _phiCompression = value; }

        /// <summary>
        /// Resistance factor for tension in tensile yielding, φ_t ... section D2 (D2-1).
        /// </summary>
		public double PhiTensionYielding { get => _phiTensionYielding; set => _phiTensionYielding = value; }

        /// <summary>
        /// Resistance factor for tension in tensile rupture, φ_t ... section D2 (D2-2).
        /// </summary>
        public double PhiTensionFracture { get => _phiTensionFracture; set => _phiTensionFracture = value; }

        /// <summary>
        /// Resistance factor for shear, φ_v ... section G1.
        /// </summary>
        public double PhiShear { get => _phiShear; set => _phiShear = value; }

        /// <summary>
        /// Resistance factor for shear, for webs of rolled I-shaped members, φ_v ... section G2.1.(a).
        /// </summary>
        public double PhiShearShortWeberRolledI { get => _phiShearShortWeberRolledI; set => _phiShearShortWeberRolledI = value; }

        /// <summary>
        /// Resistance factor for torsion, φ_T ... section H3.1.
        /// </summary>
		public double PhiTorsion { get => _phiTorsion; set => _phiTorsion = value; }

        /// <summary>
        /// The group of the standard: American
        /// </summary>
        public override StandardGroupType StandardGroup => StandardGroupType.American;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates the standard
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="remarks">The remarks</param>
        public StandardAISC(string name = "AISC", string remarks = "Specification for Structural Steel Buildings")
            : base(name, remarks)
        {
            _phiBending = 0.9;
            _phiCompression = 0.9;
            _phiTensionYielding = 0.9;
            _phiTensionFracture = 0.75;
            _phiShear = 0.9;
            _phiShearShortWeberRolledI = 1.0;
            _phiTorsion = 0.9;
        }

        /// <summary>
        /// Creates the standard with the default remarks
        /// </summary>
        /// <param name="name">The name</param>
        public StandardAISC(string name = "AISC")
            : this(name, "Specification for Structural Steel Buildings")
        {
        }

        /// <summary>
        /// Creates the standard with the default name and remarks
        /// </summary>
        public StandardAISC()
            : this("AISC", "Specification for Structural Steel Buildings")
        {
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected StandardAISC(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("StandardAISCVersion");
            }
            catch (Exception)
            {
                version = 1;
            }
            PhiBending = info.GetDouble("PhiBending");
            PhiCompression = info.GetDouble("PhiCompression");
            PhiTensionYielding = info.GetDouble("PhiTensionYielding");
            PhiTensionFracture = info.GetDouble("PhiTensionFracture");
            PhiShear = info.GetDouble("PhiShear");
            PhiShearShortWeberRolledI = info.GetDouble("PhiShearShortWeberRolledI");
            PhiTorsion = info.GetDouble("PhiTorsion");
        }

        #endregion

        #region Equals - hashcode - operators

        /// <summary>
        /// Equality with an object of the same type
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is equal</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as StandardAISC);
        }

        /// <summary>
        /// Equality of the coefficients and of the base
        /// </summary>
        /// <param name="other">The object to compare</param>
        /// <returns>True if the objects are equal</returns>
        public bool Equals(StandardAISC other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   PhiBending == other.PhiBending &&
                   PhiCompression == other.PhiCompression &&
                   PhiTensionYielding == other.PhiTensionYielding &&
                   PhiTensionFracture == other.PhiTensionFracture &&
                   PhiShear == other.PhiShear &&
                   PhiShearShortWeberRolledI == other.PhiShearShortWeberRolledI &&
                   PhiTorsion == other.PhiTorsion;
        }

        /// <summary>
        /// The hash code of the coefficients and of the base
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + PhiBending.GetHashCode();
                hashCode = hashCode * -17 + PhiCompression.GetHashCode();
                hashCode = hashCode * -17 + PhiTensionYielding.GetHashCode();
                hashCode = hashCode * -17 + PhiTensionFracture.GetHashCode();
                hashCode = hashCode * -17 + PhiShear.GetHashCode();
                hashCode = hashCode * -17 + PhiShearShortWeberRolledI.GetHashCode();
                hashCode = hashCode * -17 + PhiTorsion.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Serializes the object
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 1;
            info.AddValue("StandardAISCVersion", version);
            info.AddValue("PhiBending", PhiBending);
            info.AddValue("PhiCompression", PhiCompression);
            info.AddValue("PhiTensionYielding", PhiTensionYielding);
            info.AddValue("PhiTensionFracture", PhiTensionFracture);
            info.AddValue("PhiShear", PhiShear);
            info.AddValue("PhiShearShortWeberRolledI", PhiShearShortWeberRolledI);
            info.AddValue("PhiTorsion", PhiTorsion);
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="left">The first standard</param>
        /// <param name="right">The second standard</param>
        /// <returns>True if the standards are equal</returns>
        public static bool operator ==(StandardAISC left, StandardAISC right)
        {
            return EqualityComparer<StandardAISC>.Default.Equals(left, right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="left">The first standard</param>
        /// <param name="right">The second standard</param>
        /// <returns>True if the standards are different</returns>
        public static bool operator !=(StandardAISC left, StandardAISC right)
        {
            return !(left == right);
        }

        #endregion
    }
}
