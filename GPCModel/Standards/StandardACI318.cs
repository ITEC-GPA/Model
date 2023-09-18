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

		protected double _phiCSpiral;
		protected double _phiCTied;
		protected double _phiMaximumCompressiveAxialLoadSpiral;
		protected double _phiMaximumCompressiveAxialLoadTied;
		protected double _phiT;
		protected double _phiTP;
		protected double _phiDeformationTransitionIncrement;
		protected double _phiDeformationTransitionIncrementPrestress;
		protected double _concreteStrengthReductionFactor;
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

		#endregion

		#region Constructors

		public StandardACI318(string name = "ACI 318", string remarks = "Building Code Requirements for Structural Concrete: ACI Standard ACI 318")
			: base(name, remarks)
		{
			_phiCSpiral = 0.75;
			_phiCTied = 0.65;
			_phiMaximumCompressiveAxialLoadSpiral = 0.85;
			_phiMaximumCompressiveAxialLoadTied = 0.80;
			_phiT = 0.90;
			_phiTP = 1.0;
			_phiDeformationTransitionIncrement = 0.003;
			_phiDeformationTransitionIncrementPrestress = 0.002;
			_concreteStrengthReductionFactor = 0.85;
			_phiDeformationTransitionMaxLimit = 0.005;
		}

		public StandardACI318(string name = "ACI 318")
			: this(name, "Building Code Requirements for Structural Concrete: ACI Standard ACI 318")
		{
		}

		public StandardACI318()
			: this("ACI 318", "Building Code Requirements for Structural Concrete: ACI Standard ACI 318")
		{
		}

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
		}

		#endregion

		#region Equals - hashcode - operators

		public override bool Equals(object obj)
		{
			return Equals(obj as StandardACI318);
		}

		public bool Equals(StandardACI318 other)
		{
			return other != null &&
				   base.Equals(other) &&
				   _phiCSpiral == other._phiCSpiral &&
				   _phiCTied == other._phiCTied &&
				   _phiT == other._phiT &&
				   _phiDeformationTransitionIncrement == other._phiDeformationTransitionIncrement;
		}

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
				hashCode = hashCode * -17 + _concreteStrengthReductionFactor.GetHashCode();
				hashCode = hashCode * -17 + _phiDeformationTransitionMaxLimit.GetHashCode();
				return hashCode;
			}
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			double version = 2;
			info.AddValue("StandardACI318Version", version);

			info.AddValue("PhiCSpiral", _phiCSpiral);
			info.AddValue("PhiCTied", _phiCTied);
			info.AddValue("PhiSpiral", _phiMaximumCompressiveAxialLoadSpiral);
			info.AddValue("PhiTied", _phiMaximumCompressiveAxialLoadTied);
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
