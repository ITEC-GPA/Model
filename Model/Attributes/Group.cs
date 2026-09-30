using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Attributes
{
    /// <summary>
    /// A named group of objects of the model, in a tree of groups (parent and children)
    /// </summary>
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    [Serializable]
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
        /// Reads identity and hierarchy; missing legacy hierarchy is initialized empty.
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private Group(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _childs = SerializationFields.Read(info, "Children", new List<Group>());
            _parent = SerializationFields.Read<Group>(info, "Parent");
        }

        #endregion

        #region Public Methods Specific

        /// <summary>
        /// Adds a child group and sets its parent
        /// </summary>
        /// <param name="child">The child group</param>
        public void AddChild(Group child)
        {
            ValidateChild(child);
            if (!Childs.Any(c => ReferenceEquals(c, child))) Childs.Add(child);
            child.Parent = this;
        }

        internal void ValidateChild(Group child, bool allowReparent = false)
        {
            if (child is null) throw new ArgumentNullException(nameof(child));
            var visited = new HashSet<Group>(ReferenceComparer<Group>.Instance);
            for (var current = this; current != null; current = current.Parent)
                if (!visited.Add(current) || ReferenceEquals(current, child)) throw new InvalidOperationException("GroupCycle");
            if (!allowReparent && child.Parent != null && !ReferenceEquals(child.Parent, this))
                throw new InvalidOperationException("GroupAlreadyParented: use Model.ReparentGroup.");
            if (Childs.Any(c => !ReferenceEquals(c, child) && c?.Name == child.Name))
                throw new InvalidOperationException("DuplicateChildName");
        }

        public bool RemoveChild(Group child)
        {
            int index = Childs.FindIndex(c => ReferenceEquals(c, child));
            if (index < 0) return false;
            Childs.RemoveAt(index);
            if (ReferenceEquals(child.Parent, this)) child.Parent = null;
            return true;
        }

        /// <summary>
        /// Validates all children before attaching them and updating their parent.
        /// </summary>
        /// <param name="childs">The child groups</param>
        public void AddChilds(IEnumerable<Group> childs)
        {
            if (childs == null) throw new ArgumentNullException(nameof(childs));
            var values = childs.ToArray();
            foreach (var child in values) ValidateChild(child);
            if (values.GroupBy(c => c.Name).Any(g => g.Distinct(ReferenceComparer<Group>.Instance).Count() > 1))
                throw new InvalidOperationException("DuplicateChildName");
            foreach (var child in values) AddChild(child);
        }

        #endregion

        #region Equals, HasCode and operators

        /// <summary>
        /// Serializes identity and hierarchy with shared references.
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Children", _childs);
            info.AddValue("Parent", _parent);
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
