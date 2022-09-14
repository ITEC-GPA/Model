using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
    /// <summary>
    /// This class collects all the coefficient of the NS EN 1992-1-1
    /// "Allmenne regler og regler for bygninger"
    /// </summary>
    /// <remarks>Reference: NS EN 1992-1-1:2004.AC: 2010</remarks>
    [Serializable]
    public class StandardNSEN1992p11 : StandardEN1992p11, ISerializable
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public StandardNSEN1992p11(string name = "NS EN 1992-1-1", string remarks = "Eurocode 2: Design of concrete structures - Part 1-1: General rules and rules for buildings. NS EN 1992-1-1:2005")
            : base(name, remarks)
        {
            _alphaCC = 0.85;
        }

        public StandardNSEN1992p11(string name = "NS EN 1992-1-1")
            : this(name, "Eurocode 2: Design of concrete structures - Part 1-1: General rules and rules for buildings. NS EN 1992-1-1:2005")
        {

        }

        public StandardNSEN1992p11()
            : this("NS EN 1992-1-1", "Eurocode 2: Design of concrete structures - Part 1-1: General rules and rules for buildings. NS EN 1992-1-1:2005")
        {

        }

        protected StandardNSEN1992p11(SerializationInfo info, StreamingContext context)
            :base(info, context)
        {

        }

        public override bool Equals(object obj)
		{
			return obj is StandardNSEN1992p11 standard && base.Equals(standard);
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
