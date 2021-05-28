using GPC.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model
{
    [Serializable]
    public class ModelObjectIdSet<T> : ModelObjectSet<T> where T : ModelObjectId, ISerializable
    {

        public ModelObjectIdSet()
        {

        }

        public ModelObjectIdSet(IEqualityComparer<T> comparer) : base(comparer)
        {

        }


        public ModelObjectIdSet(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {

        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #region Equals - hashcode - Operators

        public override bool Equals(object obj)
        {
            lock (_locker)
            {
                return obj is ModelObjectIdSet<T> collection && _collection.ScrambledEquals(collection._collection)
                                                             && base.Equals(collection);
            }
        }

        public override int GetHashCode()
        {
            lock (_locker)
            {
                unchecked
                {
                    int hashCode = -391 + base.GetHashCode();

                    foreach (var element in _collection)
                    {
                        hashCode += element.GetHashCode();
                    }

                    return hashCode;
                }
            }
        }


        public static bool operator ==(ModelObjectIdSet<T> obj1, ModelObjectIdSet<T> obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ModelObjectIdSet<T> obj1, ModelObjectIdSet<T> obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion 
    }
}
