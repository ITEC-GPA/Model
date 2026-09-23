using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GPC.Model.ElementProperties
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    [Serializable]
    public abstract class PlateProperty : ElementProperty, ISerializable
    {
        #region Variables

        #endregion

        #region Properties

        #endregion

        #region Public Constructors

        /// <summary>
        /// <param name="name"></param>
        /// </summary>
        public PlateProperty(string name, int id = IDUNASSIGNED)
            : base(name, id)
        {

        }

        protected PlateProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Equals, HasCode and operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public override bool Equals(object obj)
        {
            return (obj is PlateProperty objCasted) &&
                base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                return hashCode;
            }
        }

        private string GetDebuggerDisplay()
        {
            return $"PlateProperty: {_name}";
        }

        public static bool operator ==(PlateProperty obj1, PlateProperty obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(PlateProperty obj1, PlateProperty obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
