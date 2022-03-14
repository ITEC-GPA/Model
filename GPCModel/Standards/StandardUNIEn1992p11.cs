using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
    /// <summary>
    /// This class collects all the coefficient of the UNI EN 1992-1-1
    /// "Progettazione delle strutture di calcestruzzo"
    /// </summary>
    /// <remarks>Reference: UNI EN 1992-1-1:2005</remarks>
    [Serializable]
    public class StandardUNIEn1992p11 : StandardEN1992p11, ISerializable
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public StandardUNIEn1992p11(string name = "UNI EN 1992-1-1", string remarks = "Eurocode 2: Design of concrete structures - Part 1-1: General rules and rules for buildings. UNI EN 1992-1-1:2005")
            : base(name, remarks)
        {

        }

        public StandardUNIEn1992p11(string name = "UNI EN 1992-1-1")
            : this(name, "Eurocode 2: Design of concrete structures - Part 1-1: General rules and rules for buildings. UNI EN 1992-1-1:2005")
        {

        }

        public StandardUNIEn1992p11()
            : this("UNI EN 1992-1-1", "Eurocode 2: Design of concrete structures - Part 1-1: General rules and rules for buildings. UNI EN 1992-1-1:2005")
        {

        }

        protected StandardUNIEn1992p11(SerializationInfo info, StreamingContext context)
            :base(info, context)
        {

        }

        public override bool Equals(object obj)
		{
			return obj is StandardUNIEn1992p11 p &&
				   base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return 624022166 + base.GetHashCode();
		}
	}
}
