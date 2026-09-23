using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
	/// <summary>
	/// This class collects all the coefficient of the CS-TR34 Standard
	/// </summary>
	/// <remarks>Reference: CS-TR34:2103</remarks>
	[Serializable]
	public class StandardCSTR34 : StandardModelCode2010, ISerializable
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		public StandardCSTR34(string name = "CS-TR34", string remarks = "TR 34: Concrete Industrial Ground Floors. CS-TR34:2103")
			: base(name, remarks)
		{
			_gammaF = 1.0;
			_alphaCC = 0.85;
			_alphaCT = 0.85;
		}

		public StandardCSTR34(string name = "CS-TR34")
			: this(name, "TR 34: Concrete Industrial Ground Floors. CS-TR34:2103")
		{

		}

		public StandardCSTR34()
			: this("CS-TR34", "TR 34: Concrete Industrial Ground Floors. CS-TR34:2103")
		{

		}

		protected StandardCSTR34(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
				return true;

			return obj is StandardCSTR34 standard && base.Equals(standard);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return 23 + base.GetHashCode();
			}
		}
	}
}
