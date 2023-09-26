using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace GPC.Model.Standards
{
	/// <summary>
	/// This class collects all the coefficient of the AISC
	/// </summary>
	public abstract class StandardAISC : Standard
	{
		#region Variables

		protected double _phiBending;
		protected double _phiCompression;
		protected double _phiTensionYielding;
		protected double _phiTensionFracture;
		protected double _phiShear;
		protected double _phiShearShortWeberRolledI;
		protected double _phiTorsion;

		#endregion

		#region Properties

		public double PhiBending { get => _phiBending; set => _phiBending = value; }

		public double PhiCompression { get => _phiCompression; set => _phiCompression = value; }

		public double PhiTensionYielding { get => _phiTensionYielding; set => _phiTensionYielding = value; }

		public double PhiTensionFracture { get => _phiTensionFracture; set => _phiTensionFracture = value; }

		public double PhiShear { get => _phiShear; set => _phiShear = value; }

		public double PhiShearShortWeberRolledI { get => _phiShearShortWeberRolledI; set => _phiShearShortWeberRolledI = value; }

		public double PhiTorsion { get => _phiTorsion; set => _phiTorsion = value; }

		#endregion

		#region Constructors

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

		public StandardAISC(string name = "AISC")
			: this(name, "Specification for Structural Steel Buildings")
		{
		}

		public StandardAISC()
			: this("AISC", "Specification for Structural Steel Buildings")
		{
		}

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
		}

		#endregion

		#region Equals - hashcode - operators

		public override bool Equals(object obj)
		{
			return Equals(obj as StandardAISC);
		}

		public bool Equals(StandardAISC other)
		{
			return other != null &&
				   base.Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 23;
				hashCode = hashCode * -17 + base.GetHashCode();
				return hashCode;
			}
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			double version = 0;
			info.AddValue("StandardAISCVersion", version);
		}

		#endregion
	}
}
