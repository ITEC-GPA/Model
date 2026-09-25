using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
	/// <summary>
	/// This class collects all the coefficient of the ACI 318
	/// </summary>
	[Serializable]
	public abstract class StandardACI318 : Standard, IEquatable<StandardACI318>
	{
		#region Variables

		/// <inheritdoc cref="PhiCSpiral"/>
		protected double _phiCSpiral;
		/// <inheritdoc cref="PhiCTied"/>
		protected double _phiCTied;
		/// <inheritdoc cref="PhiMaximumCompressiveAxialLoadSpiral"/>
		protected double _phiMaximumCompressiveAxialLoadSpiral;
		/// <inheritdoc cref="PhiMaximumCompressiveAxialLoadTied"/>
		protected double _phiMaximumCompressiveAxialLoadTied;
        /// <inheritdoc cref="PhiMaximumCompressiveAxialLoadComposite"/>
        protected double _phiMaximumCompressiveAxialLoadComposite;
        /// <inheritdoc cref="PhiT"/>
        protected double _phiT;
		/// <inheritdoc cref="PhiTP"/>
		protected double _phiTP;
		/// <inheritdoc cref="PhiDeformationTransitionIncrement"/>
		protected double _phiDeformationTransitionIncrement;
		/// <inheritdoc cref="PhiDeformationTransitionIncrementPrestress"/>
		protected double _phiDeformationTransitionIncrementPrestress;
		/// <inheritdoc cref="ConcreteStrengthReductionFactor"/>
		protected double _concreteStrengthReductionFactor;
		/// <inheritdoc cref="PhiDeformationTransitionMaxLimit"/>
		protected double _phiDeformationTransitionMaxLimit;

		#endregion

		#region Properties

		/// <summary>
		/// Resistance factor for compression-controlled sections with spirals
		/// </summary>
		public double PhiCSpiral { get => _phiCSpiral; set => _phiCSpiral = value; }

		/// <summary>
		/// Resistance factor for compression-controlled sections with ties
		/// </summary>
		public double PhiCTied { get => _phiCTied; set => _phiCTied = value; }

		/// <summary>
		/// Resistance factor for maximum compressive axial load sections with spirals
		/// </summary>
		public double PhiMaximumCompressiveAxialLoadSpiral { get => _phiMaximumCompressiveAxialLoadSpiral; set => _phiMaximumCompressiveAxialLoadSpiral = value; }

		/// <summary>
		/// Resistance factor for maximum compressive axial load sections with ties
		/// </summary>
		public double PhiMaximumCompressiveAxialLoadTied { get => _phiMaximumCompressiveAxialLoadTied; set => _phiMaximumCompressiveAxialLoadTied = value; }

        /// <summary>
        /// Resistance factor for maximum compressive axial load in composite sections.
        /// </summary>
        public double PhiMaximumCompressiveAxialLoadComposite { get => _phiMaximumCompressiveAxialLoadComposite; set => _phiMaximumCompressiveAxialLoadComposite = value; }

        /// <summary>
        /// Resistance factor for tension-controlled concrete sections
        /// </summary>
        public double PhiT { get => _phiT; set => _phiT = value; }

		/// <summary>
		/// Resistance factor for tension-controlled prestressed concrete sections
		/// </summary>
		public double PhiTP { get => _phiTP; set => _phiTP = value; }

		/// <summary>
		/// Compression-controlled strain increment for transition limit for section
		/// </summary>
		public double PhiDeformationTransitionIncrement { get => _phiDeformationTransitionIncrement; set => _phiDeformationTransitionIncrement = value; }

		/// <summary>
		/// Compression-controlled strain increment for transition limit for prestressed section
		/// </summary>
		public double PhiDeformationTransitionIncrementPrestress { get => _phiDeformationTransitionIncrementPrestress; set => _phiDeformationTransitionIncrementPrestress = value; }

		/// <summary>
		/// Concrete strength reduction factor for stress block compression stress-strain diagram
		/// </summary>
		public double ConcreteStrengthReductionFactor { get => _concreteStrengthReductionFactor; set => _concreteStrengthReductionFactor = value; }

		/// <summary>
		/// Compression-controlled strain limit for section
		/// </summary>
		public double PhiDeformationTransitionMaxLimit { get => _phiDeformationTransitionMaxLimit; set => _phiDeformationTransitionMaxLimit = value; }

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
        public StandardACI318(string name = "ACI 318", string remarks = "Building Code Requirements for Structural Concrete: ACI Standard ACI 318")
			: base(name, remarks)
		{
			_phiCSpiral = 0.75;
			_phiCTied = 0.65;
			_phiMaximumCompressiveAxialLoadSpiral = 0.85;
			_phiMaximumCompressiveAxialLoadTied = 0.80;
            _phiMaximumCompressiveAxialLoadComposite = 0.85;
            _phiT = 0.90;
			_phiTP = 1.0;
			_phiDeformationTransitionIncrement = 0.003;
			_phiDeformationTransitionIncrementPrestress = 0.002;
			_concreteStrengthReductionFactor = 0.85;
			_phiDeformationTransitionMaxLimit = 0.005;
		}

		/// <summary>
		/// Creates the standard with the default remarks
		/// </summary>
		/// <param name="name">The name</param>
		public StandardACI318(string name = "ACI 318")
			: this(name, "Building Code Requirements for Structural Concrete: ACI Standard ACI 318")
		{
		}

		/// <summary>
		/// Creates the standard with the default name and remarks
		/// </summary>
		public StandardACI318()
			: this("ACI 318", "Building Code Requirements for Structural Concrete: ACI Standard ACI 318")
		{
		}

		/// <summary>
		/// Deserialization constructor
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		protected StandardACI318(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			int version;
			try
			{
				version = info.GetInt32("StandardACI318Version");
			}
			catch (Exception)
			{
				version = 1;
			}

			_phiCSpiral = info.GetDouble("PhiCSpiral");
			_phiCTied = info.GetDouble("PhiCTied");
			_phiMaximumCompressiveAxialLoadSpiral = info.GetDouble("PhiSpiral");
			_phiMaximumCompressiveAxialLoadTied = info.GetDouble("PhiTied");
			_phiT = info.GetDouble("PhiT");
			_phiTP = info.GetDouble("PhiTP");
			_phiDeformationTransitionIncrement = info.GetDouble("PhiDeformationTransitionIncrement");
			_phiDeformationTransitionIncrementPrestress = info.GetDouble("PhiDeformationTransitionIncrementPrestress");

			if (version >= 2)
            {
                _concreteStrengthReductionFactor = info.GetDouble("ConcreteStrengthReductionFactor");
                _phiDeformationTransitionMaxLimit = info.GetDouble("PhiDeformationTransitionMaxLimit");
            }
			else
            {
                _concreteStrengthReductionFactor = 0.85;
                _phiDeformationTransitionMaxLimit = 0.005;
            }
            if (version >= 3)
            {
                _phiMaximumCompressiveAxialLoadComposite = info.GetDouble("PhiComposite");
            }
			else
            {
				_phiMaximumCompressiveAxialLoadComposite = 0.85;
            }
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
			return Equals(obj as StandardACI318);
		}

		/// <summary>
		/// Equality of the coefficients and of the base
		/// </summary>
		/// <param name="other">The object to compare</param>
		/// <returns>True if the objects are equal</returns>
		public bool Equals(StandardACI318 other)
		{
			return other != null &&
				   base.Equals(other) &&
				   _phiCSpiral == other._phiCSpiral &&
				   _phiCTied == other._phiCTied &&
				   _phiT == other._phiT &&
                   _phiMaximumCompressiveAxialLoadSpiral == other._phiMaximumCompressiveAxialLoadSpiral &&
                   _phiMaximumCompressiveAxialLoadTied == other._phiMaximumCompressiveAxialLoadTied &&
                   _phiMaximumCompressiveAxialLoadComposite == other._phiMaximumCompressiveAxialLoadComposite &&
                   _phiDeformationTransitionIncrement == other._phiDeformationTransitionIncrement;
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
				hashCode = hashCode * -17 + _phiCSpiral.GetHashCode();
				hashCode = hashCode * -17 + _phiCTied.GetHashCode();
				hashCode = hashCode * -17 + _phiT.GetHashCode();
				hashCode = hashCode * -17 + _phiDeformationTransitionIncrement.GetHashCode();
				hashCode = hashCode * -17 + _phiDeformationTransitionIncrementPrestress.GetHashCode();
				hashCode = hashCode * -17 + _phiMaximumCompressiveAxialLoadSpiral.GetHashCode();
				hashCode = hashCode * -17 + _phiMaximumCompressiveAxialLoadTied.GetHashCode();
                hashCode = hashCode * -17 + _phiMaximumCompressiveAxialLoadComposite.GetHashCode();
                hashCode = hashCode * -17 + _concreteStrengthReductionFactor.GetHashCode();
				hashCode = hashCode * -17 + _phiDeformationTransitionMaxLimit.GetHashCode();
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

			double version = 3;
			info.AddValue("StandardACI318Version", version);

			info.AddValue("PhiCSpiral", _phiCSpiral);
			info.AddValue("PhiCTied", _phiCTied);
			info.AddValue("PhiSpiral", _phiMaximumCompressiveAxialLoadSpiral);
			info.AddValue("PhiTied", _phiMaximumCompressiveAxialLoadTied);
            info.AddValue("PhiComposite", _phiMaximumCompressiveAxialLoadComposite);
            info.AddValue("PhiT", _phiT);
			info.AddValue("PhiTP", _phiTP);
			info.AddValue("PhiDeformationTransitionIncrement", _phiDeformationTransitionIncrement);
			info.AddValue("PhiDeformationTransitionIncrementPrestress", _phiDeformationTransitionIncrementPrestress);
			info.AddValue("ConcreteStrengthReductionFactor", _concreteStrengthReductionFactor);
			info.AddValue("PhiDeformationTransitionMaxLimit", _phiDeformationTransitionMaxLimit);
		}

		#endregion
	}
}
