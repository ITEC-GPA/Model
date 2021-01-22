using System;
using System.Runtime.Serialization;

namespace GPC.Model
{
    [Serializable]
    public abstract class ModelObject : IEquatable<ModelObject>
    {
        #region Variables

        protected Guid _guid;

        protected string _name;

        #endregion 

        #region Properties

        public Guid Guid => _guid;

        public string Name { get => _name; set { _name = value; } }

        #endregion

        #region Public Constructors

        /// <summary>
        /// <param name="guid"> Object GUID</param>
        /// </summary>
        public ModelObject()
        {
            _guid = Guid.NewGuid();
        }

        /// <summary>
        /// <param name="guid"> Object GUID</param>
        /// </summary>
        public ModelObject(Guid guid)
        {
            _guid = guid;
        }

        /// <summary>
        /// <param name="guid"> Object GUID</param>
        /// <param name="name"> Object name</param>
        /// </summary>
        public ModelObject(Guid guid, string name)
            : this(guid)
        {
            _name = name;
        }

        public ModelObject(SerializationInfo info, StreamingContext context)
        {
            _guid = (Guid)info.GetValue("Guid", typeof(Guid));
            _name = info.GetString("Name");
        }

        #endregion 

        #region Public Methods Specific

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Guid", _guid);
            info.AddValue("Name", _name);
        }

        public bool Equals(ModelObject other)
        {
            return !(other is null) && other._name == _name;
        }

        #endregion 
    }
}