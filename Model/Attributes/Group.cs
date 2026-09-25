using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GPC.Model.Attributes
{
    /// <summary>
    /// A named group of objects of the model, in a tree of groups (parent and children)
    /// </summary>
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public sealed class Group : ModelObjectId, IEquatable<Group>
    {
        #region Variables

        /// <summary>
        /// The child groups
        /// </summary>
        private List<Group> _childs;
        /// <summary>
        /// The parent group; null for a root group
        /// </summary>
        private Group _parent;

        #endregion

        #region Properties

        /// <summary>
        /// The child groups (the list of the group)
        /// </summary>
        public List<Group> Childs { get => _childs; set => _childs = value; }

        /// <summary>
        /// The parent group; null for a root group
        /// </summary>
        public Group Parent { get => _parent; private set => _parent = value; }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates a group without children
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="parent">The parent group (it is not updated: use <see cref="AddChild"/> on it)</param>
        internal Group(string name, Group parent = null)
            : base(name)
        {
            _childs = new List<Group>();
            _parent = parent;
        }

        /// <summary>
        /// Deserialization constructor: reads only the data of <see cref="ModelObjectId"/> (children and parent are not serialized)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private Group(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #endregion

        #region Public Methods Specific

        /// <summary>
        /// Adds a child group and sets its parent
        /// </summary>
        /// <param name="child">The child group</param>
        public void AddChild(Group child)
        {
            Childs.Add(child);
            child.Parent = this;
        }

        /// <summary>
        /// Adds child groups (their parent is not set, unlike <see cref="AddChild"/>)
        /// </summary>
        /// <param name="childs">The child groups</param>
        public void AddChilds(IEnumerable<Group> childs)
        {
            foreach (Group child in childs)
                Childs.Add(child);
        }

        #endregion

        #region Equals, HasCode and operators

        /// <summary>
        /// Serializes the data of <see cref="ModelObjectId"/> (children and parent are not serialized)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Equality of the names (see <see cref="ModelObjectId.Equals(object)"/>)
        /// </summary>
        /// <param name="other">The group to compare</param>
        /// <returns>True if the groups have the same name</returns>
        public bool Equals(Group other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return base.Equals(other);
        }

        /// <summary>
        /// The hash code of the name
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Equality with another object (see <see cref="Equals(Group)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal group</returns>
        public override bool Equals(object obj)
        {
            return obj is Group group && Equals(group);
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(Group)"/>); two null groups are equal
        /// </summary>
        /// <param name="obj1">The first group</param>
        /// <param name="obj2">The second group</param>
        /// <returns>True if the groups are equal</returns>
        public static bool operator ==(Group obj1, Group obj2)
        {
            if (obj1 is null)
            {
                if (obj2 is null)
                    return true;
                else
                    return false;
            }

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(Group)"/>)
        /// </summary>
        /// <param name="obj1">The first group</param>
        /// <param name="obj2">The second group</param>
        /// <returns>True if the groups are different</returns>
        public static bool operator !=(Group obj1, Group obj2)
        {
            return !(obj1 == obj2);
        }

        /// <summary>
        /// The text shown by the debugger
        /// </summary>
        /// <returns>See <see cref="ToString"/></returns>
        private string GetDebuggerDisplay()
        {
            return ToString();
        }

        /// <summary>
        /// The id and the name
        /// </summary>
        /// <returns>"Id-Name"</returns>
        public override string ToString()
        {
            return $"{Id}-{Name}";
        }

        #endregion
    }
}