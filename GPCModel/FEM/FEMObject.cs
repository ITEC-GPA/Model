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
        public FEMObject(int id) 
            : this(id, string.Empty)
        {

        }

        public FEMObject(int id, string name) 
            : base(Guid.NewGuid(), name)
        {
            base.SetId(id);
        }

        public FEMObject(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }


        #region Equals, hascode, operators, 

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            FEMObject objCasted = obj as FEMObject;

            return !(objCasted is null) && _activeStages.ScrambledEquals(objCasted._activeStages)
                                        && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();

            if (_activeStages.Count > 0)
            {
                foreach (var kvp in _activeStages)
                {
                    hashCode = hashCode + EqualityComparer<Stage>.Default.GetHashCode(kvp.Key);
                    hashCode = hashCode + EqualityComparer<bool>.Default.GetHashCode(kvp.Value);
                }
            }

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
        /// Custom Equality comparer that compare two <see cref="FEMObject"/> adding also the <see cref="Element.Id"/> as an equality parameter
        /// </summary>
        public class FemObjectIdComparer : IEqualityComparer<FEMObject>
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
                int hashCode = -23 * -17 + base.GetHashCode();

                hashCode = hashCode + obj.GetHashCode();

                hashCode = hashCode + obj.Id.GetHashCode();

                return hashCode;
            }
        }
    }
}
