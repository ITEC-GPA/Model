using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements
{
    /// <summary>
    /// Element base abstract class that is the base for all the objects inside GPC environment.
    /// </summary>

    [Serializable]
    public abstract class Element : ModelObject
    {
        #region Variables
        protected Guid _guid;
        protected string _id;
        #endregion

        #region Properties
        public Guid Guid => _guid;
        #endregion

        #region Public Constructors
        protected Element(Guid guid)
            : base(guid)
        {
            _guid = guid;
        }

        protected Element(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Public Methods Override
        /*public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }*/
        #endregion

        #region Public Methods Specific
        #endregion

        #region Private Methods Specific
        #endregion
    }
}
