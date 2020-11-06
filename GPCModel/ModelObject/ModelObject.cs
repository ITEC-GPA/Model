using System;
using System.Runtime.Serialization;

namespace GPC.Model
{
    [Serializable]
    public abstract class ModelObject
    {
        #region Variables
        /// <summary>
        /// <param name="_guid"> Object GUID</param>
        /// <param name="_name"> Object name</param>
        /// </summary>
        protected Guid _guid;
        protected string _name;
        #endregion 

        #region Properties

        public Guid Guid => _guid;
        public string Name { get => _name; set { _name = value;  } }
        #endregion

        #region Public Constructors

        public ModelObject(Guid guid)
        {
            if(guid == Guid.Empty)
            {
                _guid = new Guid();
            }
            else
            {
                _guid = guid;
            }
        }
        public ModelObject(Guid guid, string name)
            : this (guid)
        {
            _name = name;
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