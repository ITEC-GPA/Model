using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GPC.Model.ElementProperties
{
    /// <summary>
    /// Base of the properties of the plate elements
    /// </summary>
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    [Serializable]
    public abstract class PlateProperty : ElementProperty, ISerializable
    {
        /// <summary>Physical thickness in mm, independent of equivalent FEM stiffness thicknesses. Null means unknown.</summary>
        public double? PhysicalThickness { get; set; }

        #region Variables

        #endregion

        #region Properties

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a plate property
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        public PlateProperty(string name, int id = IDUNASSIGNED)
            : base(name, id)
        {

        }

        /// <summary>
        /// Deserialization constructor (see <see cref="ElementProperty"/>)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected PlateProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PhysicalThickness = SerializationFields.Read<double?>(info, "PhysicalThickness");
        }

        #endregion

        #region Equals, HasCode and operators

        /// <summary>
        /// Serializes the data of <see cref="ElementProperty"/>
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            if (PhysicalThickness.HasValue) info.AddValue("PhysicalThickness", PhysicalThickness);
        }

        /// <summary>
        /// Equality with another plate property: same name
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is a plate property with the same name</returns>
        public override bool Equals(object obj)
        {
            return (obj is PlateProperty objCasted) && GetType() == obj.GetType() && PhysicalThickness == objCasted.PhysicalThickness &&
                base.Equals(objCasted);
        }

        /// <summary>
        /// A constant hash code
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                return hashCode;
            }
        }

        /// <summary>
        /// The text shown by the debugger
        /// </summary>
        /// <returns>"PlateProperty: " and the name</returns>
        private string GetDebuggerDisplay()
        {
            return $"PlateProperty: {_name}";
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first property (not null: a null first operand throws <see cref="NullReferenceException"/>)</param>
        /// <param name="obj2">The second property</param>
        /// <returns>True if the properties are equal</returns>
        public static bool operator ==(PlateProperty obj1, PlateProperty obj2)
        {
            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first property (not null)</param>
        /// <param name="obj2">The second property</param>
        /// <returns>True if the properties are different</returns>
        public static bool operator !=(PlateProperty obj1, PlateProperty obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
