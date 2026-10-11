using GPC.Geometry;
using GPC.Model.Attributes;
using GPC.Model.Collections;
using GPC.Model.Loads;
using GPC.Model.Results.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using GPC.Model.Core.Identity;

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
        public global::GPC.Model.Core.Identity.SourceIdentity Source { get; set; }
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
        /// The results of the analyses (null while they are still in their compact form: read them through <see cref="Results"/>)
        /// </summary>
        protected List<ElementResult> _results;
        // Compact results not read yet, decoded on the first access to Results with the cases of _resultModel; results added in the
        // meantime wait in _addedResults. After the decoding, _resultOrigin and _resultOriginItems remember where the list came from,
        // so that an unchanged list encodes again to the same segments.
        [NonSerialized] private ResultStorage _storedResults;
        [NonSerialized] private List<ElementResult> _addedResults;
        [NonSerialized] private Models.Model _resultModel;
        [NonSerialized] private ResultStorage _resultOrigin;
        [NonSerialized] private ElementResult[] _resultOriginItems;
        [NonSerialized] private object _resultLock;

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
        /// The results of the analyses (the list of the element). Results kept in compact form (copies, archives) are decoded on the
        /// first access, in new objects bound to the cases of the model
        /// </summary>
        public List<ElementResult> Results => Volatile.Read(ref _results) ?? LoadResults();

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
        /// Reads identity, coordinate system, groups, assignments and results. Missing legacy collections are initialized empty.
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected Element(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            SerializationVersions.Read(info, "ElementVersion", 1, 1);
            Source = SerializationFields.Read<global::GPC.Model.Core.Identity.SourceIdentity>(info, "Source");
            _coordinateSystem = (CoordinateSystem)info.GetValue("CoordinateSystem", typeof(CoordinateSystem));
            _groups = (UniqueNameCollection<Group>)info.GetValue("Groups", typeof(UniqueNameCollection<Group>));
            _attributes = SerializationFields.Read(info, "Attributes", new UniqueIdCollection<Attributes.Attribute>());
            _attributes.ReadCounter(info, "AttributesMaxId");
            _loads = SerializationFields.Read(info, "Loads", new UniqueIdCollection<Load>());
            _loads.ReadCounter(info, "LoadsMaxId");
            _results = SerializationFields.Read(info, "Results", new List<ElementResult>());
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
                AppendResults(new[] { result });
        }

        /// <summary>
        /// Adds results (null: nothing is added)
        /// </summary>
        /// <param name="results">The results</param>
        public virtual void AddResults(IEnumerable<ElementResult> results)
        {
            if (results != null)
                AppendResults(results);
        }

        #endregion

        #region Compact results

        private object ResultLock
        {
            get
            {
                if (_resultLock == null) Interlocked.CompareExchange(ref _resultLock, new object(), null);
                return _resultLock;
            }
        }

        private List<ElementResult> LoadResults()
        {
            lock (ResultLock)
            {
                if (_results != null) return _results;
                var list = ResultCodec.Decode(_storedResults, _resultModel);
                _resultOrigin = _storedResults; _resultOriginItems = list.ToArray();
                if (_addedResults != null) list.AddRange(_addedResults);
                _storedResults = null; _addedResults = null; _resultModel = null;
                Volatile.Write(ref _results, list);
                return list;
            }
        }

        /// <summary>Adds to the list, or after the compact results without decoding them.</summary>
        internal void AppendResults(IEnumerable<ElementResult> results)
        {
            lock (ResultLock)
            {
                if (_results != null) _results.AddRange(results);
                else (_addedResults ?? (_addedResults = new List<ElementResult>())).AddRange(results);
            }
        }

        /// <summary>False while the results are still in compact form.</summary>
        internal bool ResultsLoaded => Volatile.Read(ref _results) != null;

        /// <summary>True if the element has results, without decoding them.</summary>
        internal bool HasResults
        {
            get { lock (ResultLock) return _results != null ? _results.Count != 0 : _storedResults.Count != 0 || _addedResults?.Count > 0; }
        }

        /// <summary>True if a result belongs to the dataset, without decoding the compact results.</summary>
        internal bool ContainsResultsOf(string dataset)
        {
            lock (ResultLock)
            {
                IEnumerable<ElementResult> scan = _results;
                if (scan == null) { if (_storedResults.ContainsDataset(dataset)) return true; scan = _addedResults ?? Enumerable.Empty<ElementResult>(); }
                return scan.Where(r => r?.Results != null).SelectMany(r => r.Results).Any(s => s?.State?.DatasetId == dataset);
            }
        }

        /// <summary>The compact form of the current results, without changing the element. An unchanged decoded list gives back the
        /// segments it was read from, so that the digest of the results does not depend on whether they were read.</summary>
        /// <exception cref="NotSupportedException">If a result references a sample of another element or a value outside the
        /// vocabulary of the model</exception>
        internal ResultStorage CaptureResults(ResultCaseTable cases)
        {
            lock (ResultLock)
            {
                if (_results == null)
                    return _addedResults == null || _addedResults.Count == 0 ? _storedResults
                        : _storedResults.Append(ResultCodec.Encode(_addedResults, 0, _addedResults.Count, cases));
                var list = _results;
                if (list.Count == 0) return ResultStorage.Empty;
                if (_resultOrigin != null && list.Count >= _resultOriginItems.Length && StartsWithOrigin(list))
                {
                    try
                    {
                        var segments = new List<ResultSegment>(); int at = 0;
                        foreach (var original in _resultOrigin.Segments)
                        {
                            var segment = ResultCodec.Encode(list, at, original.Count, cases);
                            segments.Add(segment.SameContent(original) ? original : segment); at += original.Count;
                        }
                        if (at < list.Count) segments.Add(ResultCodec.Encode(list, at, list.Count - at, cases));
                        return new ResultStorage(segments.ToArray());
                    }
                    // A result added after the reading references earlier samples: the list is encoded as a whole.
                    catch (ResultEncodingException ex) when (ex.Foreign) { }
                }
                return new ResultStorage(new[] { ResultCodec.Encode(list, 0, list.Count, cases) });
            }
        }

        private bool StartsWithOrigin(List<ElementResult> list)
        {
            for (int i = 0; i < _resultOriginItems.Length; i++) if (!ReferenceEquals(list[i], _resultOriginItems[i])) return false;
            return true;
        }

        /// <summary>Replaces the results with compact ones, decoded on the first access with the cases of <paramref name="model"/>.</summary>
        internal void AttachResults(ResultStorage storage, Models.Model model)
        {
            lock (ResultLock)
            {
                _resultOrigin = null; _resultOriginItems = null; _addedResults = null;
                if (storage == null || storage.Count == 0) { _storedResults = null; _resultModel = null; Volatile.Write(ref _results, new List<ElementResult>()); return; }
                _storedResults = storage; _resultModel = model; Volatile.Write(ref _results, null);
            }
        }

        /// <summary>Encodes the results and releases the decoded objects.</summary>
        internal ResultStorage CompactResults(ResultCaseTable cases, Models.Model model)
        {
            lock (ResultLock)
            {
                var storage = CaptureResults(cases);
                AttachResults(storage, model);
                return storage;
            }
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
            info.AddValue("Source", Source);
            info.AddValue("ElementVersion", 1);
            info.AddValue("Attributes", _attributes);
            info.AddValue("Loads", _loads);
            // The id counters only when a removed item had the largest id: the data contracts write the collections as dictionaries
            _attributes.WriteCounter(info, "AttributesMaxId");
            _loads.WriteCounter(info, "LoadsMaxId");
            // Images of the model carry the results apart, in compact form (ModelImage).
            if (!ResultScope.Skips(this)) info.AddValue("Results", Results);
        }

        /// <summary>
        /// Equality with another element (see the implementation of <see cref="IEquatable{Element}"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal element</returns>
        /// <remarks>FEM identity is independent of geometry and registry position.</remarks>
        public override bool Equals(object obj)
        {
            return obj is Element other && GetType() == other.GetType() && Guid == other.Guid;
        }

        /// <summary>
        /// Equality of FEM identity and concrete type.
        /// </summary>
        /// <param name="other">The element to compare</param>
        /// <returns>True if the elements represent the same FEM identity.</returns>
        bool IEquatable<Element>.Equals(Element other)
        {
            return Equals((object)other);
        }

        /// <summary>
        /// The identity hash, unaffected by geometric edits.
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return Guid.GetHashCode();
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
            return ReferenceEquals(obj1, obj2) || (!(obj1 is null) && obj1.Equals(obj2));
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
