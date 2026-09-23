using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
    /// <summary>
    /// This class collects all the coefficient of the DIN EN 1992-1-1
    /// "Bemessung und Konstruktion von Stahlbeton- und Spannbetontragwerken"
    /// </summary>
    /// <remarks>Reference: DIN EN 1992-1-1:2004.AC: 2010</remarks>
    [Serializable]
    public class StandardDINEN1992p11 : StandardEN1992p11, ISerializable
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public StandardDINEN1992p11(string name = "DIN EN 1992-1-1", string remarks = "Eurocode 2: Design of concrete structures - Part 1-1: General rules and rules for buildings. DIN EN 1992-1-1:2005")
            : base(name, remarks)
        {
            _alphaCC = 0.85;
        }

        public StandardDINEN1992p11(string name = "DIN EN 1992-1-1")
            : this(name, "Eurocode 2: Design of concrete structures - Part 1-1: General rules and rules for buildings. DIN EN 1992-1-1:2005")
        {

        }

        public StandardDINEN1992p11()
            : this("DIN EN 1992-1-1", "Eurocode 2: Design of concrete structures - Part 1-1: General rules and rules for buildings. DIN EN 1992-1-1:2005")
        {

        }

        protected StandardDINEN1992p11(SerializationInfo info, StreamingContext context)
            :base(info, context)
        {

        }

        public override bool Equals(object obj)
		{
			return obj is StandardDINEN1992p11 standard && base.Equals(standard);
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
