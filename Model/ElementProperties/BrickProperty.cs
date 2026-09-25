using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GPC.Model.ElementProperties
{
    /// <summary>
    /// Base of the properties of the brick (solid) elements
    /// </summary>
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    [Serializable]
    public abstract class BrickProperty : ElementProperty, ISerializable
    {
        #region Variables

        #endregion

        #region Properties

        #endregion

        #region Public Constructor

        /// <summary>
        /// Creates a brick property
        /// </summary>
        /// <param name="name">The name</param>
        public BrickProperty(string name)
            : base(name)
        {

        }

        /// <summary>
        /// Deserialization constructor (see <see cref="ElementProperty"/>)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected BrickProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #endregion

        #region Public Methods

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
        /// <returns>"BrickProperty: " and the name</returns>
        private string GetDebuggerDisplay()
        {
            return $"BrickProperty: {_name}";
        }

        #endregion
    }
}