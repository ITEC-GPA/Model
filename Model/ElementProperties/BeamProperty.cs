using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GPC.Model.ElementProperties
{
    /// <summary>
    /// Base of the properties of the beam elements (the sections)
    /// </summary>
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    [Serializable]
    public abstract class BeamProperty : ElementProperty, ISerializable
    {
        #region Variables


        #endregion

        #region Properties


        #endregion

        /// <summary>
        /// Creates a beam property
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        public BeamProperty(string name, int id = IDUNASSIGNED)
            : base(name, id)
        {

        }

        /// <summary>
        /// Deserialization constructor (see <see cref="ElementProperty"/>)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected BeamProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        /// <summary>
        /// Serializes the data of <see cref="ElementProperty"/>
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// The text shown by the debugger
        /// </summary>
        /// <returns>"BeamProperty: " and the name</returns>
        private string GetDebuggerDisplay()
        {
            return $"BeamProperty: {_name}";
        }

        /// <summary>
        /// Equality with another beam property: same name
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is a beam property with the same name</returns>
        public override bool Equals(object obj)
        {
            return (obj is BeamProperty objCasted) && base.Equals(objCasted);
        }

        /// <summary>
        /// The hash code of the name
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
    }
}
