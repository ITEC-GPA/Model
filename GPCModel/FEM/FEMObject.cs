using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GPC.Model.Elements;
using GPC.Utilities.Extensions;

namespace GPC.Model.FEM
{
    [Serializable]
    public abstract class FEMObject : Element, ISerializable
    {
        /// <remarks>
        /// Public setter not available, in the same assembly you can use <see cref="SetId(int)"/> otherwise you can not set the id of a <see cref="FEMObject"/>
        /// </remarks>
        /// <exception cref="NotSupportedException"></exception>
        public override int Id { get => base.Id; set => throw new NotSupportedException($"Public setter not available, use method {nameof(SetId)}"); }

        public FEMObject() 
            : this(string.Empty)
        {

        }

        public FEMObject(string name) 
            : base(Guid.NewGuid(), name)
        {

        }

        public FEMObject(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }


        internal void SetId(int id)
        {
            base.Id = id;
        }

        #region Equals, hascode, operators, 

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            FEMObject objCasted = obj as FEMObject;

            return !(objCasted is null) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();

            return hashCode;
        }


        public static bool operator ==(FEMObject obj1, FEMObject obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(FEMObject obj1, FEMObject obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion

        /// <summary>
        /// Custom Equality comparer that compare two <see cref="FEMObject"/> adding also the <see cref="ModelObjectId.Id"/> as an equality parameter
        /// </summary>
        public class FemObjectWithIdComparer : IEqualityComparer<FEMObject>
        {
            public bool Equals(FEMObject x, FEMObject y)
            {
                if (ReferenceEquals(x, y))
                    return true;

                if (x == null && y == null)
                    return true;

                if (x == null || y == null)
                    return false;

                if (x.Equals(y) && x.Id == y.Id)
                    return true;

                return false;
            }


            public int GetHashCode(FEMObject obj)
            {
                unchecked
                {
                    int hashCode = -391 + base.GetHashCode();

                    hashCode += obj.GetHashCode();

                    hashCode += obj.Id.GetHashCode();

                    return hashCode; 
                }
            }
        }


        /// <summary>
        /// Custom equality comparer that compare two <see cref="FEMObject"/> using only the <see cref="ModelObjectId.Id"/> as an equality parameter
        /// </summary>
        public class FemObjectOnlyIdComparer : IEqualityComparer<FEMObject>
        {
            public bool Equals(FEMObject x, FEMObject y)
            {
                if (x == null && y == null)
                    return true;

                if (x == null || y == null)
                    return false;

                if (x.Id == y.Id)
                    return true;

                return false;
            }

            public int GetHashCode(FEMObject obj)
            {
                unchecked
                {
                    int hashCode = -23 * -17 + base.GetHashCode();

                    hashCode = hashCode + obj.Id.GetHashCode();

                    return hashCode; 
                }
            }
        }
    }
}
