using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
    /// <summary>
    /// This class collects all the coefficient of the ANSI/AISC 360-10
    /// </summary>
    public class StandardAISC360p10 : StandardAISC
    {
        #region Variables

        #endregion

        #region Properties

        #endregion

        #region Constructors

        public StandardAISC360p10(string name = "ANSI/AISC 360-10", string remarks = "Specification for Structural Steel Buildings")
            : base(name, remarks)
        {

        }

        public StandardAISC360p10(string name = "ANSI/AISC 360-10")
            : this(name, "Specification for Structural Steel Buildings")
        {
        }

        public StandardAISC360p10()
            : this("ANSI/AISC 360-10", "Specification for Structural Steel Buildings")
        {
        }

        protected StandardAISC360p10(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("StandardAISC360_10Version");
            }
            catch (Exception)
            {
                version = 1;
            }
        }

        #endregion

        #region Equals - hashcode - operators

        public override bool Equals(object obj)
        {
            return Equals(obj as StandardAISC360p10);
        }

        public bool Equals(StandardAISC360p10 other)
        {
            return other != null &&
                   base.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                return hashCode;
            }
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 1;
            info.AddValue("StandardAISC360_10Version", version);
        }

        #endregion
    }
}
