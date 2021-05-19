using System;
using System.Runtime.Serialization;


namespace GPC.Model
{
    /// <summary>
    /// This class add the attribute Id to the <see cref="ModelObject"/> base class
    /// </summary>

    [Serializable]
    public abstract class ModelObjectId : ModelObject, ISerializable
    {
        protected int _id;

        public virtual int Id { get => _id; set { _id = value; } }

        protected ModelObjectId()
        {

        }

        protected ModelObjectId(int id)
            : base(Guid.NewGuid())
        {
            _id = id;
        }

        protected ModelObjectId(Guid guid)
            : base(guid)
        {

        }

        protected ModelObjectId(string name)
            : base(name)
        {

        }

        protected ModelObjectId(Guid guid, string name)
            : base(guid, name)
        {

        }

        protected ModelObjectId(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _id = info.GetInt32("Id");
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Id", _id);
        }


        /// <remarks>Equality is not checked against <see cref="ModelObjectId.Id"/> </remarks>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            // ID non viene messo in equals in quanto non tutte le derivate devono ritornare true se gli id sono uguali. 
            // Se ne deve occupare la derivata

            return (obj is ModelObjectId modelId) && base.Equals(modelId);
        }

        public override int GetHashCode()
        {
            return 17 * base.GetHashCode();
        }

        public static bool operator ==(ModelObjectId obj1, ModelObjectId obj2)
        {
            if (obj1 is null || obj2 is null)
                return false;

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ModelObjectId obj1, ModelObjectId obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
