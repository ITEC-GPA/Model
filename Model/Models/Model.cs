using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Attributes;
using GPC.Model.Collections;
using GPC.Model.Combinations;
using GPC.Model.Costrains;
using GPC.Model.ElementProperties;
using GPC.Model.Elements;
using GPC.Model.FreedomCases;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Restrains;
using GPC.Model.Results.ElementResults;
using GPC.Model.Results.ResultLocations;
using GPC.Model.Sections;
using GPC.Model.Stages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Models
{
    /// <summary>
    /// A finite element model: nodes, beam, area and volume elements, costrains, element properties, load cases, freedom cases, combinations,
    /// groups and stages
    /// </summary>
    [Serializable]
    public partial class Model : ModelObject, ISerializable
    {
        public Dictionary<string, GPC.Model.PostProcessing.AnalysisDataset> Datasets { get; private set; } = new Dictionary<string, GPC.Model.PostProcessing.AnalysisDataset>();
        public GPC.Model.PostProcessing.AnalysisSource AnalysisSource { get; set; }
        public Dictionary<string, GPC.Model.PostProcessing.PhysicalMemberDefinition> PhysicalMembers { get; private set; } = new Dictionary<string, GPC.Model.PostProcessing.PhysicalMemberDefinition>(StringComparer.Ordinal);
        #region Variables

        /// <summary>
        /// The nodes by id
        /// </summary>
        protected SortedCollection<NodeElement> _nodesElements;
        /// <summary>
        /// The beam elements by id
        /// </summary>
        protected SortedCollection<BeamElement> _beamElements;
        /// <summary>
        /// The area elements by id
        /// </summary>
        protected SortedCollection<AreaElement> _areaElements;
        /// <summary>
        /// The volume elements by id (not created by <see cref="Model(string)"/>: it is null)
        /// </summary>
        protected SortedCollection<VolumeElement> _volumeElements;
        /// <summary>
        /// The costrains by id
        /// </summary>
        protected UniqueIdCollection<Costrain> _costrains;

        /// <summary>
        /// The beam properties by name
        /// </summary>
        protected UniqueNameCollection<BeamProperty> _beamProperties;
        /// <summary>
        /// The plate properties by name
        /// </summary>
        protected UniqueNameCollection<PlateProperty> _areaProperties;
        /// <summary>
        /// The brick properties by name
        /// </summary>
        protected UniqueNameCollection<BrickProperty> _volumeProperties;

        /// <summary>
        /// The load cases by name
        /// </summary>
        protected UniqueNameCollection<LoadCaseBase> _loadCases;
        /// <summary>
        /// The freedom cases by name
        /// </summary>
        protected UniqueNameCollection<FreedomCase> _freedomCases;
        /// <summary>
        /// The combinations by name
        /// </summary>
        protected UniqueNameCollection<Combination> _combinations;
        /// <summary>
        /// The names of the combinations of each stage, by stage id
        /// </summary>
        protected Dictionary<int, HashSet<string>> _stageCombinationsMap;

        /// <summary>
        /// The groups by name
        /// </summary>
        protected UniqueNameCollection<Group> _groups;
        /// <summary>
        /// The stages by id
        /// </summary>
        protected UniqueIdCollection<Stage> _stages;

        #endregion

        #region Properties

        /// <summary>
        /// Collection of <see cref="NodeElement"/>
        /// The elements on this collection does not have duplicate ID and can not be duplicate. (different element with different id)
        /// </summary>
        public SortedCollection<NodeElement> NodesElements => _nodesElements;

        /// <summary>
        /// Collection of <see cref="BeamElement"/>
        /// The elements on this collection does not have duplicate ID and can not be duplicate. (different element with different id)
        /// </summary>
        public SortedCollection<BeamElement> BeamElements => _beamElements;

        /// <summary>
        /// Collection of <see cref="AreaElement"/>
        /// The elements on this collection does not have duplicate ID and can not be duplicate. (different element with different id)
        /// </summary>
        public SortedCollection<AreaElement> AreaElements => _areaElements;

        /// <summary>
        /// Collection of <see cref="VolumeElement"/>
        /// The elements on this collection does not have duplicate ID and can not be duplicate. (different element with different id)
        /// </summary>
        public SortedCollection<VolumeElement> VolumeElements => _volumeElements;

        /// <summary>
        /// Collection of <see cref="Costrain"/>
        /// The elements on this collection does not have duplicate ID and can not be duplicate. (different element with different id)
        /// </summary>
        public UniqueIdCollection<Costrain> Costrains => _costrains;

        /// <summary>
        /// Collection of <see cref="Section"/> with unique name 
        /// </summary>
        public UniqueNameCollection<BeamProperty> BeamProperties => _beamProperties;

        /// <summary>
        /// Collection of <see cref="PlateProperty"/> with unique name 
        /// </summary>
        public UniqueNameCollection<PlateProperty> PlateProperties => _areaProperties;

        /// <summary>
        /// Collection of <see cref="BrickProperty"/> with unique name 
        /// </summary>
        public UniqueNameCollection<BrickProperty> BrickProperties => _volumeProperties;

        /// <summary>
        /// Collection of <see cref="LoadCaseBase"/> with unique name 
        /// </summary>
        public UniqueNameCollection<LoadCaseBase> LoadCases => _loadCases;

        /// <summary>
        /// Collection of <see cref="FreedomCase"/> with unique name 
        /// </summary>
        public UniqueNameCollection<FreedomCase> FreedomCases => _freedomCases;

        /// <summary>
        /// Collection of <see cref="Combination"/> with unique name 
        /// </summary>
        public UniqueNameCollection<Combination> Combinations => _combinations;

        /// <summary>
        /// Map between stageId and stage combinations
        /// </summary>
        public Dictionary<int, HashSet<string>> StageCombinationsMap => _stageCombinationsMap;

        /// <summary>
        /// Collections of <see cref="Group"/>
        /// </summary>
        public UniqueNameCollection<Group> Groups => _groups;

        /// <summary>
        /// Collections of <see cref="Stage"/>
        /// </summary>
        public UniqueIdCollection<Stage> Stages => _stages;

        /// <summary>
        /// Loads acting on the whole model rather than on an element, e.g. <see cref="Loads.ModelGravityLoad"/>
        /// </summary>
        public UniqueIdCollection<Load> ModelLoads { get; private set; } = new UniqueIdCollection<Load>();

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an empty model (the collection of the volume elements is not created)
        /// </summary>
        /// <param name="name">The name of the model</param>
        public Model(string name = "")
            : base(name)
        {
            _beamProperties = new UniqueNameCollection<BeamProperty>();
            _areaProperties = new UniqueNameCollection<PlateProperty>();
            _volumeProperties = new UniqueNameCollection<BrickProperty>();

            _groups = new UniqueNameCollection<Group>();
            _groups.ItemRenamed += OnGroupRenamed;
            _groups.ItemRenaming += OnGroupRenaming;
            _costrains = new UniqueIdCollection<Costrain>();

            _nodesElements = new SortedCollection<NodeElement>();
            _beamElements = new SortedCollection<BeamElement>();
            _areaElements = new SortedCollection<AreaElement>();
            _volumeElements = new SortedCollection<VolumeElement>();

            _stages = new UniqueIdCollection<Stage>(); // solo id come equality comparer

            _loadCases = new UniqueNameCollection<LoadCaseBase>();
            _freedomCases = new UniqueNameCollection<FreedomCase>();
            _combinations = new UniqueNameCollection<Combination>();

            _stageCombinationsMap = new Dictionary<int, HashSet<string>>();
        }

        /// <summary>
        /// Reads versioned model data and optional post-processing records.
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected Model(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int schemaVersion = SerializationFields.Read(info, "ModelSchemaVersion", 0);
            if (schemaVersion < 0 || schemaVersion > 1) throw new SerializationException("Unsupported Model schema version.");
            Datasets = SerializationFields.Read(info, "Datasets", new Dictionary<string, GPC.Model.PostProcessing.AnalysisDataset>());
            AnalysisSource = SerializationFields.Read<GPC.Model.PostProcessing.AnalysisSource>(info, "AnalysisSource");
            CheckReports = SerializationFields.Read(info, "CheckReports", new GPC.Model.PostProcessing.CheckReport[0]).ToList();
            PhysicalMembers = SerializationFields.Read(info, "PhysicalMembers", new GPC.Model.PostProcessing.PhysicalMemberDefinition[0]).ToDictionary(m => m.Id, StringComparer.Ordinal);
            PreservedSourceData = SerializationFields.Read(info, "PreservedSourceData", new GPC.Model.PostProcessing.PreservedAssignment[0]).ToList();
            _nodesElements = (SortedCollection<NodeElement>)info.GetValue("Nodes", typeof(SortedCollection<NodeElement>));
            _beamElements = (SortedCollection<BeamElement>)info.GetValue("Beams", typeof(SortedCollection<BeamElement>));
            _areaElements = (SortedCollection<AreaElement>)info.GetValue("Areas", typeof(SortedCollection<AreaElement>));
            _volumeElements = (SortedCollection<VolumeElement>)info.GetValue("Volumes", typeof(SortedCollection<VolumeElement>)) ?? new SortedCollection<VolumeElement>();
            _costrains = (UniqueIdCollection<Costrain>)info.GetValue("Costrains", typeof(UniqueIdCollection<Costrain>));

            _beamProperties = (UniqueNameCollection<BeamProperty>)info.GetValue("BeamProperties", typeof(UniqueNameCollection<BeamProperty>));
            _areaProperties = (UniqueNameCollection<PlateProperty>)info.GetValue("PlateProperties", typeof(UniqueNameCollection<PlateProperty>));
            _volumeProperties = (UniqueNameCollection<BrickProperty>)info.GetValue("BrickProperties", typeof(UniqueNameCollection<BrickProperty>));

            _loadCases = (UniqueNameCollection<LoadCaseBase>)info.GetValue("LoadCaseBases", typeof(UniqueNameCollection<LoadCaseBase>));
            _freedomCases = (UniqueNameCollection<FreedomCase>)info.GetValue("FreedomCases", typeof(UniqueNameCollection<FreedomCase>));
            _combinations = (UniqueNameCollection<Combination>)info.GetValue("Combinations", typeof(UniqueNameCollection<Combination>));

            _stageCombinationsMap = (Dictionary<int, HashSet<string>>)info.GetValue("StageCombinationsMap", typeof(Dictionary<int, HashSet<string>>));
            _groups = (UniqueNameCollection<Group>)info.GetValue("Groups", typeof(UniqueNameCollection<Group>));
            _groups.ItemRenamed += OnGroupRenamed;
            _groups.ItemRenaming += OnGroupRenaming;
            _stages = (UniqueIdCollection<Stage>)info.GetValue("Stages", typeof(UniqueIdCollection<Stage>));
            ModelLoads = SerializationFields.Read(info, "ModelLoads", new UniqueIdCollection<Load>());
        }

        #endregion

        #region Methods

        #region Add Get Attributes

        #region Element Properties

        /// <summary>
        /// Adds a beam, plate or brick property
        /// </summary>
        /// <param name="elementProperty">The property</param>
        /// <returns><see langword="true"/> if the property has been added.
        /// <para><see langword="false"/> if the property is null, of another type or a property of the same type with the same name is already present</para>
        /// </returns>
        public virtual bool AddProperty(ElementProperty elementProperty)
        {
            if (elementProperty is null)
                return false;

            if (elementProperty is BeamProperty beamProperty)
            {
                if (_beamProperties.ContainsKey(beamProperty.Name))
                    return false;

                _beamProperties.Add(beamProperty.Name, beamProperty);
                return true;
            }

            if (elementProperty is PlateProperty plateProperty)
            {
                if (_areaProperties.ContainsKey(elementProperty.Name))
                    return false;

                _areaProperties.Add(elementProperty.Name, plateProperty);
                return true;
            }
            else if (elementProperty is BrickProperty brickProperty)
            {
                if (_volumeProperties.ContainsKey(elementProperty.Name))
                    return false;

                _volumeProperties.Add(brickProperty.Name, brickProperty);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// The plate property with a name
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns>The property; null if no plate property has the name</returns>
        public virtual PlateProperty GetPlateProperty(string name)
        {
            return _areaProperties.GetElementByName(name);
        }

        /// <summary>
        /// The beam property with a name
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns>The property; null if no beam property has the name</returns>
        public virtual BeamProperty GetBeamProperty(string name)
        {
            return _beamProperties.GetElementByName(name);
        }

        /// <summary>
        /// The brick property with a name
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns>The property; null if no brick property has the name</returns>
        public virtual BrickProperty GetBrickProperty(string name)
        {
            return _volumeProperties.GetElementByName(name);
        }

        /// <summary>
        /// The names of the beam properties
        /// </summary>
        /// <returns>A new list with the names</returns>
        public List<string> GetBeamPropertyNames()
        {
            return _beamProperties.GetNames();
        }

        /// <summary>
        /// The names of the plate properties
        /// </summary>
        /// <returns>A new list with the names</returns>
        public List<string> GetPlatePropertyNames()
        {
            return _areaProperties.GetNames();
        }

        /// <summary>
        /// The names of the brick properties
        /// </summary>
        /// <returns>A new list with the names</returns>
        public List<string> GetBrickPropertyNames()
        {
            return _volumeProperties.GetNames();
        }

        #endregion

        #region LoadCase / FredomCase

        /// <summary>
        /// Adds a load case
        /// </summary>
        /// <param name="loadCase">The load case</param>
        /// <returns>Always true</returns>
        /// <exception cref="ArgumentException">If a load case with the same name exists</exception>
        public bool AddLoadCase(LoadCaseBase loadCase)
        {
            _loadCases.Add(loadCase.Name, loadCase);
            return true;
        }

        /// <summary>
        /// Adds load cases
        /// </summary>
        /// <param name="loadCaseBases">The load cases</param>
        /// <returns>False if <paramref name="loadCaseBases"/> is null</returns>
        /// <exception cref="ArgumentException">If a load case with the same name exists</exception>
        public bool AddLoadCases(IEnumerable<LoadCaseBase> loadCaseBases)
        {
            return _loadCases.AddRange(loadCaseBases);
        }

        /// <summary>
        /// The load case with a name
        /// </summary>
        /// <param name="loadCaseName">The name</param>
        /// <returns>The load case; null if no load case has the name</returns>
        public LoadCaseBase GetLoadCaseByName(string loadCaseName)
        {
            return _loadCases.GetElementByName(loadCaseName);
        }

        /// <summary>
        /// Adds a freedom case
        /// </summary>
        /// <param name="fredomCases">The freedom case</param>
        /// <returns>Always true</returns>
        /// <exception cref="ArgumentException">If a freedom case with the same name exists</exception>
        public bool AddFreedomCase(FreedomCase fredomCases)
        {
            _freedomCases.Add(fredomCases.Name, fredomCases);
            return true;
        }

        /// <summary>
        /// The freedom case with a name
        /// </summary>
        /// <param name="freedomCaseName">The name</param>
        /// <returns>The freedom case; null if no freedom case has the name</returns>
        public FreedomCase GetFreedomCaseByName(string freedomCaseName)
        {
            return _freedomCases.GetElementByName(freedomCaseName);
        }

        /// <summary>
        /// The load cases
        /// </summary>
        /// <returns>A new array with the load cases</returns>
        public LoadCaseBase[] GetLoadCases()
        {
            return _loadCases.Values.ToArray();
        }

        /// <summary>
        /// The names of the load cases
        /// </summary>
        /// <returns>A new array with the names</returns>
        public string[] GetLoadCaseNames()
        {
            return _loadCases.GetNames().ToArray();
        }

        /// <summary>
        /// The freedom cases
        /// </summary>
        /// <returns>A new array with the freedom cases</returns>
        public FreedomCase[] GetFreedomCases()
        {
            return _freedomCases.Values.ToArray();
        }

        /// <summary>
        /// The names of the freedom cases
        /// </summary>
        /// <returns>A new array with the names</returns>
        public string[] GetFreedomCaseNames()
        {
            return _freedomCases.GetNames().ToArray();
        }

        #endregion

        #region Combinations

        /// <summary>
        /// Adds a combination
        /// </summary>
        /// <param name="combination">The combination</param>
        /// <returns>Always true</returns>
        /// <exception cref="ArgumentException">If a combination with the same name exists</exception>
        public virtual bool AddCombination(Combination combination)
        {
            _combinations.Add(combination.Name, combination);
            return true;
        }

        /// <summary>
        /// Adds combinations
        /// </summary>
        /// <param name="combinations">The combinations</param>
        /// <returns>False if <paramref name="combinations"/> is null</returns>
        /// <exception cref="ArgumentException">If a combination with the same name exists</exception>
        public virtual bool AddCombinations(IEnumerable<Combination> combinations)
        {
            return _combinations.AddRange(combinations);
        }

        /// <summary>
        /// Adds a combination to a stage (see <see cref="StageCombinationsMap"/>)
        /// </summary>
        /// <param name="stageId">The id of the stage</param>
        /// <param name="combinationName">The name of the combination</param>
        /// <returns>False if the stage already has the combination</returns>
        internal bool AddStageCombinationMap(int stageId, string combinationName)
        {
            if (!_stageCombinationsMap.ContainsKey(stageId))
                _stageCombinationsMap[stageId] = new HashSet<string>();

            return _stageCombinationsMap[stageId].Add(combinationName);
        }

        /// <summary>
        /// Removes a combination from a stage (see <see cref="StageCombinationsMap"/>)
        /// </summary>
        /// <param name="stageId">The id of the stage</param>
        /// <param name="combinationName">The name of the combination</param>
        /// <returns>False if the stage does not have the combination</returns>
        /// <exception cref="KeyNotFoundException">If the stage has no combinations</exception>
        internal bool RemoveStageCombinationMap(int stageId, string combinationName)
        {
            return _stageCombinationsMap[stageId].Remove(combinationName);
        }

        /// <summary>
        /// The combinations
        /// </summary>
        /// <returns>A new array with the combinations</returns>
        public Combination[] GetCombinations()
        {
            return _combinations.Values.ToArray();
        }

        #endregion

        #region Groups

        /// <summary>
        /// Creates a group and adds it to the model
        /// </summary>
        /// <param name="name">The name of the group</param>
        /// <param name="partent">The parent group (optional)</param>
        /// <returns>The new group</returns>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is null or white space or a group with the same name exists</exception>
        public Group AddGroup(string name, Group partent = null)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrEmpty(name))
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));

            if (partent != null && (!Groups.TryGetValue(partent.Name, out var registeredParent) || !ReferenceEquals(registeredParent, partent)))
                throw new InvalidOperationException("UnregisteredParentGroup");
            if (Groups.ContainsKey(name)) throw new ArgumentException("Group already exists: " + name);
            Group group = new Group(name);
            partent?.AddChild(group);
            _groups.Add(group.Name, group);
            return group;
        }

        /// <summary>
        /// Assigns an existing group to elements
        /// </summary>
        /// <param name="elements">The elements</param>
        /// <param name="groupName">The name of the group</param>
        /// <returns>False for a missing group or null element. Existing memberships are idempotent; validation precedes changes.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="elements"/> is null</exception>
        /// <exception cref="ArgumentException">If <paramref name="groupName"/> is null or empty</exception>
        public bool SetGroup(IEnumerable<Element> elements, string groupName)
        {
            if (elements is null)
                throw new ArgumentNullException(nameof(elements));

            if (string.IsNullOrEmpty(groupName) || string.IsNullOrEmpty(groupName))
                throw new ArgumentException($"'{nameof(groupName)}' cannot be null or empty.", nameof(groupName));

            var group = _groups.GetElementByName(groupName);

            if (group == null)
                return false;

            var members = elements.ToArray();
            if (members.Any(element => element is null)) return false;
            AssignGroup(groupName, members);
            return true;
        }

        /// <summary>
        /// Assigns existing groups to elements
        /// </summary>
        /// <param name="elements">The elements</param>
        /// <param name="groupNames">The names of the groups</param>
        /// <returns>False if an element is null. All groups and members are validated before changing memberships.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="elements"/> or <paramref name="groupNames"/> is null</exception>
        /// <exception cref="ArgumentException">If a name is null or empty</exception>
        /// <exception cref="KeyNotFoundException">If a group does not exist</exception>
        public bool SetGroupRange(IEnumerable<Element> elements, IEnumerable<string> groupNames)
        {
            if (elements is null)
                throw new ArgumentNullException(nameof(elements));

            if (groupNames is null)
                throw new ArgumentNullException(nameof(groupNames));

            var names = groupNames.Distinct().ToArray();
            var members = elements.ToArray();
            if (members.Any(element => element is null)) return false;
            RegisteredElements(members);
            foreach (var name in names)
            {
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(name))
                    throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

                Group group = _groups[name];

                foreach (var element in members)
                {
                    if (element.Groups.TryGetValue(name, out var existing) && !ReferenceEquals(existing, group))
                        throw new InvalidOperationException("GroupIdentityConflict");
                }
            }
            foreach (var name in names) AssignGroup(name, members);
            return true;
        }

        /// <summary>
        /// The groups
        /// </summary>
        /// <returns>A new array with the groups</returns>
        public Group[] GetGroups()
        {
            return _groups.Values.ToArray();
        }


        #endregion

        #region Stages

        //// <summary>
        //// Add a stage the to the stage list. This stage will the clone of stage with <see cref="ModelObjectId.Id"/> equal to <paramref name="stageId"/>"/>
        //// </summary>
        //// <exception cref="ArgumentException">If stage with id equals to <paramref name="stageId"/> does not exist</exception>
        //public virtual Stage AddStage(int stageId)
        //{
        //    Stage stage = _stages.GetById(stageId);

        //    var stageCloned = new Stage(stage);
        //    _stages.Add(new Stage(stage));
        //    return stageCloned;
        //}

        /// <summary>
        /// The stages
        /// </summary>
        /// <returns>A new array with the stages</returns>
        public Stage[] GetStages()
        {
            return _stages.Values.ToArray();
        }

        /// <summary>
        /// The stage with an id
        /// </summary>
        /// <param name="stageId">The id</param>
        /// <returns>The stage; null if no stage has the id</returns>
        public Stage GetStageById(int stageId)
        {
            return _stages.GetById(stageId);
        }

        /// <summary>
        /// Tell if a stage has the id
        /// </summary>
        /// <param name="stageId">The id</param>
        /// <returns>True if the id is present</returns>
        public bool ContainsStageId(int stageId)
        {
            return _stages.Contains(stageId);
        }

        //public virtual IEnumerable<Combination> GetStageCombinations(int stageId)
        //{
        //    return _stages.GetById(stageId).GetCombinations();
        //}

        #endregion

        #endregion

        #region Add Get Geometry

        #region FiniteElements

        /// <summary>
        /// Adds a beam or area element, new nodes at its points (also if the model has nodes there) and its property (a property with the same name is
        /// replaced)
        /// </summary>
        /// <param name="finiteElement">The element</param>
        /// <exception cref="ArgumentNullException">If <paramref name="finiteElement"/> is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="finiteElement"/> is a <see cref="VolumeElement"/></exception>
        /// <exception cref="NotSupportedException">If <paramref name="finiteElement"/> is of another type</exception>
        /// <exception cref="InvalidOperationException">If the name of an attribute of the element is not the name of a load case (the element
        /// has already been added)</exception>
        public virtual void AddFiniteElement(Element finiteElement)
        {
            if (finiteElement is null)
                throw new ArgumentNullException(nameof(finiteElement));

            if (finiteElement is BeamElement beamElement)
            {
                _nodesElements.Add(new NodeElement(beamElement.StartPoint));
                _nodesElements.Add(new NodeElement(beamElement.EndPoint));
                _beamElements.Add(beamElement);
                if (!_beamProperties.ContainsKey(beamElement.BeamProperty.Name))
                    _beamProperties.Add(beamElement.BeamProperty.Name, beamElement.BeamProperty);
                else
                    _beamProperties[beamElement.BeamProperty.Name] = beamElement.BeamProperty;
            }
            else if (finiteElement is AreaElement areaElement)
            {
                for (int i = 0; i < areaElement.Points.Length; i++)
                    _nodesElements.Add(new NodeElement(areaElement.Points[i]));
                _areaElements.Add(areaElement);
                if (!_areaProperties.ContainsKey(areaElement.PlateProperty.Name))
                    _areaProperties.Add(areaElement.PlateProperty.Name, areaElement.PlateProperty);
                else
                    _areaProperties[areaElement.PlateProperty.Name] = areaElement.PlateProperty;
            }
            else if (finiteElement is VolumeElement)
            {
                throw new ArgumentOutOfRangeException("");
            }
            else
            {
                throw new NotSupportedException(finiteElement.GetType().ToString());
            }

            foreach (KeyValuePair<int, Attributes.Attribute> attribute in finiteElement.Attributes)
            {
                if (!LoadCaseExist(attribute.Value.Name))
                    throw new InvalidOperationException($"Loadcase {attribute.Value.Name} does not exist in the femModel");
            }
        }

        /// <summary>
        /// Adds beam or area elements (see <see cref="AddFiniteElement(Element)"/>)
        /// </summary>
        /// <param name="finiteElements">The elements</param>
        public virtual void AddFiniteElements(Element[] finiteElements)
        {
            foreach (var element in finiteElements)
            {
                AddFiniteElement(element);
            }
        }

        /// <summary>
        /// The beam element with an id
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>The element</returns>
        /// <exception cref="KeyNotFoundException">If no beam element has the id</exception>
        public BeamElement GetBeamElement(int id)
        {
            return _beamElements[id];
        }

        /// <summary>
        /// The node with an id
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>The node</returns>
        /// <exception cref="KeyNotFoundException">If no node has the id</exception>
        public NodeElement GetNodeElement(int id)
        {
            return _nodesElements[id];
        }

        /// <summary>
        /// The area element with an id
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>The element</returns>
        /// <exception cref="KeyNotFoundException">If no area element has the id</exception>
        public AreaElement GetAreaElement(int id)
        {
            return _areaElements[id];
        }

        /// <summary>
        /// Tell if a beam property has the name
        /// </summary>
        /// <param name="propertyName">The name</param>
        /// <returns>True if the name is present</returns>
        public virtual bool ContainsBeamProperty(string propertyName)
        {
            return _beamProperties.ContainsKey(propertyName);
        }

        /// <summary>
        /// Tell if a plate property has the name
        /// </summary>
        /// <param name="propertyName">The name</param>
        /// <returns>True if the name is present</returns>
        public virtual bool ContainsAreaProperty(string propertyName)
        {
            return _areaProperties.ContainsKey(propertyName);
        }

        /// <summary>
        /// Tell if a brick property has the name
        /// </summary>
        /// <param name="propertyName">The name</param>
        /// <returns>True if the name is present</returns>
        public virtual bool ContainsVolumeProperty(string propertyName)
        {
            return _volumeProperties.ContainsKey(propertyName);
        }

        #endregion

        #region Nodes

        /// <summary>
        /// Adds a node (see <see cref="SortedCollection{T}.Add(T)"/>)
        /// </summary>
        /// <param name="node">The node</param>
        /// <returns>The id of the node</returns>
        /// <exception cref="NullReferenceException">If <paramref name="node"/> is null</exception>
        /// <exception cref="InvalidOperationException">If the name of an attribute of the node is not the name of a load case</exception>
        protected virtual int AddNode(NodeElement node)
        {
            foreach (var attribute in node.Attributes)
            {
                if (!LoadCaseExist(attribute.Value.Name))
                    throw new InvalidOperationException($"Loadcase {attribute.Value.Name} does not exist in the femModel");
            }

            return _nodesElements.Add(node); // l'Add lancia un ArgumentNullException se gli si passa null
        }

        /// <summary>
        /// Adds nodes (see <see cref="AddNode(NodeElement)"/>)
        /// </summary>
        /// <param name="nodes">The nodes</param>
        /// <returns>The ids of the nodes</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="nodes"/> is null</exception>
        /// <exception cref="InvalidOperationException">If the name of an attribute of a node is not the name of a load case</exception>
        protected virtual int[] AddNodes(NodeElement[] nodes)
        {
            if (nodes != null)
            {
                int[] indexes = new int[nodes.Length];
                for (int i = 0; i < nodes.Length; i++)
                {
                    indexes[i] = AddNode(nodes[i]);

                    foreach (var attribute in nodes[i].Attributes)
                    {
                        if (!LoadCaseExist(attribute.Value.Name))
                            throw new InvalidOperationException($"Loadcase {attribute.Value.Name} does not exist in the femModel");
                    }
                }

                return indexes;
            }
            throw new ArgumentNullException();
        }

        /// <summary>
        /// The node with an id
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>The node</returns>
        /// <exception cref="KeyNotFoundException">If no node has the id</exception>
        public virtual NodeElement GetNode(int id)
        {
            return _nodesElements[id];
        }

        /// <summary>
        /// An enumerator of the nodes (sorted by id)
        /// </summary>
        /// <returns>The enumerator</returns>
        public virtual IEnumerator<NodeElement> GetNodesEnumerator()
        {
            return _nodesElements.Values.GetEnumerator();
        }

        /// <summary>
        /// The nodes (sorted by id)
        /// </summary>
        /// <returns>A new array with the nodes</returns>
        public NodeElement[] GetNodes()
        {
            return _nodesElements.Values.ToArray();
        }


        #endregion

        #region Costrain

        /// <summary>
        /// Adds a costrain and its nodes (see <see cref="AddNode(NodeElement)"/> and <see cref="UniqueIdCollection{T}.Add(T)"/>)
        /// </summary>
        /// <param name="costrain">The costrain</param>
        /// <returns>The id of the costrain</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="costrain"/> is null</exception>
        public virtual int AddCostrain(Costrain costrain)
        {
            if (costrain is null)
                throw new ArgumentNullException(nameof(costrain));

            AddNode(costrain.StartNode);
            AddNodes(costrain.EndNodes);

            _costrains.Add(costrain);
            return costrain.Id;
        }

        /// <summary>
        /// Adds costrains and their nodes (see <see cref="AddCostrain(Costrain)"/>)
        /// </summary>
        /// <param name="costrains">The costrains</param>
        public virtual void AddCostrains(IEnumerable<Costrain> costrains)
        {
            foreach (var costrain in costrains)
            {
                AddCostrain(costrain);
            }
        }

        /// <summary>
        /// Tell if a costrain has the id of the given one
        /// </summary>
        /// <param name="costrain">The costrain</param>
        /// <returns>True if the id of <paramref name="costrain"/> is present</returns>
        public virtual bool ContainsCostrains(Costrain costrain)
        {
            return _costrains.ContainsKey(costrain.Id);
        }


        /// <summary>
        /// The costrain with an id
        /// </summary>
        /// <param name="index">The id</param>
        /// <returns>The costrain</returns>
        /// <exception cref="KeyNotFoundException">If no costrain has the id</exception>
        public virtual Costrain GetCostrain(int index)
        {
            return _costrains[index];
        }

        /// <summary>
        /// The costrains
        /// </summary>
        /// <returns>A new array with the costrains</returns>
        public virtual Costrain[] GetCostrains()
        {
            return _costrains.Values.ToArray();
        }


        /// <summary>
        /// An enumerator of the costrains
        /// </summary>
        /// <returns>The enumerator</returns>
        public virtual IEnumerator<Costrain> GetCostrainEnumerator()
        {
            return _costrains.Values.GetEnumerator();
        }


        #endregion

        #region Mesh

        /// <summary>
        /// Adds meshes (see <see cref="AddMesh(Mesh, string, string, Dictionary{IPointLoad, int[]}, Dictionary{ILineLoad, int[]}, Dictionary{IAreaLoad, int[]}, Dictionary{GeometryRestrain, int[]}, out Dictionary{int, int}, out Dictionary{int, int}, out Dictionary{int, int}, string)"/>):
        /// the lists have one item for each mesh. Currently the meshes are never added: a non empty property name throws
        /// <see cref="ArgumentNullException"/> (the check is inverted) and <see cref="AddMesh(Mesh, string, string, Dictionary{IPointLoad, int[]}, Dictionary{ILineLoad, int[]}, Dictionary{IAreaLoad, int[]}, Dictionary{GeometryRestrain, int[]}, out Dictionary{int, int}, out Dictionary{int, int}, out Dictionary{int, int}, string)"/>
        /// is not called (the status starts false)
        /// </summary>
        /// <param name="meshes">The meshes</param>
        /// <param name="platePropertyNames">The names of the plate properties of the faces</param>
        /// <param name="brickPropertyName">The names of the brick properties of the volumes</param>
        /// <param name="vertexLoadMeshEntityMap">The maps between point loads and <see cref="MeshVertex"/>.Id</param>
        /// <param name="vertexLineLoadMeshEntityMap">The maps between line loads and <see cref="MeshVertex"/>.Id</param>
        /// <param name="plateLoadMeshEntityMap">The maps between area loads and <see cref="MeshFace"/>.Id</param>
        /// <param name="restrainMeshEntityMap">The maps between restrains and <see cref="MeshVertex"/>.Id</param>
        /// <param name="nodesNewIndexMap">A map between the <see cref="MeshVertex"/>.Id of <paramref name="meshes"/> and the id of the same nodes in the femModel</param>
        /// <param name="platesNewIndexMap">A map between the <see cref="MeshFace"/>.Id of <paramref name="meshes"/> and the id of the same plate in the femModel</param>
        /// <param name="brickNewIndexMap">A map between the <see cref="MeshVolume"/>.Id of <paramref name="meshes"/> and the id of the same brick in the femModel</param>
        /// <returns>True if all the meshes have been added</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="meshes"/> or one of the meshes or maps is null</exception>
        /// <exception cref="ArgumentException">If the sizes of the lists do not match</exception>
        public virtual bool AddMeshes(List<Mesh> meshes, List<string> platePropertyNames, List<string> brickPropertyName, List<Dictionary<IPointLoad, int[]>> vertexLoadMeshEntityMap,
                                      List<Dictionary<ILineLoad, int[]>> vertexLineLoadMeshEntityMap, List<Dictionary<IAreaLoad, int[]>> plateLoadMeshEntityMap, List<Dictionary<GeometryRestrain, int[]>> restrainMeshEntityMap,
                                      out List<Dictionary<int, int>> nodesNewIndexMap, out List<Dictionary<int, int>> platesNewIndexMap, out List<Dictionary<int, int>> brickNewIndexMap)
        {
            //List<(int[] nodesId, int[] platesId, int[] volumesId)> elementsIndexes = new List<(int[] nodesId, int[] platesId, int[] volumesId)>();

            if (meshes is null)
                throw new ArgumentNullException(nameof(meshes));

            // Garantisce la stessa lunghezza delle liste, ma non che siano liste di non nulli
            if (meshes.Select(i => i.Faces.Count).Max() != 0 && meshes.Count != platePropertyNames.Count)
                throw new ArgumentException($"Size of {nameof(meshes)} and {nameof(platePropertyNames)} are different");
            if (meshes.Select(i => i.Volumes.Count).Max() != 0 && meshes.Count != brickPropertyName.Count)
                throw new ArgumentException($"Size of {nameof(meshes)} and {nameof(brickPropertyName)} are different");
            if (meshes.Count != vertexLoadMeshEntityMap.Count)
                throw new ArgumentException($"Size of {nameof(meshes)} and {nameof(vertexLoadMeshEntityMap)} are different");
            if (meshes.Count != vertexLineLoadMeshEntityMap.Count)
                throw new ArgumentException($"Size of {nameof(meshes)} and {nameof(vertexLineLoadMeshEntityMap)} are different");
            if (meshes.Select(i => i.Faces.Count).Max() != 0 && meshes.Count != plateLoadMeshEntityMap.Count)
                throw new ArgumentException($"Size of {nameof(meshes)} and {nameof(plateLoadMeshEntityMap)} are different");
            if (meshes.Count != restrainMeshEntityMap.Count)
                throw new ArgumentException($"Size of {nameof(meshes)} and {nameof(restrainMeshEntityMap)} are different");

            nodesNewIndexMap = new List<Dictionary<int, int>>();
            platesNewIndexMap = new List<Dictionary<int, int>>();
            brickNewIndexMap = new List<Dictionary<int, int>>();

            bool status = false;
            for (int i = 0; i < meshes.Count; i++)
            {
                if (meshes[i] is null)
                    throw new ArgumentNullException(nameof(meshes));

                if (!string.IsNullOrEmpty(platePropertyNames[i]) || !string.IsNullOrWhiteSpace(platePropertyNames[i]))
                    throw new ArgumentNullException(nameof(platePropertyNames));

                if (!string.IsNullOrEmpty(brickPropertyName[i]) || !string.IsNullOrWhiteSpace(brickPropertyName[i]))
                    throw new ArgumentNullException(nameof(brickPropertyName));

                if (vertexLoadMeshEntityMap[i] is null)
                    throw new ArgumentNullException(nameof(vertexLoadMeshEntityMap));

                if (vertexLineLoadMeshEntityMap[i] is null)
                    throw new ArgumentNullException(nameof(vertexLineLoadMeshEntityMap));

                if (plateLoadMeshEntityMap[i] is null)
                    throw new ArgumentNullException(nameof(plateLoadMeshEntityMap));

                if (restrainMeshEntityMap[i] is null)
                    throw new ArgumentNullException(nameof(restrainMeshEntityMap));

                Dictionary<int, int> singleNodesNewIndexMap = new Dictionary<int, int>();
                Dictionary<int, int> singlePlatesNewIndexMap = new Dictionary<int, int>();
                Dictionary<int, int> singleBrickNewIndexMap = new Dictionary<int, int>();

                status = status && AddMesh(meshes[i], platePropertyNames[i], brickPropertyName[i], vertexLoadMeshEntityMap[i], vertexLineLoadMeshEntityMap[i], plateLoadMeshEntityMap[i],
                    restrainMeshEntityMap[i], out singleNodesNewIndexMap, out singlePlatesNewIndexMap, out singleBrickNewIndexMap);

                nodesNewIndexMap.Add(singleNodesNewIndexMap);
                platesNewIndexMap.Add(singlePlatesNewIndexMap);
                brickNewIndexMap.Add(singleBrickNewIndexMap);
            }

            return status;
        }

        /// <summary>
        /// Adds a mesh to the model (see <see cref="AddMesh(Mesh, string, string, Dictionary{IPointLoad, int[]}, Dictionary{ILineLoad, int[]}, Dictionary{IAreaLoad, int[]}, Dictionary{GeometryRestrain, int[]}, out Dictionary{int, int}, out Dictionary{int, int}, out Dictionary{int, int}, string)"/>)
        /// </summary>
        /// <param name="mesh">The mesh</param>
        /// <param name="platePropertyName">The name of the plate property of the faces</param>
        /// <param name="brickPropertyName">The name of the brick property of the volumes</param>
        /// <param name="vertexLoadMeshEntityMap">Map between <see cref="IPointLoad"/> and <see cref="MeshVertex"/>.Id</param>
        /// <param name="vertexLineLoadMeshEntityMap">Map between <see cref="ILineLoad"/> and <see cref="MeshVertex"/>.Id</param>
        /// <param name="plateLoadMeshEntityMap">Map between <see cref="IAreaLoad"/> and <see cref="MeshFace"/>.Id</param>
        /// <param name="restrainMeshEntityMap">Map between <see cref="GeometryRestrain"/> and <see cref="MeshVertex"/>.Id</param>
        /// <param name="groupName">The name of a new group assigned to the new nodes and elements (optional)</param>
        /// <returns>Always true</returns>
        public virtual bool AddMesh(Mesh mesh, string platePropertyName, string brickPropertyName, Dictionary<IPointLoad, int[]> vertexLoadMeshEntityMap,
            Dictionary<ILineLoad, int[]> vertexLineLoadMeshEntityMap, Dictionary<IAreaLoad, int[]> plateLoadMeshEntityMap, Dictionary<GeometryRestrain, int[]> restrainMeshEntityMap, string groupName = "")
        {
            return AddMesh(mesh, platePropertyName, brickPropertyName, vertexLoadMeshEntityMap, vertexLineLoadMeshEntityMap, plateLoadMeshEntityMap,
                restrainMeshEntityMap, out _, out _, out _, groupName);
        }

        /// <summary>
        /// Adds a mesh to the model: a new node for each vertex (also if the model has a node there), an area element for each face, a volume
        /// element for each volume, the restrains and the loads. The load cases not in the model are added
        /// </summary>
        /// <param name="mesh">The mesh</param>
        /// <param name="platePropertyName">The name of the plate property of the faces (it must exist if the mesh has faces)</param>
        /// <param name="brickPropertyName">The name of the brick property of the volumes (it must exist if the mesh has volumes)</param>
        /// <param name="vertexLoadMeshEntityMap">Map between <see cref="IPointLoad"/> and <see cref="MeshVertex"/>.Id: a <see cref="PointLoad"/> is
        /// added to each node</param>
        /// <param name="vertexLineLoadMeshEntityMap">Map between <see cref="ILineLoad"/> and <see cref="MeshVertex"/>.Id: the load of a
        /// <see cref="LineLoad"/> is lumped on the nodes (length / (nodes - 1), halved at the ends of the line)</param>
        /// <param name="plateLoadMeshEntityMap">Map between <see cref="IAreaLoad"/> and <see cref="MeshFace"/>.Id: the load is added to each area
        /// element</param>
        /// <param name="restrainMeshEntityMap">Map between <see cref="GeometryRestrain"/> and <see cref="MeshVertex"/>.Id: the restrains and the
        /// stiffnesses are added to each node</param>
        /// <param name="nodesNewIndexMap">A map between the <see cref="MeshVertex"/>.Id of <paramref name="mesh"/> and the id of the same nodes in the femModel</param>
        /// <param name="platesNewIndexMap">A map between the <see cref="MeshFace"/>.Id of <paramref name="mesh"/> and the id of the same plate in the femModel</param>
        /// <param name="brickNewIndexMap">A map between the <see cref="MeshVolume"/>.Id of <paramref name="mesh"/> and the id of the same brick in the femModel</param>
        /// <param name="groupName">The name of a new group assigned to the new nodes and elements (optional)</param>
        /// <returns>Always true</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="mesh"/> is null</exception>
        /// <exception cref="ArgumentException">If a group named <paramref name="groupName"/> exists or a load case of a load has the name of a
        /// different load case of the model</exception>
        /// <exception cref="NotImplementedException">If the property of the faces or of the volumes does not exist or a load is of another type</exception>
        /// <exception cref="KeyNotFoundException">If an id of the maps is neither a vertex of <paramref name="mesh"/> nor a node or area element of the model</exception>
        /// <remarks>The ids of the maps not found in <paramref name="mesh"/> are used as ids of nodes (or area elements) of the model. The point
        /// loads get the instance of the load case of the model; the line and area loads keep their instance</remarks>
        public virtual bool AddMesh(Mesh mesh, string platePropertyName, string brickPropertyName, Dictionary<IPointLoad, int[]> vertexLoadMeshEntityMap,
            Dictionary<ILineLoad, int[]> vertexLineLoadMeshEntityMap, Dictionary<IAreaLoad, int[]> plateLoadMeshEntityMap, Dictionary<GeometryRestrain, int[]> restrainMeshEntityMap,
            out Dictionary<int, int> nodesNewIndexMap, out Dictionary<int, int> platesNewIndexMap, out Dictionary<int, int> brickNewIndexMap, string groupName = "")
        {
            if (mesh is null)
                throw new ArgumentNullException(nameof(mesh));

            PlateProperty plateProperty = null;
            BrickProperty brickProperty = null;

            Group group = null;

            // Gruppi
            if (!string.IsNullOrEmpty(groupName) && !string.IsNullOrWhiteSpace(groupName))
                group = AddGroup(groupName);

            // Aggiorno la lista proprietà
            if (mesh.Faces.Count != 0)
            {
                plateProperty = GetPlateProperty(platePropertyName);
            }

            if (mesh.Volumes.Count != 0)
            {
                brickProperty = GetBrickProperty(brickPropertyName);
            }

            Dictionary<int, int> nodesMap = new Dictionary<int, int>(); // Mappa tra indici dei nodi dentro _nodes e indici dei vertici della mesh nel caso esistano già dentro _nodes.

            // Aggiunge nodi alla collection di nodi
            using (var enumerator = mesh.GetVerticesEnumerator())
            {
                for (int i = 0; i < mesh.VerticesCount; i++)
                {
                    enumerator.MoveNext();
                    if (group != null)
                    {
                        NodeElement nodeElm = new NodeElement(enumerator.Current.Point);
                        nodeElm.AddGroup(group);
                        nodesMap[enumerator.Current.Id] = _nodesElements.Add(nodeElm);
                    }
                    else
                    {
                        nodesMap[enumerator.Current.Id] = _nodesElements.Add(new NodeElement(enumerator.Current.Point));
                    }
                }
            }

            // Aggiunge elementi FEM
            // Aggiunge Faces
            Dictionary<int, int> platesMap = new Dictionary<int, int>();
            var faces = mesh.Faces.ToArray();
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                if (plateProperty is PlateProperty)
                {
                    if (faces[i].IsQuad)
                    {
                        AreaElement plate = new AreaElement(new Shape(new Polygon3d {
                                _nodesElements[nodesMap[faces[i].A]].Position,
                                _nodesElements[nodesMap[faces[i].B]].Position,
                                _nodesElements[nodesMap[faces[i].C]].Position,
                                _nodesElements[nodesMap[faces[i].D]].Position}), plateProperty);
                        plate.ConnectNodes(_nodesElements[nodesMap[faces[i].A]], _nodesElements[nodesMap[faces[i].B]],
                            _nodesElements[nodesMap[faces[i].C]], _nodesElements[nodesMap[faces[i].D]]);
                        if (group != null)
                            plate.AddGroup(group);

                        platesMap[faces[i].Id] = _areaElements.Add(plate);
                    }
                    else
                    {
                        AreaElement plate = new AreaElement(new Shape(new Polygon3d {
                                _nodesElements[nodesMap[faces[i].A]].Position,
                                _nodesElements[nodesMap[faces[i].B]].Position,
                                _nodesElements[nodesMap[faces[i].C]].Position}), plateProperty);
                        plate.ConnectNodes(_nodesElements[nodesMap[faces[i].A]], _nodesElements[nodesMap[faces[i].B]],
                            _nodesElements[nodesMap[faces[i].C]]);
                        if (group != null)
                            plate.AddGroup(group);

                        platesMap[faces[i].Id] = _areaElements.Add(plate);
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            // Aggiunge Volumes
            Dictionary<int, int> brickMap = new Dictionary<int, int>();
            var volumes = mesh.Volumes.ToArray();
            for (int i = 0; i < mesh.Volumes.Count; i++)
            {
                if (volumes[i].IsQuadrangularPrism)
                {
                    if (brickProperty is BrickProperty bp)
                    {
                        VolumeElement brick = new VolumeElement(new NodeElement[] {
                                _nodesElements[nodesMap[volumes[i].A]],
                                _nodesElements[nodesMap[volumes[i].B]],
                                _nodesElements[nodesMap[volumes[i].C]],
                                _nodesElements[nodesMap[volumes[i].D]],
                                _nodesElements[nodesMap[volumes[i].E]],
                                _nodesElements[nodesMap[volumes[i].F]],
                                _nodesElements[nodesMap[volumes[i].G]],
                                _nodesElements[nodesMap[volumes[i].H]]
                            }, bp);

                        if (group != null)
                            brick.AddGroup(group);

                        brickMap[volumes[i].Id] = _volumeElements.Add(brick);
                    }
                    else
                        throw new NotImplementedException();
                }
                else
                {
                    if (brickProperty is BrickProperty bp)
                    {
                        VolumeElement brick = new VolumeElement(new NodeElement[] {
                                _nodesElements[nodesMap[volumes[i].A]],
                                _nodesElements[nodesMap[volumes[i].B]],
                                _nodesElements[nodesMap[volumes[i].C]],
                                _nodesElements[nodesMap[volumes[i].D]],
                                _nodesElements[nodesMap[volumes[i].E]],
                                _nodesElements[nodesMap[volumes[i].F]]
                            }, bp);

                        if (group != null)
                            brick.AddGroup(group);

                        brickMap[volumes[i].Id] = _volumeElements.Add(brick);
                    }
                    else
                        throw new NotImplementedException();
                }
            }

            // Gestione restrain 
            if (restrainMeshEntityMap != null)
            {
                foreach (var kvp in restrainMeshEntityMap)
                {
                    GeometryRestrain geometryRestrain = kvp.Key;
                    int[] indexes = kvp.Value;

                    Dictionary<GeometryRestrain.DOF, bool> restrains = geometryRestrain.GetRestrains();
                    Dictionary<GeometryRestrain.DOF, double> stiffneses = geometryRestrain.GetStiffnesses();
                    Dictionary<GeometryRestrain.DOF, double> displacements = geometryRestrain.GetImposedDisplacement();

                    NodeRestrain nra = new NodeRestrain((NodeElement)geometryRestrain.GetElement(), geometryRestrain.CoordinateSystem);
                    NodeRestrain nsa = new NodeRestrain((NodeElement)geometryRestrain.GetElement(), geometryRestrain.CoordinateSystem);

                    // TODO:  gestire il fatto che uno spostamento imposto può essere applicato in un grado di libertà vincolato
                    foreach (KeyValuePair<GeometryRestrain.DOF, bool> restrain in restrains)
                    {
                        if (restrain.Value)
                            nra.AddExternalRestrain(restrain.Key);
                    }

                    foreach (KeyValuePair<GeometryRestrain.DOF, double> displacement in displacements)
                    {
                        nra.AddImposedDisplacement(displacement.Key, displacement.Value);
                    }

                    foreach (KeyValuePair<GeometryRestrain.DOF, double> stiffness in stiffneses)
                    {
                        nsa.AddStiffness(stiffness.Key, stiffness.Value);
                    }

                    //foreach (var index in indexes)
                    for (int i = 0; i < indexes.Length; i++)
                    {
                        int nodeId = nodesMap.ContainsKey(indexes[i]) ? nodesMap[indexes[i]] : indexes[i];

                        NodeElement node = _nodesElements[nodeId];

                        if (nra.Restrains.Count > 0)
                            node.Attributes.Add(nra);

                        if (nsa.Restrains.Where(j => j.HasStiffness).Count() > 0)
                            node.Attributes.Add(nsa);

                    }
                }
            }

            // Gestione carichi
            if (vertexLoadMeshEntityMap != null)
            {
                foreach (var kvp in vertexLoadMeshEntityMap)
                {
                    IPointLoad load = kvp.Key;
                    int[] indexes = kvp.Value;

                    var lc = (load as Load).LoadCase;

                    LoadCaseBase loadCase;
                    if (LoadCaseExist(lc.Name))
                    {
                        loadCase = GetLoadCaseByName(lc.Name);
                        if (!loadCase.Equals(lc))
                            throw new ArgumentException($"LoadCase {lc.Name} is not equal to the one inside the FemModel");
                    }
                    else
                    {
                        if (AddLoadCase(lc))
                            loadCase = lc;
                        else
                            throw new InvalidOperationException();
                    }

                    //foreach (var index in indexes)
                    for (int i = 0; i < indexes.Length; i++)
                    {
                        int nodeId = nodesMap.ContainsKey(indexes[i]) ? nodesMap[indexes[i]] : indexes[i];

                        NodeElement node = _nodesElements[nodeId]; // se non trova l'indice viene lanciata una keynotfoundException

                        if (load is PointLoad pl)
                        {
                            PointLoad nfa = new PointLoad(pl.F1, pl.F2, pl.F3, pl.M1, pl.M2, pl.M3, node.Position, loadCase, pl.CoordinateSystem);
                            node.Loads.Add(nfa);
                        }
                        else
                            throw new NotImplementedException();
                    }
                }
            }

            // Gestione carichi
            if (vertexLineLoadMeshEntityMap != null)
            {
                foreach (var kvp in vertexLineLoadMeshEntityMap)
                {
                    ILineLoad load = kvp.Key;
                    int[] indexes = kvp.Value;
                    var lineLenght = load.GetGeometry().GetLength();

                    var lc = (load as Load).LoadCase;

                    LoadCaseBase loadCase;
                    if (LoadCaseExist(lc.Name))
                    {
                        loadCase = GetLoadCaseByName(lc.Name);
                        if (!loadCase.Equals(lc))
                            throw new ArgumentException($"LoadCase {lc.Name} is not equal to the one inside the FemModel");
                    }
                    else
                    {
                        if (AddLoadCase(lc))
                            loadCase = lc;
                        else
                            throw new InvalidOperationException();
                    }

                    //foreach (var index in indexes)
                    for (int i = 0; i < indexes.Length; i++)
                    {
                        int nodeId = nodesMap.ContainsKey(indexes[i]) ? nodesMap[indexes[i]] : indexes[i];

                        NodeElement node = _nodesElements[nodeId];

                        if (load is LineLoad ll)
                        {
                            // carico è F/L (FL/L nel caso di momento)
                            // carico su nodo intermedio: F/L / (nnodi - 1)
                            // carico su nodo estremità: F/L / (nnodi - 1) / 2.0
                            // se per qualche motivo l'equals start/end non funziona, viene applicato più carico

                            var line = ll.GetGeometry();
                            var factor = lineLenght / (indexes.Count() - 1);

                            if (node.Equals(line.Start) || node.Equals(line.End))
                                factor /= 2.0;

                            PointLoad pointLoad = new PointLoad(ll.F1 * factor, ll.F2 * factor, ll.F3 * factor,
                                ll.M1 * factor, ll.M2 * factor, ll.M3 * factor, node.Position, ll.LoadCase, ll.CoordinateSystem);

                            node.Loads.Add(pointLoad);
                        }
                        else
                            throw new NotImplementedException();
                    }
                }
            }

            // Gestione carichi
            if (plateLoadMeshEntityMap != null)
            {
                foreach (var kvp in plateLoadMeshEntityMap)
                {
                    IAreaLoad load = kvp.Key;
                    int[] indexes = kvp.Value;

                    var lc = (load as Load).LoadCase;

                    LoadCaseBase loadCase;
                    if (LoadCaseExist(lc.Name))
                    {
                        loadCase = GetLoadCaseByName(lc.Name);
                        if (!loadCase.Equals(lc))
                            throw new ArgumentException($"LoadCase {lc.Name} is not equal to the one inside the FemModel");
                    }
                    else
                    {
                        if (AddLoadCase(lc))
                            loadCase = lc;
                        else
                            throw new InvalidOperationException();
                    }

                    //foreach (var index in indexes)
                    for (int i = 0; i < indexes.Length; i++)
                    {
                        int plateId = platesMap.ContainsKey(indexes[i]) ? platesMap[indexes[i]] : indexes[i];
                        AreaElement areaElement = _areaElements[plateId];

                        if (load is NormalAreaLoad pl)
                        {
                            areaElement.Loads.Add(pl); ;
                        }
                        else if (load is AreaLoad gal)
                        {
                            areaElement.Loads.Add(gal);
                        }
                        else
                            throw new NotImplementedException();
                    }
                }
            }

            nodesNewIndexMap = nodesMap;
            platesNewIndexMap = platesMap;
            brickNewIndexMap = brickMap;

            return true;
        }

        /// <summary>
        /// A mesh with a face for each area and volume element (it throws <see cref="NullReferenceException"/> if the volume elements are null, see
        /// <see cref="Model(string)"/>)
        /// </summary>
        /// <returns>The new mesh</returns>
        public virtual Mesh GetMesh()
        {
            Mesh mesh = new Mesh();

            foreach (var element in _areaElements)
            {
                mesh.AddFaceMesh(element.Value.Points);
            }

            foreach (var element in _volumeElements)
            {
                mesh.AddFaceMesh(element.Value.Nodes.Select(i => i.Position).ToArray());
            }

            return mesh;
        }

        #endregion

        #endregion

        #region Results

        /// <summary>
        /// The results of the nodes for a combination
        /// </summary>
        /// <param name="combination">The combination</param>
        /// <param name="group">If not null, only the nodes of this group</param>
        /// <returns>The results related to <paramref name="combination"/></returns>
        /// <remarks>The nodes are read by id from 0 to count - 1</remarks>
        public ResultLocation[] GetCombinationNodeResults(Combination combination, Group group = null)
        {
            List<ResultLocation> results = new List<ResultLocation>();
            if (group != null)
            {
                for (int i = 0; i < _nodesElements.Count; i++)
                {
                    NodeElement nodeElement = _nodesElements[i];

                    if (nodeElement.ContainsGroup(group))
                    {
                        for (int j = 0; j < nodeElement.Results.Count; j++)
                        {
                            ElementResult nodeElementResults = nodeElement.Results[j];
                            for (int k = 0; k < nodeElementResults.Results.Count; k++)
                            {
                                ResultLocation res = nodeElementResults.Results[k];
                                if (res.Case.Equals(combination))
                                {
                                    results.Add(res);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < _nodesElements.Count; i++)
                {
                    NodeElement nodeElement = _nodesElements[i];

                    for (int j = 0; j < nodeElement.Results.Count; j++)
                    {
                        ElementResult nodeElementResults = nodeElement.Results[j];
                        for (int k = 0; k < nodeElementResults.Results.Count; k++)
                        {
                            ResultLocation res = nodeElementResults.Results[k];
                            if (res.Case.Equals(combination))
                            {
                                results.Add(res);
                            }
                        }
                    }
                }
            }

            return results.ToArray();
        }

        /// <summary>
        /// The results of the beam elements for a combination
        /// </summary>
        /// <param name="combination">The combination</param>
        /// <param name="group">If not null, only the elements of this group</param>
        /// <returns>The results related to <paramref name="combination"/></returns>
        /// <remarks>The elements are read by id from 0 to count - 1</remarks>
        public ResultLocation[] GetCombinationBeamResults(Combination combination, Group group = null)
        {
            List<ResultLocation> results = new List<ResultLocation>();
            if (group != null)
            {
                for (int i = 0; i < _beamElements.Count; i++)
                {
                    BeamElement element = _beamElements[i];

                    if (element.ContainsGroup(group))
                    {
                        for (int j = 0; j < element.Results.Count; j++)
                        {
                            ElementResult nodeElementResults = element.Results[j];
                            for (int k = 0; k < nodeElementResults.Results.Count; k++)
                            {
                                ResultLocation res = nodeElementResults.Results[k];
                                if (res.Case.Equals(combination))
                                {
                                    results.Add(res);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < _beamElements.Count; i++)
                {
                    BeamElement element = _beamElements[i];

                    for (int j = 0; j < element.Results.Count; j++)
                    {
                        ElementResult nodeElementResults = element.Results[j];
                        for (int k = 0; k < nodeElementResults.Results.Count; k++)
                        {
                            ResultLocation res = nodeElementResults.Results[k];
                            if (res.Case.Equals(combination))
                            {
                                results.Add(res);
                            }
                        }
                    }
                }
            }

            return results.ToArray();
        }

        /// <summary>
        /// The results of the area elements for a combination
        /// </summary>
        /// <param name="combination">The combination</param>
        /// <param name="group">If not null, only the elements of this group</param>
        /// <returns>The results related to <paramref name="combination"/></returns>
        /// <remarks>The elements are read by id from 0 to count - 1</remarks>
        public ResultLocation[] GetCombinationAreaResults(Combination combination, Group group = null)
        {
            List<ResultLocation> results = new List<ResultLocation>();
            if (group != null)
            {
                foreach (AreaElement element in _areaElements.Values)
                {

                    if (element.ContainsGroup(group))
                    {
                        for (int j = 0; j < element.Results.Count; j++)
                        {
                            ElementResult nodeElementResults = element.Results[j];
                            for (int k = 0; k < nodeElementResults.Results.Count; k++)
                            {
                                ResultLocation res = nodeElementResults.Results[k];
                                if (res.Case.Equals(combination))
                                {
                                    results.Add(res);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (AreaElement element in _areaElements.Values)
                {

                    for (int j = 0; j < element.Results.Count; j++)
                    {
                        ElementResult nodeElementResults = element.Results[j];
                        for (int k = 0; k < nodeElementResults.Results.Count; k++)
                        {
                            ResultLocation res = nodeElementResults.Results[k];
                            if (res.Case.Equals(combination))
                            {
                                results.Add(res);
                            }
                        }
                    }
                }
            }

            return results.ToArray();
        }

        #endregion

        #endregion

        #region Edits

        /// <summary>
        /// Merges the coincident nodes (not implemented)
        /// </summary>
        /// <exception cref="NotImplementedException">Always</exception>
        public void CleanMesh()
        {
            // Fare in modo che chiamando questo metodo i nodi uguali ma che avranno ID diverso vengano tolti dalla collection <see cref="FemModel._nodes"/> 
            // tranne uno, e che i riferimenti ai nodi dentro gli elementi vengano sostituiti con quelli dell'unico nodo rimasto 

            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes a beam element (all the elements equal to it; its nodes are not removed)
        /// </summary>
        /// <param name="finiteElement">The element</param>
        public void RemoveElement(BeamElement finiteElement)
        {
            _beamElements.Remove(finiteElement);
        }

        /// <summary>
        /// Removes an area element (all the elements equal to it; its nodes are not removed)
        /// </summary>
        /// <param name="finiteElement">The element</param>
        public void RemoveElement(AreaElement finiteElement)
        {
            _areaElements.Remove(finiteElement);
        }

        /// <summary>
        /// Removes a volume element (all the elements equal to it; its nodes are not removed)
        /// </summary>
        /// <param name="finiteElement">The element</param>
        public void RemoveElement(VolumeElement finiteElement)
        {
            _volumeElements.Remove(finiteElement);
        }

        /// <summary>
        /// Removes a node (all the nodes equal to it; the elements are not changed)
        /// </summary>
        /// <param name="finiteElement">The node</param>
        public void RemoveElement(NodeElement finiteElement)
        {
            RemoveNodeChecked(finiteElement.Id);
        }

        #endregion

        #region Attribute Checks

        /// <summary>
        /// Tell if a load case has the name
        /// </summary>
        /// <param name="loadCaseName">The name</param>
        /// <returns>True if the name is present</returns>
        public bool LoadCaseExist(string loadCaseName)
        {
            return _loadCases.ContainsKey(loadCaseName);
        }

        /// <summary>
        /// Tell if a freedom case has the name
        /// </summary>
        /// <param name="freedomCaseName">The name</param>
        /// <returns>True if the name is present</returns>
        public bool FreedomCaseExist(string freedomCaseName)
        {
            return _freedomCases.ContainsKey(freedomCaseName);
        }

        /// <summary>
        /// Tell if a group has the name
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns>True if the name is present</returns>
        public bool GroupExist(string name)
        {
            return _groups.ContainsKey(name);
        }

        #endregion

        #region Equals - HashCode - Operators

        /// <summary>
        /// Serializes versioned model data with shared references.
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("ModelSchemaVersion", 1);
            info.AddValue("CheckReports", CheckReports.ToArray());
            if (PhysicalMembers.Count != 0) info.AddValue("PhysicalMembers", PhysicalMembers.Values.ToArray());
            info.AddValue("PreservedSourceData", PreservedSourceData.ToArray());
            info.AddValue("Datasets", Datasets);
            info.AddValue("AnalysisSource", AnalysisSource);
            info.AddValue("Nodes", _nodesElements, typeof(SortedCollection<NodeElement>));
            info.AddValue("Beams", _beamElements, typeof(SortedCollection<BeamElement>));
            info.AddValue("Areas", _areaElements, typeof(SortedCollection<AreaElement>));
            info.AddValue("Volumes", _volumeElements, typeof(SortedCollection<VolumeElement>));
            info.AddValue("Costrains", _costrains, typeof(UniqueIdCollection<Costrain>));

            info.AddValue("BeamProperties", _beamProperties, typeof(UniqueNameCollection<BeamProperty>));
            info.AddValue("PlateProperties", _areaProperties, typeof(UniqueNameCollection<PlateProperty>));
            info.AddValue("BrickProperties", _volumeProperties, typeof(UniqueNameCollection<BrickProperty>));

            info.AddValue("LoadCaseBases", _loadCases, typeof(UniqueNameCollection<LoadCaseBase>));
            info.AddValue("FreedomCases", _freedomCases, typeof(UniqueNameCollection<FreedomCase>));
            info.AddValue("Combinations", _combinations, typeof(UniqueNameCollection<Combination>));

            info.AddValue("StageCombinationsMap", _stageCombinationsMap, typeof(Dictionary<int, HashSet<string>>));
            info.AddValue("Stages", _stages, typeof(UniqueIdCollection<Stage>));
            info.AddValue("Groups", _groups, typeof(UniqueNameCollection<Group>));
            // Written only when present, so that archives of models without them are unchanged.
            if (ModelLoads.Count != 0) info.AddValue("ModelLoads", ModelLoads, typeof(UniqueIdCollection<Load>));
        }

        /// <summary>
        /// The hash code of the collections (as instances) and of the base (it throws <see cref="NullReferenceException"/> if the volume
        /// elements are null, see <see cref="Model(string)"/>)
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -391 + base.GetHashCode();

                hashCode = hashCode * -17 + _nodesElements.GetHashCode();
                hashCode = hashCode * -17 + _beamElements.GetHashCode();
                hashCode = hashCode * -17 + _areaElements.GetHashCode();
                hashCode = hashCode * -17 + _volumeElements.GetHashCode();
                hashCode = hashCode * -17 + _costrains.GetHashCode();
                hashCode = hashCode * -17 + _beamProperties.GetHashCode();
                hashCode = hashCode * -17 + _areaProperties.GetHashCode();
                hashCode = hashCode * -17 + _volumeProperties.GetHashCode();
                hashCode = hashCode * -17 + _loadCases.GetHashCode();
                hashCode = hashCode * -17 + _freedomCases.GetHashCode();
                hashCode = hashCode * -17 + _combinations.GetHashCode();
                hashCode = hashCode * -17 + _stageCombinationsMap.GetHashCode();
                hashCode = hashCode * -17 + _groups.GetHashCode();
                hashCode = hashCode * -17 + _stages.GetHashCode();

                return hashCode;
            }
        }

        /// <summary>
        /// Equality of the collections (as instances) and of the base (it throws <see cref="NullReferenceException"/> if the volume elements
        /// are null, see <see cref="Model(string)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal model</returns>
        public override bool Equals(object obj)
        {
            return obj is Model model &&
                _nodesElements.Equals(model._nodesElements) &&
                _beamElements.Equals(model._beamElements) &&
                _areaElements.Equals(model._areaElements) &&
                _volumeElements.Equals(model._volumeElements) &&
                _costrains.Equals(model._costrains) &&
                _beamProperties.Equals(model._beamProperties) &&
                _areaProperties.Equals(model._areaProperties) &&
                _volumeProperties.Equals(model._volumeProperties) &&
                _loadCases.Equals(model._loadCases) &&
                _freedomCases.Equals(model._freedomCases) &&
                _combinations.Equals(model._combinations) &&
                _stageCombinationsMap.Equals(model._stageCombinationsMap) &&
                _groups.Equals(model._groups) &&
                _stages.Equals(model._stages) &&
                base.Equals(obj);
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first model</param>
        /// <param name="obj2">The second model</param>
        /// <returns>True if the models are equal</returns>
        public static bool operator ==(Model obj1, Model obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first model</param>
        /// <param name="obj2">The second model</param>
        /// <returns>True if the models are different</returns>
        public static bool operator !=(Model obj1, Model obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
