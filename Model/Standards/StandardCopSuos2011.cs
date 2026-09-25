using GPC.Model.Collections;
using GPC.Model.Combinations;
using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
    /// <summary>
    /// Code of Practice for the Structural Use of Steel 2011 (Hong Kong): partial factors (the generation of the combinations is not implemented)
    /// </summary>
    [Serializable]
    public class StandardCopSuos2011 : Standard, Standard.ICombinationsGenerator, ISerializable
    {
        #region Public Enum

        /// <summary>
        /// The limit states. Reference: CopSuos2011
        /// </summary>
        public enum LimitStates
        {
            /// <summary>
            /// Ultimate limit state: equilibrium
            /// </summary>
            UltimateEquilibrium,
            /// <summary>
            /// Ultimate limit state: strength
            /// </summary>
            UltimateStrength,
            /// <summary>
            /// Ultimate limit state: fatigue
            /// </summary>
            UltimateFatigue,
            /// <summary>
            /// Ultimate limit state: integrity and robustness
            /// </summary>
            UltimateIntegrityAndRobustness,
            /// <summary>
            /// Ultimate limit state: fracture
            /// </summary>
            UltimateFracture,
            /// <summary>
            /// Serviceability limit state
            /// </summary>
            Serviceability,
        }

        #endregion

        #region Variables

        /// <inheritdoc cref="GammaM1"/>
        private double _gammaM1;
        /// <inheritdoc cref="GammaM2"/>
        private double _gammaM2;

        #endregion

        #region Properties

        /// <summary>
        /// The partial factor γM1 (1.0)
        /// </summary>
        public double GammaM1 { get => _gammaM1; set => _gammaM1 = value; }

        /// <summary>
        /// The partial factor γM2 (1.2)
        /// </summary>
        public double GammaM2 { get => _gammaM2; set => _gammaM2 = value; }

        #endregion
        /// <summary>
        /// The group of the standard: HongKong
        /// </summary>
        public override StandardGroupType StandardGroup => StandardGroupType.HongKong;

        #region Public Constructor

        /// <summary>
        /// Creates the standard
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="remarks">The remarks</param>
        public StandardCopSuos2011(string name = "Cop2011", string remarks = "")
            : base(name, remarks)
        {
            _gammaM1 = 1.0;
            _gammaM2 = 1.2;
        }

        /// <summary>
        /// Creates the standard with the default remarks
        /// </summary>
        /// <param name="name">The name</param>
        public StandardCopSuos2011(string name = "Cop2011")
            : this(name, "")
        {
        }

        /// <summary>
        /// Creates the standard with the default name and remarks
        /// </summary>
        public StandardCopSuos2011()
            : this("Cop2011", "")
        {
        }

        /// <summary>
        /// Deserialization constructor (the base constructor is not called: the name and the remarks are not read)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected StandardCopSuos2011(SerializationInfo info, StreamingContext context)
        {
            _gammaM1 = info.GetDouble("GammaM1");
            _gammaM2 = info.GetDouble("GammaM2");
        }

        #endregion

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="loadCases">The load cases</param>
        /// <param name="options">The options</param>
        /// <param name="name">The prefix of the names</param>
        /// <returns>Nothing</returns>
        /// <exception cref="NotImplementedException">Always</exception>
        public UniqueNameCollection<Combination> CreateCombinations(LoadCaseBase[] loadCases, CombinationsOptions options, string name = "cmb")
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Serializes the object
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("GammaM1", _gammaM1);
            info.AddValue("GammaM2", _gammaM2);
        }

        /// <summary>
        /// Equality with an object of the same type
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is equal</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return obj is StandardCopSuos2011 suos &&
                   _gammaM1 == suos._gammaM1 &&
                   _gammaM2 == suos._gammaM2;
        }

        /// <summary>
        /// The hash code of the coefficients and of the base
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -17;
                hashCode = hashCode * -23 + _gammaM1.GetHashCode();
                hashCode = hashCode * -23 + _gammaM2.GetHashCode();
                return hashCode;
            }
        }
    }
}
