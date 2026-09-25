using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
	/// <summary>
	/// This class collects all the coefficient of the DS EN 1992-1-1
	/// "Generelle regler samt regler for bygningskonstruktioner"
	/// </summary>
	/// <remarks>Reference: DS EN 1992-1-1:2004.AC: 2021</remarks>
	[Serializable]
	public class StandardDSEN1992p11 : StandardEN1992p11, ISerializable
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		public StandardDSEN1992p11(string name = "DS EN 1992-1-1", string remarks = "Eurocode 2: Design of concrete structures - Part 1-1: General rules and rules for buildings. DS EN 1992-1-1:2005")
			: base(name, remarks)
		{
			_gammaC = 1.4;
			_gammaS = 1.2;
		}

		/// <summary>
		/// Creates the standard with the default remarks
		/// </summary>
		/// <param name="name">The name</param>
		public StandardDSEN1992p11(string name = "DS EN 1992-1-1")
			: this(name, "Eurocode 2: Design of concrete structures - Part 1-1: General rules and rules for buildings. DS EN 1992-1-1:2005")
		{

		}

		/// <summary>
		/// Creates the standard with the default name and remarks
		/// </summary>
		public StandardDSEN1992p11()
			: this("DS EN 1992-1-1", "Eurocode 2: Design of concrete structures - Part 1-1: General rules and rules for buildings. DS EN 1992-1-1:2005")
		{

		}

		/// <summary>
		/// Deserialization constructor
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		protected StandardDSEN1992p11(SerializationInfo info, StreamingContext context)
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
			return obj is StandardDSEN1992p11 standard && base.Equals(standard);
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
