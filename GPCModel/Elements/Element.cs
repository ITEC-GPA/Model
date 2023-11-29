using GPC.Geometry;
using GPC.Model.Attributes;
using GPC.Model.Collections;
using GPC.Model.Loads;
using GPC.Model.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Elements
{
    /// <summary>
    /// This class rapresnet a base class for all the phisical element inside the gpc model library
    /// </summary>

    [Serializable]
    public abstract class Element : ModelObjectId, ISerializable, IEquatable<Element>
    {
        protected CoordinateSystem _coordinateSystem;
        protected UniqueNameCollection<Group> _groups;
        protected UniqueIdCollection<Attributes.Attribute> _attributes;
        protected UniqueIdCollection<Load> _loads;
        protected List<ElementResult> _results;

        public CoordinateSystem CoordinateSystem { get => _coordinateSystem; set => _coordinateSystem = value; }
        public UniqueNameCollection<Group> Groups { get => _groups; set => _groups = value; }
        public UniqueIdCollection<Attributes.Attribute> Attributes { get => _attributes; set => _attributes = value; }
        public UniqueIdCollection<Load> Loads { get => _loads; set => _loads = value; }
        public List<ElementResult> Results => _results;

        protected Element(string name = "")
            : this(IDUNASSIGNED, name)
        {

        }

        protected Element(int id = IDUNASSIGNED, string name = "", CoordinateSystem coordinateSystem = null)
            : base(id, name)
        {
            if (coordinateSystem == null)
                _coordinateSystem = CoordinateSystem.Global;
            else
                _coordinateSystem = coordinateSystem;
            _groups = new UniqueNameCollection<Group>();
            _attributes = new UniqueIdCollection<Attributes.Attribute>();
            _loads = new UniqueIdCollection<Load>();
            _results = new List<ElementResult>();
        }

        protected Element(int id)
            : this(id, "", null)
        {
        }

        protected Element(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _coordinateSystem = (CoordinateSystem)info.GetValue("CoordinateSystem", typeof(CoordinateSystem));
            _groups = (UniqueNameCollection<Group>)info.GetValue("Groups", typeof(UniqueNameCollection<Group>));
        }

        #region Methods

        public bool ContainsGroup(string groupName)
        {
            return _groups.ContainsKey(groupName);
        }

        public bool ContainsGroup(Group group)
        {
            return _groups.ContainsValue(group);
        }

        public bool AddGroup(Group group)
        {
            if (group is null)
                return false;

            _groups.Add(group.Name, group); // torniamo vero anche se add torna falso, cioè alcuni elementi non aggiunti in quanto già presenti
            return true;
        }

        public bool AddGroupRange(IEnumerable<Group> groups)
        {
            if (groups is null)
                return false;

            _groups.AddRange(groups);
            return true;
        }

        public Group[] GetGroups()
        {
            return _groups.Values.ToArray();
        }

        public virtual void AddResult(FiniteElementResult result)
        {
            if (result != null)
                _results.Add(result);
        }

        #endregion

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("CoordinateSystem", _coordinateSystem);
            info.AddValue("Groups", _groups);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Element);
        }

        bool IEquatable<Element>.Equals(Element other)
        {
            return !(other is null) &&
                _coordinateSystem == other.CoordinateSystem;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _coordinateSystem.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Element obj1, Element obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(Element obj1, Element obj2)
        {
            return !(obj1 == obj2);
        }
    }
}