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

		public StandardDSEN1992p11(string name = "DS EN 1992-1-1")
			: this(name, "Eurocode 2: Design of concrete structures - Part 1-1: General rules and rules for buildings. DS EN 1992-1-1:2005")
		{

		}

		public StandardDSEN1992p11()
			: this("DS EN 1992-1-1", "Eurocode 2: Design of concrete structures - Part 1-1: General rules and rules for buildings. DS EN 1992-1-1:2005")
		{

		}

		protected StandardDSEN1992p11(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}

		public override bool Equals(object obj)
		{
			return obj is StandardDSEN1992p11 standard && base.Equals(standard);
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
