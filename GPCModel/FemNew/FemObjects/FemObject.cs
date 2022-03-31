using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;


namespace GPC.Model.Fem.FemObjects
{

    /// <summary>
    /// This is the base class for all the Fem objects
    /// </summary>
    [Serializable]
    public abstract class FemObject : ModelObjectId, ISerializable
    {

        protected readonly UniqueNameCollection<Group> _groups; // non usiamo groupCollection in quanto l'id è già stato assegnato dal femModel.
                                                                // Usiamo questa collection per avere contains con nome e perchè è thread-safe


        // id non deve essere settabile esternamente
        protected internal FemObject(int id, string name, Guid guid) 
            : base(id, name, guid)
        {
            _groups = new UniqueNameCollection<Group>();
        }

        public FemObject(string name = "") 
            : this (ModelObjectId.IDUNASSIGNED, name, new Guid())
            
        {

        }

        // id non deve essere settabile esternamente
        protected internal FemObject(int id, string name)
            : this(id, name, new Guid())

        {

        }

        protected FemObject(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #region Groups methods

        public bool ContainsGroup(string groupName)
        {
            return _groups.ContainsName(groupName);
        }

        public bool ContainsGroup(Group group)
        {
            return _groups.Contains(group);
        }

        /// <summary>
        /// This is an internal method, since only the femModel class can add a group to the femObject
        /// </summary>
        internal bool AddGroup(Group group)
        {
            if (group is null)
                return false;

            _groups.Add(group); // torniamo vero anche se add torna falso, cioè alcuni elementi non aggiunti in quanto già presenti
            return true;
        }

        /// <summary>
        /// This is an internal method, since only the femModel class can add a group to the femObject
        /// </summary>
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

        #endregion


        #region Equals, hascode, operators, 

        public override bool Equals(object obj)
        {
            return (obj is FemObject objCasted) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return -17 * base.GetHashCode();
            }
        }


        public static bool operator ==(FemObject obj1, FemObject obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            return obj1.Equals(obj2);
        }

        public static bool operator !=(FemObject obj1, FemObject obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion

        /// <summary>
        /// Custom Equality comparer that compare two <see cref="FemObject"/> adding also the <see cref="ModelObjectId.Id"/> as an equality parameter
        /// </summary>
        public class FemObjectWithIdComparer : IEqualityComparer<FemObject>
        {
            public bool Equals(FemObject x, FemObject y)
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


            public int GetHashCode(FemObject obj)
            {
                unchecked
                {
                    return ((-391 + obj.Id.GetHashCode()) * -17 + obj.GetHashCode()) * -17 + base.GetHashCode();
                }
            }
        }


        /// <summary>
        /// Custom equality comparer that compare two <see cref="FemObject"/> using only the <see cref="ModelObjectId.Id"/> as an equality parameter
        /// </summary>
        public class FemObjectOnlyIdComparer : IEqualityComparer<FemObject>
        {
            public bool Equals(FemObject x, FemObject y)
            {
                if (x == null && y == null)
                    return true;

                if (x == null || y == null)
                    return false;

                if (x.Id == y.Id)
                    return true;

                return false;
            }

            public int GetHashCode(FemObject obj)
            {
                unchecked
                {
                    return (-391 + obj.Id.GetHashCode()) * -17 + base.GetHashCode();
                }
            }
        }
    }
}
