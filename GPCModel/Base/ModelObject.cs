using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model
{
    [Serializable]
    public abstract class ModelObject : ISerializable
    {

        #region Variables

        protected Guid _guid;

        protected string _name;

        #endregion 

        #region Properties

        public Guid Guid => _guid;

        public string Name => _name; // Setter non disponibile in quanto il nome deve essere una variabile non mutabile in modo da poter avere la ModelObjectNameEqualityComparer

        #endregion 

        #region Public Constructors

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

        public ModelObject(string name)
        {
            _name = name;
            _guid = Guid.NewGuid();
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

        protected ModelObject(SerializationInfo info, StreamingContext context)
        {
            _guid = (Guid)info.GetValue("Guid", typeof(Guid));
            _name = info.GetString("Name");
        }

        #endregion 

        /// <returns> <see langword="true"/> if <paramref name="guid"/> match the object <see cref="Guid"/> </returns>
        public bool CompareGuid(Guid guid)
        {
            return _guid.Equals(guid);
        }

        #region Equals - HashCode - Operators

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Guid", _guid);
            info.AddValue("Name", _name);
        }

        /// <returns><see langword="True"/> if <paramref name="obj"/> have the same <see cref="Name"/> of this object </returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            return (obj is ModelObject objCasted) && objCasted._name == _name;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return -391 * EqualityComparer<string>.Default.GetHashCode(_name);
            }
        }

        public static bool operator ==(ModelObject obj1, ModelObject obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

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
            /// <returns> <inheritdoc/>
            /// <para> true if both <paramref name="x"/> and <paramref name="y"/> are null </para>
            /// </returns>
            /// <remarks> Only <see cref="ModelObject.Name"/> is used as equality parameter </remarks>
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


            /// <remarks> Only <see cref="ModelObject.Name"/> is used as equality parameter </remarks>
            int IEqualityComparer<ModelObject>.GetHashCode(ModelObject obj)
            {
                return -17 * obj.Name.GetHashCode();
            }
        }

        #endregion CUSTOM EQUALITY COMPARER
    }
}
