using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GPC.Model.Attributes
{
    /// <summary>
    /// This class to be used to group some <see cref="FemObject"/> togethers.
    /// </summary>
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public sealed class Group : ModelObjectId, IEquatable<Group>
    {
        #region Variables

        private List<Group> _childs;
        private Group _parent;

        #endregion

        #region Properties

        public List<Group> Childs { get => _childs; set => _childs = value; }

        public Group Parent { get => _parent; private set => _parent = value; }

        #endregion

        #region Public Constructors

        internal Group(string name, Group parent = null)
            : base(name)
        {
            _childs = new List<Group>();
            _parent = parent;
        }

        private Group(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #endregion

        #region Public Methods Specific

        public void AddChild(Group child)
        {
            Childs.Add(child);
            child.Parent = this;
        }

        public void AddChilds(IEnumerable<Group> childs)
        {
            foreach (Group child in childs)
                Childs.Add(child);
        }

        #endregion

        #region Equals, HasCode and operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public bool Equals(Group other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return base.Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Group group && Equals(group);
        }

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

        public static bool operator !=(Group obj1, Group obj2)
        {
            return !(obj1 == obj2);
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }

        public override string ToString()
        {
            return $"{Id}-{Name}";
        }

        #endregion
    }
}