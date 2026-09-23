using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GPC.Model.ElementProperties
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    [Serializable]
    public abstract class BrickProperty : ElementProperty, ISerializable
    {
        #region Variables

        #endregion

        #region Properties

        #endregion

        #region Public Constructor

        public BrickProperty(string name)
            : base(name)
        {

        }

        protected BrickProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #endregion

        #region Public Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        private string GetDebuggerDisplay()
        {
            return $"BrickProperty: {_name}";
        }

        #endregion
    }
}