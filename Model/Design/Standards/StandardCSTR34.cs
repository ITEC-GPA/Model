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

		/// <summary>
		/// Creates the standard with the default remarks
		/// </summary>
		/// <param name="name">The name</param>
		public StandardCSTR34(string name = "CS-TR34")
			: this(name, "TR 34: Concrete Industrial Ground Floors. CS-TR34:2103")
		{

		}

		/// <summary>
		/// Creates the standard with the default name and remarks
		/// </summary>
		public StandardCSTR34()
			: this("CS-TR34", "TR 34: Concrete Industrial Ground Floors. CS-TR34:2103")
		{

		}

		/// <summary>
		/// Deserialization constructor
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		protected StandardCSTR34(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

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

			return obj is StandardCSTR34 standard && base.Equals(standard);
		}

		/// <summary>
		/// The hash code of the coefficients and of the base
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return 23 + base.GetHashCode();
			}
		}
	}
}
