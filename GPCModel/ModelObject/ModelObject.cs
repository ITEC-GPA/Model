using System;
using System.Collections.Generic;
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
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._name.Equals(_name);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as ModelObject);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + EqualityComparer<string>.Default.GetHashCode(_name);
            return hashCode;
        }

        public static bool operator ==(ModelObject obj1, ModelObject obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ModelObject obj1, ModelObject obj2)
        {
            return !(obj1 == obj2);
        } 


        #endregion

    }
}