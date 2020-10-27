using System;
using System.Runtime.Serialization;

namespace GPC.Model
{
    [Serializable]
    public abstract class ModelObject
    {
        #region Variables

        protected Guid _guid;

        #endregion 

        #region Properties

        public Guid Guid => _guid;

        #endregion

        #region Public Constructors

        public ModelObject(Guid guid)
        {
            _guid = guid;
        }

        public ModelObject(SerializationInfo info, StreamingContext context)
        {
            _guid = (Guid)info.GetValue("Guid", typeof(Guid));
        }

        #endregion

        #region Public Methods Specific

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Guid", _guid);
        }

        #endregion
    }
}