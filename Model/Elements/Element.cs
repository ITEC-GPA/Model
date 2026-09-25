using GPC.Geometry;
using GPC.Model.Attributes;
using GPC.Model.Collections;
using GPC.Model.Loads;
using GPC.Model.Results.ElementResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Elements
{
    /// <summary>
    /// Base of the physical elements of the model (nodes, beams, areas, volumes): coordinate system, groups, attributes, loads and results
    /// </summary>
    [Serializable]
    public abstract class Element : ModelObjectId, ISerializable, IEquatable<Element>
    {
        /// <summary>
        /// The local coordinate system
        /// </summary>
        protected CoordinateSystem _coordinateSystem;
        /// <summary>
        /// The groups of the element, by name
        /// </summary>
        protected UniqueNameCollection<Group> _groups;
        /// <summary>
        /// The attributes of the element, by id
        /// </summary>
        protected UniqueIdCollection<Attributes.Attribute> _attributes;
        /// <summary>
        /// The loads applied to the element, by id
        /// </summary>
        protected UniqueIdCollection<Load> _loads;
        /// <summary>
        /// The results of the analyses
        /// </summary>
        protected List<ElementResult> _results;

        /// <summary>
        /// The local coordinate system (<see cref="CoordinateSystem.Global"/> by default)
        /// </summary>
        public CoordinateSystem CoordinateSystem { get => _coordinateSystem; set => _coordinateSystem = value; }
        /// <summary>
        /// The groups of the element, by name
        /// </summary>
        public UniqueNameCollection<Group> Groups { get => _groups; set => _groups = value; }
        /// <summary>
        /// The attributes of the element, by id
        /// </summary>
        public UniqueIdCollection<Attributes.Attribute> Attributes { get => _attributes; set => _attributes = value; }
        /// <summary>
        /// The loads applied to the element, by id
        /// </summary>
        public UniqueIdCollection<Load> Loads { get => _loads; set => _loads = value; }
        /// <summary>
        /// The results of the analyses (the list of the element)
        /// </summary>
        public List<ElementResult> Results => _results;

        /// <summary>
        /// Creates an element without id, in the global coordinate system
        /// </summary>
        /// <param name="name">The name</param>
        protected Element(string name = "")
            : this(IDUNASSIGNED, name)
        {

        }

        /// <summary>
        /// Creates an element with empty groups, attributes, loads and results
        /// </summary>
        /// <param name="id">The id</param>
        /// <param name="name">The name</param>
        /// <param name="coordinateSystem">The local coordinate system; null: the global one</param>
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

        /// <summary>
        /// Creates an element with an id, without name, in the global coordinate system
        /// </summary>
        /// <param name="id">The id</param>
        protected Element(int id)
            : this(id, "", null)
        {
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="ModelObjectId"/>, the coordinate system and the groups (attributes, loads and
        /// results are not serialized: they are null)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected Element(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _coordinateSystem = (CoordinateSystem)info.GetValue("CoordinateSystem", typeof(CoordinateSystem));
            _groups = (UniqueNameCollection<Group>)info.GetValue("Groups", typeof(UniqueNameCollection<Group>));
        }

        #region Methods

        /// <summary>
        /// Tell if the element belongs to a group
        /// </summary>
        /// <param name="groupName">The name of the group</param>
        /// <returns>True if the element has a group with the name</returns>
        public bool ContainsGroup(string groupName)
        {
            return _groups.ContainsKey(groupName);
        }

        /// <summary>
        /// Tell if the element belongs to a group
        /// </summary>
        /// <param name="group">The group</param>
        /// <returns>True if the element has the group (compared by name)</returns>
        public bool ContainsGroup(Group group)
        {
            return _groups.ContainsValue(group);
        }

        /// <summary>
        /// Adds the element to a group
        /// </summary>
        /// <param name="group">The group</param>
        /// <returns>False if <paramref name="group"/> is null</returns>
        /// <exception cref="ArgumentException">If the element already has a group with the same name</exception>
        public bool AddGroup(Group group)
        {
            if (group is null)
                return false;

            _groups.Add(group.Name, group); // torniamo vero anche se add torna falso, cioè alcuni elementi non aggiunti in quanto già presenti
            return true;
        }

        /// <summary>
        /// Adds the element to groups
        /// </summary>
        /// <param name="groups">The groups</param>
        /// <returns>False if <paramref name="groups"/> is null</returns>
        /// <exception cref="ArgumentException">If the element already has a group with the same name</exception>
        public bool AddGroupRange(IEnumerable<Group> groups)
        {
            if (groups is null)
                return false;

            _groups.AddRange(groups);
            return true;
        }

        /// <summary>
        /// The groups of the element
        /// </summary>
        /// <returns>A new array with the groups</returns>
        public Group[] GetGroups()
        {
            return _groups.Values.ToArray();
        }

        /// <summary>
        /// Adds a result (a null result is ignored)
        /// </summary>
        /// <param name="result">The result</param>
        public virtual void AddResult(ElementResult result)
        {
            if (result != null)
                _results.Add(result);
        }

        /// <summary>
        /// Adds results (null: nothing is added)
        /// </summary>
        /// <param name="results">The results</param>
        public virtual void AddResults(IEnumerable<ElementResult> results)
        {
            if (results != null)
                _results.AddRange(results);
        }

        #endregion

        /// <summary>
        /// Serializes the data of <see cref="ModelObjectId"/>, the coordinate system and the groups
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("CoordinateSystem", _coordinateSystem);
            info.AddValue("Groups", _groups);
        }

        /// <summary>
        /// Equality with another element (see the implementation of <see cref="IEquatable{Element}"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal element</returns>
        /// <remarks>It calls <c>Equals(obj as Element)</c>, that is this same method (the implementation of the interface is explicit): infinite recursion
        /// (see the list of the defects found)</remarks>
        public override bool Equals(object obj)
        {
            return Equals(obj as Element);
        }

        /// <summary>
        /// Equality of the coordinate systems
        /// </summary>
        /// <param name="other">The element to compare</param>
        /// <returns>True if the elements have the same coordinate system</returns>
        bool IEquatable<Element>.Equals(Element other)
        {
            return !(other is null) &&
                _coordinateSystem == other.CoordinateSystem;
        }

        /// <summary>
        /// The hash code of the name and of the coordinate system
        /// </summary>
        /// <returns>The hash code</returns>
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

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first element (not null)</param>
        /// <param name="obj2">The second element</param>
        /// <returns>True if the elements are equal</returns>
        public static bool operator ==(Element obj1, Element obj2)
        {
            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first element (not null)</param>
        /// <param name="obj2">The second element</param>
        /// <returns>True if the elements are different</returns>
        public static bool operator !=(Element obj1, Element obj2)
        {
            return !(obj1 == obj2);
        }
    }
}