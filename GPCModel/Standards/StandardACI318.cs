using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
	/// <summary>
	/// This class collects all the coefficient of the ACI 318
	/// </summary>
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

		#endregion

		#region Properties

		#endregion

		/// <summary>
		/// Resistance factor for compression-controlled sections with spirals
		/// </summary>
		public double PhiCSpiral => _phiCSpiral;

		/// <summary>
		/// Resistance factor for compression-controlled sections with ties
		/// </summary>
		public double PhiCTied => _phiCTied;

		/// <summary>
		/// Resistance factor for maximum compressive axial load sections with spirals
		/// </summary>
		public double PhiMaximumCompressiveAxialLoadSpiral => _phiMaximumCompressiveAxialLoadSpiral;

		/// <summary>
		/// Resistance factor for maximum compressive axial load sections with ties
		/// </summary>
		public double PhiMaximumCompressiveAxialLoadTied => _phiMaximumCompressiveAxialLoadTied;

		/// <summary>
		/// Resistance factor for tension-controlled concrete sections
		/// </summary>
		public double PhiT => _phiT;

		/// <summary>
		/// Resistance factor for tension-controlled prestressed concrete sections
		/// </summary>
		public double PhiTP => _phiTP;

		/// <summary>
		/// Compression-controlled strain limit for section
		/// </summary>
		public double PhiDeformationTransitionIncrement => _phiDeformationTransitionIncrement;

		/// <summary>
		/// Compression-controlled strain limit for prestressed section
		/// </summary>
		public double PhiDeformationTransitionIncrementPrestress => _phiDeformationTransitionIncrementPrestress;

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
			_phiCSpiral = info.GetDouble("PhiCSpiral");
			_phiCTied = info.GetDouble("PhiCTied");
			_phiMaximumCompressiveAxialLoadSpiral = info.GetDouble("PhiSpiral");
			_phiMaximumCompressiveAxialLoadTied = info.GetDouble("PhiTied");
			_phiT = info.GetDouble("PhiT");
			_phiDeformationTransitionIncrement = info.GetDouble("PhiDeformationTransitionIncrement");
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
				hashCode = hashCode * -17+ base.GetHashCode();
				hashCode = hashCode * -17+ _phiCSpiral.GetHashCode();
				hashCode = hashCode * -17+ _phiCTied.GetHashCode();
				hashCode = hashCode * -17+ _phiT.GetHashCode();
				hashCode = hashCode * -17+ _phiDeformationTransitionIncrement.GetHashCode();
				return hashCode;
			}
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("PhiCSpiral", _phiCSpiral);
			info.AddValue("PhiCTied", _phiCTied);
			info.AddValue("PhiSpiral", _phiMaximumCompressiveAxialLoadSpiral);
			info.AddValue("PhiTied", _phiMaximumCompressiveAxialLoadTied);
			info.AddValue("PhiT", _phiT);
			info.AddValue("PhiDeformationTransitionIncrement", _phiDeformationTransitionIncrement);
		}

		#endregion

		#region Public Setter

		public void SetPhiCSpiral(double phiCSpiral)
		{
			_phiCSpiral = phiCSpiral;
		}

		public void SetPhiCTied(double phiCTied)
		{
			_phiCTied = phiCTied;
		}

		public void SetPhiSpiral(double phiSpiral)
		{
			_phiMaximumCompressiveAxialLoadSpiral = phiSpiral;
		}

		public void SetPhiTied(double phiTied)
		{
			_phiMaximumCompressiveAxialLoadTied = phiTied;
		}

		public void SetPhiT(double phiT)
		{
			_phiT = phiT;
		}

		public void SetPhiDeformationTransitionIncrement(double phiDeformationTransitionIncrement)
		{
			_phiDeformationTransitionIncrement = phiDeformationTransitionIncrement;
		}

		#endregion
	}
}
