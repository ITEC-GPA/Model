using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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

        public StandardEN1992p11(string name = "EN 1992-1-1")
            : base(name, "Eurocode 2: Design of concrete structures - Part 1-1: General rules and rules for buildings. EN 1992-1-1:2004/AC:2010")
        {

        }

        public StandardEN1992p11()
            : base("EN 1992-1-1", "Eurocode 2: Design of concrete structures - Part 1-1: General rules and rules for buildings. EN 1992-1-1:2004/AC:2010")
        {

        }

        protected StandardEN1992p11(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return obj is StandardEN1992p11 standard && base.Equals(standard);
        }

        public override int GetHashCode()
        {
            return 624022166 + base.GetHashCode();
        }
    }
}
