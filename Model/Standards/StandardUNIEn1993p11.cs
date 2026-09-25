using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
    /// <summary>
    /// UNI EN 1993: Eurocode 3 - Italian
    /// </summary>
    [Serializable]
    public class StandardUNIEN1993p11 : StandardEN1993p11, ISerializable
    {
        /// <summary>
        /// Creates the standard
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="remarks">The remarks</param>
        public StandardUNIEN1993p11(string name = "UNI EN 1993", string remarks = "Eurocode 3 - Italian")
            : base(name, remarks)
        {
            _gammaM0 = 1.05;
            _gammaM1 = 1.10;
            _gammaM2 = 1.25;
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected StandardUNIEN1993p11(SerializationInfo info, StreamingContext context)
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
            return obj is StandardUNIEN1993p11 p &&
                   base.Equals(obj);
        }

        /// <summary>
        /// The hash code of the coefficients and of the base
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return 624022166 + base.GetHashCode();
        }

    }
}
