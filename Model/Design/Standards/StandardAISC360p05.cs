using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
    /// <summary>
    /// This class collects all the coefficient of the ANSI/AISC 360-05
    /// </summary>
    [Serializable]
    public class StandardAISC360p05 : StandardAISC
    {
        #region Variables

        #endregion

        #region Properties

        #endregion

        #region Constructors

        /// <summary>
        /// Creates the standard
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="remarks">The remarks</param>
        public StandardAISC360p05(string name = "ANSI/AISC 360-05", string remarks = "Specification for Structural Steel Buildings")
            : base(name, remarks)
        {

        }

        /// <summary>
        /// Creates the standard with the default remarks
        /// </summary>
        /// <param name="name">The name</param>
        public StandardAISC360p05(string name = "ANSI/AISC 360-05")
            : this(name, "Specification for Structural Steel Buildings")
        {
        }

        /// <summary>
        /// Creates the standard with the default name and remarks
        /// </summary>
        public StandardAISC360p05()
            : this("ANSI/AISC 360-05", "Specification for Structural Steel Buildings")
        {
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected StandardAISC360p05(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("StandardAISC360_05Version");
            }
            catch (Exception)
            {
                version = 1;
            }
        }

        #endregion

        #region Equals - hashcode - operators

        /// <summary>
        /// Equality with an object of the same type
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is equal</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as StandardAISC360p05);
        }

        /// <summary>
        /// Equality of the coefficients and of the base
        /// </summary>
        /// <param name="other">The object to compare</param>
        /// <returns>True if the objects are equal</returns>
        public bool Equals(StandardAISC360p05 other)
        {
            return other != null &&
                   base.Equals(other);
        }

        /// <summary>
        /// The hash code of the coefficients and of the base
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Serializes the object
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 1;
            info.AddValue("StandardAISC360_05Version", version);
        }

        #endregion
    }
}
