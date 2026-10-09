using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model
{
    /// <summary>
    /// Base of the objects of the model: a <see cref="Guid"/> and a <see cref="Name"/>. Two objects are equal if they have the same name
    /// </summary>
    [Serializable]
    public abstract class ModelObject : ISerializable
    {
        #region Variables

        /// <summary>
        /// The unique identifier
        /// </summary>
        protected Guid _guid;

        /// <summary>
        /// The name
        /// </summary>
        protected string _name;

        /// <summary>
        /// The version of the serialized data (see <see cref="SerializationVersion"/>)
        /// </summary>
        private int _serializationVersion;

        #endregion

        #region Properties

        /// <summary>
        /// Default value is zero.
        /// Increment this parameter if you have modified a class already serialized. Then handle the deserialiazation in the constructor
        /// </summary>
        protected int SerializationVersion { get => _serializationVersion; set => _serializationVersion = value; }

        /// <summary>
        /// The unique identifier (assigned at the creation or read from the serialized data)
        /// </summary>
        public Guid Guid => _guid;

        /// <summary>
        /// The name. Registered name indexes are updated atomically, rejecting collisions before mutation.
        /// Legacy Equals/GetHashCode still compare content: use <see cref="ModelObjectIdentityComparer"/> for entity hash sets.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                Collections.NameIndexRegistry.Rename(this, value, () => _name = value);
            }
        }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates an object with a new Guid and no name
        /// </summary>
        public ModelObject()
        {
            _guid = Guid.NewGuid();
        }

        /// <summary>
        /// Creates an object with a given Guid and no name
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public ModelObject(Guid guid)
        {
            _guid = guid;
        }

        /// <summary>
        /// Creates an object with a new Guid
        /// </summary>
        /// <param name="name">Object name</param>
        public ModelObject(string name)
        {
            _name = name;
            _guid = Guid.NewGuid();
        }

        /// <summary>
        /// Creates an object with a given Guid
        /// </summary>
        /// <param name="guid">Object GUID</param>
        /// <param name="name">Object name</param>
        public ModelObject(Guid guid, string name)
            : this(guid)
        {
            _name = name;
        }

        /// <summary>
        /// Deserialization constructor: reads the serialization version (0 if it is missing), the Guid and the name
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected ModelObject(SerializationInfo info, StreamingContext context)
        {
            try
            {
                // se va in eccezione stai deserializzando un file senza la versione salvata.
                // impostiamo a zero che rappresenta la prima versione del file 
                _serializationVersion = info.GetInt32("SerializationVersion");
            }
            catch (SerializationException)
            {
                _serializationVersion = 0;
            }

            _guid = (Guid)info.GetValue("Guid", typeof(Guid));
            _name = info.GetString("Name");
        }

        #endregion 

        /// <summary>
        /// Compares the Guid of the object with a given one
        /// </summary>
        /// <param name="guid">The Guid to compare</param>
        /// <returns><see langword="true"/> if <paramref name="guid"/> match the object <see cref="Guid"/></returns>
        public bool CompareGuid(Guid guid)
        {
            return _guid.Equals(guid);
        }

        #region Equals - HashCode - Operators

        /// <summary>
        /// Serializes the serialization version, the Guid and the name
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("SerializationVersion", _serializationVersion);
            info.AddValue("Guid", _guid);
            info.AddValue("Name", _name);
        }

        /// <summary>
        /// Equality of the names
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="ModelObject"/> with the same <see cref="Name"/> of this object</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            return (obj is ModelObject objCasted) && objCasted._name == _name;
        }

        /// <summary>
        /// The hash code of the name
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return -391 * EqualityComparer<string>.Default.GetHashCode(_name);
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null objects are equal
        /// </summary>
        /// <param name="obj1">The first object</param>
        /// <param name="obj2">The second object</param>
        /// <returns>True if the objects have the same name</returns>
        public static bool operator ==(ModelObject obj1, ModelObject obj2)
        {
            if (obj1 is null)
                return obj2 is null;

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first object</param>
        /// <param name="obj2">The second object</param>
        /// <returns>True if the objects have different names</returns>
        public static bool operator !=(ModelObject obj1, ModelObject obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion

        #region CUSTOM EQUALITY COMPARER

        /// <summary>
        /// Compare two <see cref="ModelObject"/> using only <see cref="ModelObject.Name"/> as equality parameter
        /// </summary>
        [Serializable]
        public class ModelObjectNameEqualityComparer : IEqualityComparer<ModelObject>
        {
            /// <summary>
            /// Equality of the names
            /// </summary>
            /// <param name="x">The first object</param>
            /// <param name="y">The second object</param>
            /// <returns>True if the objects have the same name, or if both <paramref name="x"/> and <paramref name="y"/> are null</returns>
            /// <remarks>Only <see cref="ModelObject.Name"/> is used as equality parameter</remarks>
            bool IEqualityComparer<ModelObject>.Equals(ModelObject x, ModelObject y)
            {
                if (x == null && y == null)
                    return true;

                if (x == null || y == null)
                    return false;

                if (ReferenceEquals(x, y))
                    return true;

                if (x.Name.Equals(y.Name))
                    return true;

                return false;
            }


            /// <summary>
            /// The hash code of the name
            /// </summary>
            /// <param name="obj">The object (with a name not null)</param>
            /// <returns>The hash code</returns>
            /// <remarks>Only <see cref="ModelObject.Name"/> is used as equality parameter</remarks>
            int IEqualityComparer<ModelObject>.GetHashCode(ModelObject obj)
            {
                return -17 * obj.Name.GetHashCode();
            }
        }

        #endregion CUSTOM EQUALITY COMPARER
    }
}
