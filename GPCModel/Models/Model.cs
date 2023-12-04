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
    [Serializable]
    public class Model : ModelObject, ISerializable
    {
        #region Variables

        protected SortedCollection<NodeElement> _nodesElements;
        protected SortedCollection<BeamElement> _beamElements;
        protected SortedCollection<AreaElement> _areaElements;
        protected SortedCollection<VolumeElement> _volumeElements;
        protected UniqueIdCollection<Costrain> _costrains;

        protected UniqueNameCollection<BeamProperty> _beamProperties;
        protected UniqueNameCollection<PlateProperty> _areaProperties;
        protected UniqueNameCollection<BrickProperty> _volumeProperties;

        protected UniqueNameCollection<LoadCaseBase> _loadCases;
        protected UniqueNameCollection<FreedomCase> _freedomCases;
        protected UniqueNameCollection<Combination> _combinations;
        protected Dictionary<int, HashSet<string>> _stageCombinationsMap;

        protected UniqueNameCollection<Group> _groups;
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

        #endregion

        #region Constructors

        public Model(string name = "")
            : base(name)
        {
            _beamProperties = new UniqueNameCollection<BeamProperty>();
            _areaProperties = new UniqueNameCollection<PlateProperty>();
            _volumeProperties = new UniqueNameCollection<BrickProperty>();

            _groups = new UniqueNameCollection<Group>();
            _costrains = new UniqueIdCollection<Costrain>();

            _nodesElements = new SortedCollection<NodeElement>();
            _beamElements = new SortedCollection<BeamElement>();
            _areaElements = new SortedCollection<AreaElement>();

            _stages = new UniqueIdCollection<Stage>(); // solo id come equality comparer

            _loadCases = new UniqueNameCollection<LoadCaseBase>();
            _freedomCases = new UniqueNameCollection<FreedomCase>();
            _combinations = new UniqueNameCollection<Combination>();

            _stageCombinationsMap = new Dictionary<int, HashSet<string>>();
        }

        protected Model(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _nodesElements = (SortedCollection<NodeElement>)info.GetValue("Nodes", typeof(SortedCollection<NodeElement>));
            _beamElements = (SortedCollection<BeamElement>)info.GetValue("Beams", typeof(SortedCollection<BeamElement>));
            _areaElements = (SortedCollection<AreaElement>)info.GetValue("Areas", typeof(SortedCollection<AreaElement>));
            _volumeElements = (SortedCollection<VolumeElement>)info.GetValue("Volumes", typeof(SortedCollection<VolumeElement>));
            _costrains = (UniqueIdCollection<Costrain>)info.GetValue("Costrains", typeof(UniqueIdCollection<Costrain>));

            _beamProperties = (UniqueNameCollection<BeamProperty>)info.GetValue("BeamProperties", typeof(UniqueNameCollection<BeamProperty>));
            _areaProperties = (UniqueNameCollection<PlateProperty>)info.GetValue("PlateProperties", typeof(UniqueNameCollection<PlateProperty>));
            _volumeProperties = (UniqueNameCollection<BrickProperty>)info.GetValue("BrickProperties", typeof(UniqueNameCollection<BrickProperty>));

            _loadCases = (UniqueNameCollection<LoadCaseBase>)info.GetValue("LoadCaseBases", typeof(UniqueNameCollection<LoadCaseBase>));
            _freedomCases = (UniqueNameCollection<FreedomCase>)info.GetValue("FreedomCases", typeof(UniqueNameCollection<FreedomCase>));
            _combinations = (UniqueNameCollection<Combination>)info.GetValue("Combinations", typeof(UniqueNameCollection<Combination>));

            _stageCombinationsMap = (Dictionary<int, HashSet<string>>)info.GetValue("StageCombinationsMap", typeof(Dictionary<int, HashSet<string>>));
            _groups = (UniqueNameCollection<Group>)info.GetValue("Groups", typeof(UniqueNameCollection<Group>));
            _stages = (UniqueIdCollection<Stage>)info.GetValue("Stages", typeof(UniqueIdCollection<Stage>));
        }

        #endregion

        #region Methods

        #region Add Get Attributes

        #region Element Properties

        /// <returns><see langword="true"/> if the property has been added. 
        /// <para><see langword="false"/> if a property with the same name is already present</para> 
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

        public virtual PlateProperty GetPlateProperty(string name)
        {
            return _areaProperties.GetElementByName(name);
        }

        public virtual BeamProperty GetBeamProperty(string name)
        {
            return _beamProperties.GetElementByName(name);
        }

        public virtual BrickProperty GetBrickProperty(string name)
        {
            return _volumeProperties.GetElementByName(name);
        }

        public List<string> GetBeamPropertyNames()
        {
            return _beamProperties.GetNames();
        }

        public List<string> GetPlatePropertyNames()
        {
            return _areaProperties.GetNames();
        }

        public List<string> GetBrickPropertyNames()
        {
            return _volumeProperties.GetNames();
        }

        #endregion

        #region LoadCase / FredomCase

        /// <inheritdoc cref="UniqueNameCollection{T}.Add(T)"/>
        public bool AddLoadCase(LoadCaseBase loadCase)
        {
            _loadCases.Add(loadCase.Name, loadCase);
            return true;
        }

        public bool AddLoadCases(IEnumerable<LoadCaseBase> loadCaseBases)
        {
            return _loadCases.AddRange(loadCaseBases);
        }

        /// <inheritdoc cref="UniqueNameCollection{T}.GetElementByName(string)"/>
        public LoadCaseBase GetLoadCaseByName(string loadCaseName)
        {
            return _loadCases.GetElementByName(loadCaseName);
        }

        public bool AddFreedomCase(FreedomCase fredomCases)
        {
            _freedomCases.Add(fredomCases.Name, fredomCases);
            return true;
        }

        public FreedomCase GetFreedomCaseByName(string freedomCaseName)
        {
            return _freedomCases.GetElementByName(freedomCaseName);
        }

        public LoadCaseBase[] GetLoadCases()
        {
            return _loadCases.Values.ToArray();
        }

        public string[] GetLoadCaseNames()
        {
            return _loadCases.GetNames().ToArray();
        }

        public FreedomCase[] GetFreedomCases()
        {
            return _freedomCases.Values.ToArray();
        }

        public string[] GetFreedomCaseNames()
        {
            return _freedomCases.GetNames().ToArray();
        }

        #endregion

        #region Combinations

        public virtual bool AddCombination(Combination combination)
        {
            _combinations.Add(combination.Name, combination);
            return true;
        }

        public virtual bool AddCombinations(IEnumerable<Combination> combinations)
        {
            return _combinations.AddRange(combinations);
        }

        internal bool AddStageCombinationMap(int stageId, string combinationName)
        {
            if (!_stageCombinationsMap.ContainsKey(stageId))
                _stageCombinationsMap[stageId] = new HashSet<string>();

            return _stageCombinationsMap[stageId].Add(combinationName);
        }

        internal bool RemoveStageCombinationMap(int stageId, string combinationName)
        {
            return _stageCombinationsMap[stageId].Remove(combinationName);
        }

        public Combination[] GetCombinations()
        {
            return _combinations.Values.ToArray();
        }

        #endregion

        #region Groups

        public Group AddGroup(string name, Group partent = null)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrEmpty(name))
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));

            Group group = new Group(name, partent);
            _groups.Add(group.Name, group);
            return group;
        }

        public bool SetGroup(IEnumerable<Element> elements, string groupName)
        {
            if (elements is null)
                throw new ArgumentNullException(nameof(elements));

            if (string.IsNullOrEmpty(groupName) || string.IsNullOrEmpty(groupName))
                throw new ArgumentException($"'{nameof(groupName)}' cannot be null or empty.", nameof(groupName));

            var group = _groups.GetElementByName(groupName);

            if (group == null)
                return false;

            foreach (Element element in elements)
            {
                if (element is null)
                    return false;

                if (!element.AddGroup(group))
                    return false;
            }

            return true;
        }

        public bool SetGroupRange(IEnumerable<Element> elements, IEnumerable<string> groupNames)
        {
            if (elements is null)
                throw new ArgumentNullException(nameof(elements));

            if (groupNames is null)
                throw new ArgumentNullException(nameof(groupNames));

            foreach (var name in groupNames)
            {
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(name))
                    throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

                Group group = _groups[name];

                foreach (var element in elements)
                {
                    if (element is null)
                        return false;

                    if (!element.AddGroup(group))
                        return false;
                }
            }

            return true;
        }

        public Group[] GetGroups()
        {
            return _groups.Values.ToArray();
        }


        #endregion

        #region Stages

        /// <summary>
        /// Add a stage the to the stage list. This stage will the clone of stage with <see cref="ModelObjectId.Id"/> equal to <paramref name="stageId"/>"/>
        /// </summary>
        /// <exception cref="ArgumentException">If stage with id equals to <paramref name="stageId"/> does not exist</exception>
        //public virtual Stage AddStage(int stageId)
        //{
        //    Stage stage = _stages.GetById(stageId);

        //    var stageCloned = new Stage(stage);
        //    _stages.Add(new Stage(stage));
        //    return stageCloned;
        //}

        public Stage[] GetStages()
        {
            return _stages.Values.ToArray();
        }

        public Stage GetStageById(int stageId)
        {
            return _stages.GetById(stageId);
        }

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

        /// <summary> Add a <paramref name="finiteElement"/> and its <see cref="Node"/> to the FemModel</summary>
        /// <param name="finiteElement"></param>
        /// <param name="propertyName">The name of the property that will be assigned to the <paramref name="finiteElement"/></param>
        /// <remarks>This is a O(2n) Operation</remarks>
        /// <inheritdoc cref="GetPlateProperty(string)"/>
        /// <exception cref="ArgumentNullException">If the property list does not contain a property with a name equal to <paramref name="propertyName"/></exception>
        /// <exception cref="ArgumentNullException">If the nodes inside the <paramref name="finiteElement"/> are null</exception>
        public virtual void AddFiniteElement(Element finiteElement)
        {
            if (finiteElement is null)
                throw new ArgumentNullException(nameof(finiteElement));

            if (finiteElement is BeamElement beamElement)
            {
                _nodesElements.Add(new NodeElement(beamElement.StartPoint));
                _nodesElements.Add(new NodeElement(beamElement.EndPoint));
                _beamElements.Add(beamElement);
                _beamProperties.Add(beamElement.Name, beamElement.BeamProperty);
            }
            else if (finiteElement is AreaElement areaElement)
            {
                for (int i = 0; i < areaElement.Points.Length; i++)
                    _nodesElements.Add(new NodeElement(areaElement.Points[i]));
                _areaElements.Add(areaElement);
                _areaProperties.Add(areaElement.Name, areaElement.PlateProperty);
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

        public virtual void AddFiniteElements(Element[] finiteElements)
        {
            foreach (var element in finiteElements)
            {
                AddFiniteElement(element);
            }
        }

        public BeamElement GetBeamElement(int id)
        {
            return _beamElements[id];
        }

        public NodeElement GetNodeElement(int id)
        {
            return _nodesElements[id];
        }

        public AreaElement GetAreaElement(int id)
        {
            return _areaElements[id];
        }

        public virtual bool ContainsBeamProperty(string propertyName)
        {
            return _beamProperties.ContainsKey(propertyName);
        }

        public virtual bool ContainsAreaProperty(string propertyName)
        {
            return _areaProperties.ContainsKey(propertyName);
        }

        public virtual bool ContainsVolumeProperty(string propertyName)
        {
            return _volumeProperties.ContainsKey(propertyName);
        }

        #endregion

        #region Nodes

        protected virtual int AddNode(NodeElement node)
        {
            foreach (var attribute in node.Attributes)
            {
                if (!LoadCaseExist(attribute.Value.Name))
                    throw new InvalidOperationException($"Loadcase {attribute.Value.Name} does not exist in the femModel");
            }

            return _nodesElements.Add(node); // l'Add lancia un ArgumentNullException se gli si passa null
        }

        /// <inheritdoc cref="AddNode(Node)"/>
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

        /// <inheritdoc cref="SortedCollection{T}.GetById(int)"/>
        public virtual NodeElement GetNode(int id)
        {
            return _nodesElements[id];
        }

        public virtual IEnumerator<NodeElement> GetNodesEnumerator()
        {
            return _nodesElements.Values.GetEnumerator();
        }

        public NodeElement[] GetNodes()
        {
            return _nodesElements.Values.ToArray();
        }


        #endregion

        #region Costrain

        /// <summary> Add a <paramref name="costrain"/> and its <see cref="Node"/> to the FemModel</summary>
        /// <param name="costrain"></param>
        /// <remarks>This is a O(2n) Operation</remarks>
        /// <inheritdoc cref="AddNode(Node)"/>
        /// <inheritdoc cref="FemObjectCollection{T}.AddUnique(T)"/>
        public virtual int AddCostrain(Costrain costrain)
        {
            if (costrain is null)
                throw new ArgumentNullException(nameof(costrain));

            AddNode(costrain.StartNode);
            AddNodes(costrain.EndNodes);

            _costrains.Add(costrain);
            return costrain.Id;
        }

        /// <summary> Add a <paramref name="costrains"/> and its <see cref="Node"/> to the FemModel</summary>
        /// <param name="costrains"></param>
        /// <remarks>This is a O(2n) Operation</remarks>
        /// <inheritdoc cref="AddNode(Node)"/>
        /// <inheritdoc cref="FemObjectCollection{T}.AddUnique(T)"/>
        public virtual void AddCostrains(IEnumerable<Costrain> costrains)
        {
            foreach (var costrain in costrains)
            {
                AddCostrain(costrain);
            }
        }

        /// <returns>True if <paramref name="costrain"/> is contained in the <see cref="Model._costrains"/> collections </returns>
        /// <inheritdoc cref="FemObjectCollection{T}.Contains(T)"/>
        public virtual bool ContainsCostrains(Costrain costrain)
        {
            return _costrains.ContainsKey(costrain.Id);
        }


        /// <param name="index"></param>
        /// <inheritdoc cref="FemObjectCollection{T}.GetElementById(int)"/>
        public virtual Costrain GetCostrain(int index)
        {
            return _costrains[index];
        }

        public virtual Costrain[] GetCostrains()
        {
            return _costrains.Values.ToArray();
        }


        /// <inheritdoc cref="FemObjectCollection{T}.GetEnumerator()"/>
        public virtual IEnumerator<Costrain> GetCostrainEnumerator()
        {
            return _costrains.Values.GetEnumerator();
        }


        #endregion

        #region Mesh

        /// <summary>
        /// 
        /// </summary>
        /// <param name="meshes"></param>
        /// <param name="platePropertyNames"></param>
        /// <param name="brickPropertyName"></param>
        /// <param name="vertexLoadMeshEntityMap"></param>
        /// <param name="vertexLineLoadMeshEntityMap"></param>
        /// <param name="plateLoadMeshEntityMap"></param>
        /// <param name="restrainMeshEntityMap"></param>
        /// <param name="nodesNewIndexMap">A map between the <see cref="MeshVertex"/>.Id of <paramref name="meshes"/> and the id of the same nodes in the femModel</param>
        /// <param name="platesNewIndexMap">A map between the <see cref="MeshFace"/>.Id of <paramref name="meshes"/> and the id of the same plate in the femModel</param>
        /// <param name="brickNewIndexMap">A map between the <see cref="MeshVolume"/>.Id of <paramref name="meshes"/> and the id of the same brick in the femModel</param>
        /// <exception cref="ArgumentException">If list of argument does not match</exception>
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
        /// Add a mesh to the Fem model
        /// </summary>
        /// <param name="mesh"></param> 
        /// <param name="platePropertyName"></param>
        /// <param name="brickPropertyName"></param>
        /// <param name="vertexLoadMeshEntityMap">Map between <see cref="IPointLoad"/> and <see cref="MeshVertex"/>.Id</param>
        /// <param name="vertexLineLoadMeshEntityMap">Map between <see cref="ILineLoad"/> and <see cref="MeshVertex"/>.Id</param>
        /// <param name="plateLoadMeshEntityMap">Map between <see cref="IAreaLoad"/> and <see cref="MeshFace"/>.Id</param>
        /// <param name="restrainMeshEntityMap">Map between IGeometryRestrain and <see cref="MeshVertex"/>.Id</param>
        /// <param name="groupName"></param>
        /// <exception cref="KeyNotFoundException">If a <see cref="MeshVertex"/>.Id of <paramref name="restrainMeshEntityMap"/> is not found in the <paramref name="mesh"/> vertices ids</exception>
        /// <remarks>The instances of <see cref="LoadCaseBase"/> and <see cref="FreedomCase"/> will be replaced with the one in the <see cref="Model._loadCases"/> and <see cref="Model._freedomCases"/>  </remarks>
        public virtual bool AddMesh(Mesh mesh, string platePropertyName, string brickPropertyName, Dictionary<IPointLoad, int[]> vertexLoadMeshEntityMap,
            Dictionary<ILineLoad, int[]> vertexLineLoadMeshEntityMap, Dictionary<IAreaLoad, int[]> plateLoadMeshEntityMap, Dictionary<GeometryRestrain, int[]> restrainMeshEntityMap, string groupName = "")
        {
            return AddMesh(mesh, platePropertyName, brickPropertyName, vertexLoadMeshEntityMap, vertexLineLoadMeshEntityMap, plateLoadMeshEntityMap,
                restrainMeshEntityMap, out _, out _, out _, groupName);
        }

        /// <summary>
        /// Add a mesh to the Fem model
        /// </summary>
        /// <param name="mesh"></param> 
        /// <param name="platePropertyName"></param>
        /// <param name="brickPropertyName"></param>
        /// <param name="vertexLoadMeshEntityMap">Map between <see cref="IPointLoad"/> and <see cref="MeshVertex"/>.Id</param>
        /// <param name="vertexLineLoadMeshEntityMap">Map between <see cref="ILineLoad"/> and <see cref="MeshVertex"/>.Id</param>
        /// <param name="plateLoadMeshEntityMap">Map between <see cref="IAreaLoad"/> and <see cref="MeshFace"/>.Id</param>
        /// <param name="restrainMeshEntityMap">Map between IGeometryRestrain and <see cref="MeshVertex"/>.Id</param>
        /// <param name="nodesNewIndexMap">A map between the <see cref="MeshVertex"/>.Id of <paramref name="mesh"/> and the id of the same nodes in the femModel</param>
        /// <param name="platesNewIndexMap">A map between the <see cref="MeshFace"/>.Id of <paramref name="mesh"/> and the id of the same plate in the femModel</param>
        /// <param name="brickNewIndexMap">A map between the <see cref="MeshVolume"/>.Id of <paramref name="mesh"/> and the id of the same brick in the femModel</param>
        /// <param name="groupName"></param>
        /// <exception cref="KeyNotFoundException">If a <see cref="MeshVertex"/>.Id of <paramref name="restrainMeshEntityMap"/> is not found in the <paramref name="mesh"/> vertices ids</exception>
        /// <remarks>The instances of <see cref="LoadCaseBase"/> and <see cref="FreedomCase"/> will be replaced with the one in the <see cref="Model._loadCases"/> and <see cref="Model._freedomCases"/>  </remarks>
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

        /// <returns>The results related to <paramref name="combination"/></returns>
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

        public ResultLocation[] GetCombinationAreaResults(Combination combination, Group group = null)
        {
            List<ResultLocation> results = new List<ResultLocation>();
            if (group != null)
            {
                for (int i = 0; i < _areaElements.Count; i++)
                {
                    AreaElement element = _areaElements[i];

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
                for (int i = 0; i < _areaElements.Count; i++)
                {
                    AreaElement element = _areaElements[i];

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

        public void CleanMesh()
        {
            // Fare in modo che chiamando questo metodo i nodi uguali ma che avranno ID diverso vengano tolti dalla collection <see cref="FemModel._nodes"/> 
            // tranne uno, e che i riferimenti ai nodi dentro gli elementi vengano sostituiti con quelli dell'unico nodo rimasto 

            throw new NotImplementedException();
        }

        public void RemoveElement(BeamElement finiteElement)
        {
            _beamElements.Remove(finiteElement);
        }

        public void RemoveElement(AreaElement finiteElement)
        {
            _areaElements.Remove(finiteElement);
        }

        public void RemoveElement(VolumeElement finiteElement)
        {
            _volumeElements.Remove(finiteElement);
        }

        public void RemoveElement(NodeElement finiteElement)
        {
            _nodesElements.Remove(finiteElement);
        }

        #endregion

        #region Attribute Checks

        public bool LoadCaseExist(string loadCaseName)
        {
            return _loadCases.ContainsKey(loadCaseName);
        }

        public bool FreedomCaseExist(string freedomCaseName)
        {
            return _freedomCases.ContainsKey(freedomCaseName);
        }

        public bool GroupExist(string name)
        {
            return _groups.ContainsKey(name);
        }

        #endregion

        #region Equals - HashCode - Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("Nodes", _nodesElements, typeof(SortedCollection<NodeElement>));
            info.AddValue("Elements", _beamElements, typeof(SortedCollection<BeamElement>));
            info.AddValue("Areas", _areaElements, typeof(SortedCollection<AreaElement>));
            info.AddValue("Volumes", _volumeElements, typeof(SortedCollection<VolumeElement>));
            info.AddValue("Costrains", _costrains, typeof(UniqueIdCollection<Costrain>));

            info.AddValue("PlateProperties", _beamProperties, typeof(UniqueNameCollection<BeamProperty>));
            info.AddValue("PlateProperties", _areaProperties, typeof(UniqueNameCollection<PlateProperty>));
            info.AddValue("BrickProperties", _volumeProperties, typeof(UniqueNameCollection<BrickProperty>));

            info.AddValue("LoadCaseBases", _loadCases, typeof(UniqueNameCollection<LoadCaseBase>));
            info.AddValue("FreedomCases", _freedomCases, typeof(UniqueNameCollection<FreedomCase>));
            info.AddValue("Combinations", _combinations, typeof(UniqueNameCollection<Combination>));

            info.AddValue("StageCombinationsMap", _stageCombinationsMap, typeof(Dictionary<int, HashSet<string>>));
            info.AddValue("Stages", _stages, typeof(UniqueIdCollection<Stage>));
            info.AddValue("Stages", _groups, typeof(UniqueNameCollection<Group>));
        }

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

        public static bool operator !=(Model obj1, Model obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
