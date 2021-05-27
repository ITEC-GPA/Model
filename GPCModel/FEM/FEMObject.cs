using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GPC.Model.Elements;
using GPC.Model.FEM.Collections;
using GPC.Utilities.Extensions;

namespace GPC.Model.FEM
{
    [Serializable]
    public abstract class FEMObject : ModelObjectId, ISerializable
    {

        protected readonly UniqueNameCollection<Group> _groups; // non usiamo groupCollection in quanto l'id è già stato assegnato dal femModel.
                                                                // Usiamo questa collection per avere contains con nome e perchè è thread-safe


        public FEMObject()
        {
            _groups = new UniqueNameCollection<Group>();
        }

        public FEMObject(string name) 
            : base(name)
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


        public bool ContainsGroup(string groupName)
        {
            return _groups.Contains(groupName);
        }
        
        public bool ContainsGroup(Group group)
        {
            return _groups.Contains(group);
        }

        internal bool AddGroup(Group group)
        {
            if (group is null)
                return false;

            _groups.Add(group); // torniamo vero anche se add torna falso, cioè alcuni elementi non aggiunti in quanto già presenti
            return true;
        }

        internal bool AddGroupRange(IEnumerable<Group> groups)
        {
            if (groups is null)
                return false;

            _groups.AddRange(groups);
            return true;
        }


        public Group[] GetGroups()
        {
            return _groups.ToArray();
        }


        internal void SetId(int id)
        {
            // teoricamente questo metodo non serve più. Al momento esiste solo per retrocompatibilità
            base.Id = id;
        }

        #region Equals, hascode, operators, 

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return (obj is FEMObject objCasted) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            return -17 * base.GetHashCode();
        }


        public static bool operator ==(FEMObject obj1, FEMObject obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

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
                    return ((-391 + obj.Id.GetHashCode())* -17 + obj.GetHashCode()) * -17 + base.GetHashCode();
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
                    return (-391 + obj.Id.GetHashCode()) * -17 + base.GetHashCode();
                }
            }
        }
    }
}
