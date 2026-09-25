using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
	/// <summary>
	/// This class collects all the coefficient of the Eurocode2 Standard
	/// </summary>
	/// <remarks>Reference: EN 1992-1-1:2004/AC:2010</remarks>
	[Serializable]
	public class StandardEN1992p11 : StandardModelCode2010, ISerializable
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		public StandardEN1992p11(string name = "EN 1992-1-1", string remarks = "Eurocode 2: Design of concrete structures - Part 1-1: General rules and rules for buildings. EN 1992-1-1:2004/AC:2010")
			: base(name, remarks)
		{

		}

		/// <summary>
		/// Creates the standard with the default remarks
		/// </summary>
		/// <param name="name">The name</param>
		public StandardEN1992p11(string name = "EN 1992-1-1")
			: this(name, "Eurocode 2: Design of concrete structures - Part 1-1: General rules and rules for buildings. EN 1992-1-1:2004/AC:2010")
		{

		}

		/// <summary>
		/// Creates the standard with the default name and remarks
		/// </summary>
		public StandardEN1992p11()
			: this("EN 1992-1-1", "Eurocode 2: Design of concrete structures - Part 1-1: General rules and rules for buildings. EN 1992-1-1:2004/AC:2010")
		{

		}

		/// <summary>
		/// Deserialization constructor
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		protected StandardEN1992p11(SerializationInfo info, StreamingContext context)
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

			return obj is StandardEN1992p11 standard && base.Equals(standard);
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
