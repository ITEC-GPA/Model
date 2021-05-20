using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Combinations;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Collections;
using GPC.Model.FEM.Costrains;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.FEM.Properties;
using GPC.Model.FreedomCases;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Restrains;
using GPC.Model.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.FEM
{
    [Serializable]
    public class FemModel : ModelObject, ISerializable
    {

        public enum AnalysisTypes
        {
            Linear, 
            NonLinear,
            Modal,
            Buckling, 
            LinearDynamic
        }


        #region Variables

        // ELEMENTI

        /// <summary>
        /// Collection of <see cref="Node"/>
        /// The nodes on this collection does not have duplicate ID and can not be duplicate. (different point with different id)
        /// </summary>
        protected FemObjectCollection<Node> _nodes;

        /// <summary>
        /// Collection of <see cref="FiniteElement"/>
        /// The elements on this collection does not have duplicate ID and can not be duplicate. (different element with different id)
        /// </summary>
        protected FemObjectCollection<FiniteElement> _elements;

        /// <summary>
        /// Collection of <see cref="Costrain"/>
        /// The elements on this collection does not have duplicate ID and can not be duplicate. (different element with different id)
        /// </summary>
        protected FemObjectCollection<Costrain> _costrains;

        // PROPRIETà

        /// <summary>
        /// Collection of <see cref="PlateProperty"/> with unique name 
        /// </summary>
        protected UniqueNameCollection<PlateProperty> _plateProperties;

        /// <summary>
        /// Collection of <see cref="BrickProperty"/> with unique name 
        /// </summary>
        protected UniqueNameCollection<BrickProperty> _brickProperties;

        // LOADCASES

        /// <summary>
        /// Collection of <see cref="LoadCaseBase"/> with unique name 
        /// </summary>
        protected UniqueNameCollection<LoadCaseBase> _loadCases;

        // FREEDOM CASES 

        /// <summary>
        /// Collection of <see cref="FreedomCase"/> with unique name 
        /// </summary>
        protected UniqueNameCollection<FreedomCase> _freedomCases;
        
        // COMBINATION

        /// <summary>
        /// Collection of <see cref="Combination"/> with unique name 
        /// </summary>
        protected UniqueNameCollection<Combination> _combinations;


        /// <summary>
        /// Map between stageId and stage combinations
        /// </summary>
        protected Dictionary<int, HashSet<string>> _stageCombinationsMap;



        // STAGE

        protected UniqueIdCollection<Stage> _stages;

        // MODELATTRIBUTES

        protected List<IModelAttribute> _modelAttributes;


        // CoordinatesSystem ? 


        // RISULTATI

        protected AnalysisTypes _analysisType;

        #endregion

        #region Properties

        public AnalysisTypes AnalysisType { get => _analysisType; set => _analysisType = value; }

        public IEnumerable<Combination> Combinations => _combinations;

        public IEnumerable<LoadCaseBase> LoadCases => _loadCases;


        #endregion

        #region Constructors

        public FemModel()
            : this(string.Empty)
        {
            
        }

        public FemModel(string name) 
            : base(name)
        {
            _nodes = new FemObjectCollection<Node>();
            _elements = new FemObjectCollection<FiniteElement>();
            _costrains = new FemObjectCollection<Costrain>();

            _stages = new UniqueIdCollection<Stage>(); // solo id come equality comparer

            _plateProperties = new UniqueNameCollection<PlateProperty>();
            _brickProperties = new UniqueNameCollection<BrickProperty>();
            
            _loadCases = new UniqueNameCollection<LoadCaseBase>();
            _freedomCases = new UniqueNameCollection<FreedomCase>();
            _combinations = new UniqueNameCollection<Combination>();

            _stageCombinationsMap = new Dictionary<int, HashSet<string>>();

            _analysisType = AnalysisTypes.Linear;

            _modelAttributes = new List<IModelAttribute>();
        }

        
        public FemModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            throw new NotImplementedException();
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
            {
                throw new ArgumentNullException(nameof(elementProperty));
            }


            if (elementProperty is IPlateProperty)
            {
                if (_plateProperties.Contains(elementProperty))
                    return false;

                _plateProperties.Add((PlateProperty)elementProperty);
                return true;
            }
            else if (elementProperty is BrickProperty property)
            {
                if (_brickProperties.Contains(elementProperty))
                    return false;

                _brickProperties.Add(property);
                return true;
            }
            else
            {
                throw new NotSupportedException($"Type: {elementProperty.GetType()} not suppoted");
            }
        }


        /// <inheritdoc cref="UniqueNameCollection{T}.GetElementByName(string)"/>
        public virtual PlateProperty GetPlateProperty(string name)
        {
            return _plateProperties.GetElementByName(name);
        }


        /// <inheritdoc cref="UniqueNameCollection{T}.GetElementByName(string)"/>
        public virtual BrickProperty GetBrickProperty(string name)
        {
            return _brickProperties.GetElementByName(name);
        }

        /// <inheritdoc cref="UniqueNameCollection{T}.GetNames()"/>
        public List<string> GetPlatePropertyNames()
        {
            return _plateProperties.GetNames();
        }

        /// <inheritdoc cref="UniqueNameCollection{T}.GetNames()"/>
        public List<string> GetBrickPropertyNames()
        {
            return _brickProperties.GetNames();
        }

        #endregion


        #region LoadCase / FredomCase

        /// <inheritdoc cref="UniqueNameCollection{T}.Add(T)"/>
        public bool AddLoadCase(LoadCaseBase loadCase)
        {
            return _loadCases.Add(loadCase);
        }


        /// <inheritdoc cref="UniqueNameCollection{T}.GetElementByName(string)"/>
        public LoadCaseBase GetLoadCaseByName(string loadCaseName)
        {
            return _loadCases.GetElementByName(loadCaseName);
        }


        /// <inheritdoc cref="UniqueNameCollection{T}.Add(T)"/>
        public bool AddFreedomCase(FreedomCase fredomCases)
        {
            return _freedomCases.Add(fredomCases);
        }


        /// <inheritdoc cref="UniqueNameCollection{T}.GetElementByName(string)"/>
        public FreedomCase GetFreedomCaseByName(string freedomCaseName)
        {
            return _freedomCases.GetElementByName(freedomCaseName);
        }

        #endregion


        #region Combinations

        public virtual bool AddCombination(Combination combination)
        {
            return _combinations.Add(combination);
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
        #endregion


        #region Stages

        /// <summary>
        /// Add a stage to the stage list. The stage will empty (without elements and nodes)
        /// </summary>
        public virtual Stage AddStage(string name, AnalysisTypes analysisType, bool morph = false)
        {
            Stage stage = new Stage(name, this, analysisType, morph, null);
            _stages.Add(stage);
            return stage;
        }

        /// <summary>
        /// Add a stage the to the stage list. This stage will the clone of stage with <see cref="ModelObjectId.Id"/> equal to <paramref name="stageId"/>"/>
        /// </summary>
        /// <exception cref="ArgumentException">If stage with id equals to <paramref name="stageId"/> does not exist</exception>
        public virtual Stage AddStage(int stageId)
        {
            Stage stage = _stages.GetElementById(stageId);

            var stageCloned = new Stage(stage);
            _stages.Add(new Stage(stage));
            return stageCloned;
        }

        /// <summary>
        /// Add a stage to the femModel, all the elements will be copied into this stage. <see cref="FemModel._combinations"/> will be copied into the stage
        /// </summary>
        public virtual Stage AddStageAsCopyOfModel(string name, AnalysisTypes analysisType)
        {
            Stage stage = new Stage(name, this, analysisType, false, _combinations);

            stage.SetFiniteElements(_elements);

            _stages.Add(stage);

            return stage;
        }

        public virtual Stage GetStageById(int stageId)
        {
            return _stages.GetElementById(stageId);
        }

        public virtual bool ContainsStageId(int stageId)
        {
            return _stages.Contains(stageId);
        }

        public virtual IEnumerable<Combination> GetStageCombinations(int stageId)
        {
            return _stages.GetElementById(stageId).GetCombinations();
        }

        public virtual IEnumerator<KeyValuePair<FiniteElement, Stage.StageFiniteElementProperty>> GetStagePropertyEnumerator(int stageId)
        {
            return _stages.GetElementById(stageId).GetStageFiniteElementPropertiesEnumerator();
        }



        #endregion


        #region ModelAttribute

        /// <summary>Create the a ModelAccelerationAttribute using the loadcase with name equal to <paramref name="loadCaseName"/></summary>
        /// <remarks>Before calling this method, the loadCase must be Added by means of <see cref="FemModel.AddLoadCase(LoadCaseBase)"/></remarks>
        /// <exception cref="ArgumentException"></exception>
        public ModelAccelerationAttribute AddModelAcceleration(string loadCaseName)
        {
            ModelAccelerationAttribute modelAttribute;

            if (LoadCaseExist(loadCaseName))
            {
                modelAttribute = new ModelAccelerationAttribute(loadCaseName);
            }
            else
            {
                throw new ArgumentException();
            }


            _modelAttributes.Add(modelAttribute);

            return modelAttribute;
        } 

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
        public virtual void AddFiniteElement(FiniteElement finiteElement, string propertyName)
        {
            if (finiteElement is null)
                throw new ArgumentNullException(nameof(finiteElement));

            if (string.IsNullOrEmpty(propertyName) || string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));


            ElementProperty property;
            if (finiteElement is Plate)
            {
                property = GetPlateProperty(propertyName);

                if (property is null)
                    throw new ArgumentOutOfRangeException($"The property list does not contain {propertyName}");
            }
            else if (finiteElement is Brick)
            {
                property = GetBrickProperty(propertyName);

                if (property is null)
                    throw new ArgumentOutOfRangeException($"The property list does not contain {propertyName}");
            }
            else
            {
                throw new NotSupportedException(finiteElement.GetType().ToString());
            }

            finiteElement.SetProperty(property);

            AddNodes(finiteElement.Nodes);

            foreach(var attribute in finiteElement.AttributesLoadCase)
            {
                if (!LoadCaseExist(attribute.LoadCaseName))
                    throw new InvalidOperationException($"Loadcase {attribute.LoadCaseName} does not exist in the femModel");
            }

            foreach (var attribute in finiteElement.AttributesFreedomCase)
            {
                if (!FreedomCaseExist(attribute.FreedomCaseName))
                    throw new InvalidOperationException($"Loadcase {attribute.FreedomCaseName} does not exist in the femModel");
            }


            _elements.Add(finiteElement);

        }


        /// <inheritdoc cref="FemModel.AddFiniteElement(FiniteElement, string)"/>
        public virtual void AddFiniteElements(FiniteElement[] finiteElements, string propertyName)
        {
            foreach (var element in finiteElements)
            {
                AddFiniteElement(element, propertyName);
            }
        }


        /// <param name="id"></param>
        /// <returns></returns>
        /// <inheritdoc cref="FemObjectCollection{T}.GetElementById(int)"/>
        public virtual FiniteElement GetFiniteElement(int id)
        {
            return _elements[id];
        }


        public virtual IEnumerator<FiniteElement> GetElementsEnumerator()
        {
            return _elements.GetEnumerator();
        }


        /// <returns>True if <paramref name="finiteElement"/> is contained in the <see cref="FemModel._elements"/> collections </returns>
        /// <inheritdoc cref="FemObjectCollection{T}.Contains(T)"/>
        public virtual bool ContainsFiniteElement(FiniteElement finiteElement)
        {
            return _elements.Contains(finiteElement);
        }


        /// <returns>True if property with name: <paramref name="propertyName"/> is contained in the <see cref="FemModel._plateProperties"/> or <see cref="FemModel._brickProperties"/> collections </returns>
        /// <inheritdoc cref="UniqueNameCollection{T}.Contains(string)"/>
        public virtual bool ContainsProperty(string propertyName)
        {
            return _plateProperties.Contains(propertyName) || _brickProperties.Contains(propertyName);
        }


        #endregion

        #region Nodes


        /// <inheritdoc cref="FemObjectCollection{T}.Add(T)"/>
        protected virtual int AddNode(Node node)
        {
            // non fa la copia, cosi i riferimenti ai nodi dentro agli elementi finiti rimangono 
            return _nodes.Add(node); // l'Add lancia un ArgumentNullException se gli si passa null
        }


        /// <inheritdoc cref="FemObjectCollection{T}.Add(T)"/>
        protected virtual int[] AddNodes(Node[] nodes)
        {
            if (nodes != null)
            {
                int[] indexes = new int[nodes.Length];
                for (int i = 0; i < nodes.Length; i++)
                {
                    indexes[i] = AddNode(nodes[i]);

                    foreach (var attribute in nodes[i].AttributesLoadCase)
                    {
                        if (!LoadCaseExist((attribute as LoadCaseAttribute).LoadCaseName))
                            throw new InvalidOperationException($"Loadcase {(attribute as LoadCaseAttribute).LoadCaseName} does not exist in the femModel");
                    }

                    foreach (var attribute in nodes[i].AttributesFreedomCase)
                    {
                        if (!FreedomCaseExist((attribute as FreedomCaseAttribute).FreedomCaseName))
                            throw new InvalidOperationException($"Loadcase {(attribute as FreedomCaseAttribute).FreedomCaseName} does not exist in the femModel");
                    }
                }

                return indexes;
            }
            throw new ArgumentNullException();
        }


        /// <inheritdoc cref="FemObjectCollection{T}.GetElementById(int)"/>
        public virtual Node GetNode(int id)
        {
            return _nodes.GetElementById(id);
        }


        public virtual IEnumerator<Node> GetNodesEnumerator()
        {
            return _nodes.GetEnumerator();
        }


        #endregion

        #region Costrain

        /// <summary> Add a <paramref name="costrain"/> and its <see cref="Node"/> to the FemModel</summary>
        /// <param name="costrain"></param>
        /// <remarks>This is a O(2n) Operation</remarks>
        /// <inheritdoc cref="AddNode(Node)"/>
        /// <inheritdoc cref="FemObjectCollection{T}.Add(T)"/>
        public virtual int AddCostrain(Costrain costrain)
        {
            if (costrain is null)
                throw new ArgumentNullException(nameof(costrain));


            AddNode(costrain.StartNode);
            AddNodes(costrain.EndNodes);


            return _costrains.Add(costrain);
        }

        /// <summary> Add a <paramref name="costrains"/> and its <see cref="Node"/> to the FemModel</summary>
        /// <param name="costrains"></param>
        /// <remarks>This is a O(2n) Operation</remarks>
        /// <inheritdoc cref="AddNode(Node)"/>
        /// <inheritdoc cref="FemObjectCollection{T}.Add(T)"/>
        public virtual void AddCostrains(IEnumerable<Costrain> costrains)
        {
            foreach(var costrain in costrains)
            {
                AddCostrain(costrain);
            }
        }

        /// <returns>True if <paramref name="costrain"/> is contained in the <see cref="FemModel._costrains"/> collections </returns>
        /// <inheritdoc cref="FemObjectCollection{T}.Contains(T)"/>
        public virtual bool ContainsCostrains(Costrain costrain)
        {
            return _costrains.Contains(costrain);
        }


        /// <param name="index"></param>
        /// <inheritdoc cref="FemObjectCollection{T}.GetElementById(int)"/>
        public virtual Costrain GetCostrain(int index)
        {
            return _costrains[index];
        }


        /// <inheritdoc cref="FemObjectCollection{T}.GetEnumerator()"/>
        public virtual IEnumerator<Costrain> GetCostrainEnumerator()
        {
            return _costrains.GetEnumerator();
        }


        #endregion

        #region Mesh and shapes


        /// <summary>
        /// Generate planar mesh from a shapes. Mesh options need to be setted by <see cref="Mesh.GenerateOptions"/>
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="options"></param>
        /// <param name="platePropertyName"></param>
        /// <param name="loads"></param>
        /// <param name="restrains"></param>
        public virtual void AddShape(Shape shape, string platePropertyName, Mesh.GenerateOptions options, List<Load> loads, List<GeometryRestrain> restrains)
        {

            if (shape is null)
                throw new ArgumentNullException(nameof(shape));


            var embeddedGeometries = new HashSet<GeometryBase>(); // geometrie uniche da passare al meshatore

            // per ogni carico embedda la geometria nella mesh
            if (loads != null)
            {
                foreach (var load in loads)
                {
                    if (load is LineLoad ll)
                        embeddedGeometries.Add(ll.GetGeometry());
                    else if (load is PointLoad pl)
                        embeddedGeometries.Add(pl.GetGeometry());
                    else if (load is AreaLoad || load is NormalAreaLoad)
                        throw new NotImplementedException($"Load type: {load.GetType()} not implemented");
                    else
                        throw new NotSupportedException($"Load type: {load.GetType()} not supported");
                }
            }

            // per ogni vincolo embedda la geometria nella mesh
            if (restrains != null)
            {
                foreach (var restrain in restrains)
                {
                    if (restrain is LineRestrain lr)
                    {
                        embeddedGeometries.Add(lr.GetGeometry());
                    }
                    else if (restrain is PointRestrain pr)
                    {
                        embeddedGeometries.Add(pr.GetGeometry());
                    }
                    else
                        throw new NotSupportedException($"Restrain type: {restrain.GetType()} not supported");
                }
            }

            // Genera la mesh

            bool status = Mesh.Generate(new List<Shape> { shape }, 
                                        new Dictionary<Shape, GeometryBase[]>() { [shape] = embeddedGeometries.ToArray() }, 
                                        options, 
                                        out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);

            if (!status)
            {
                throw generateMeshStatus.GetLastException();
            }

            if (meshes.Count > 1) // Non è possibile ma controlliamo lo stesso
                throw new Exception();


            // Creo associazioni fra carichi e indici elementi
            Dictionary<IPointLoad, int[]> vertexLoadMeshEntityMap = new Dictionary<IPointLoad, int[]>();
            Dictionary<ILineLoad, int[]> vertexLineLoadMeshEntityMap = new Dictionary<ILineLoad, int[]>();
            Dictionary<IAreaLoad, int[]> plateLoadMeshEntityMap = new Dictionary<IAreaLoad, int[]>();
            Dictionary<GeometryRestrain, int[]> restrainMeshEntityMap = new Dictionary<GeometryRestrain, int[]>();

            if (loads != null)
            {
                foreach (var load in loads)
                {
                    if (load is IPointLoad pl)
                    {
                        if (generateMeshStatus.EmbeddedGeometriesVertexMap[meshes.First()].ContainsKey(pl.GetGeometry()))
                            vertexLoadMeshEntityMap[pl] = generateMeshStatus.EmbeddedGeometriesVertexMap[meshes.First()][pl.GetGeometry()];
                    }
                    else if (load is ILineLoad ll)
                    {
                        if (generateMeshStatus.EmbeddedGeometriesVertexMap[meshes.First()].ContainsKey(ll.GetGeometry()))
                            vertexLineLoadMeshEntityMap[ll] = generateMeshStatus.EmbeddedGeometriesVertexMap[meshes.First()][ll.GetGeometry()];
                    }
                    else if (load is IAreaLoad)
                    {
                        throw new NotSupportedException($"Load type: {load.GetType()} not supported");
                    }
                    else
                        throw new NotSupportedException($"Load type: {load.GetType()} not supported");
                }
            }

            if (restrains != null)
            {
                foreach (var restrain in restrains)
                {
                    restrainMeshEntityMap[restrain] = generateMeshStatus.EmbeddedGeometriesVertexMap[meshes.First()][restrain.GetGeometry()];
                }
            }

            AddMesh(meshes.First(), platePropertyName, null, vertexLoadMeshEntityMap, vertexLineLoadMeshEntityMap, null, restrainMeshEntityMap, out _, out _, out _);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="shapes"></param>
        /// <param name="options"></param>
        /// <param name="platePropertyNames"></param>
        /// <param name="loads"></param>
        /// <param name="restrains"></param>
        public virtual void AddShapes(List<Shape> shapes, List<string> platePropertyNames, Mesh.GenerateOptions options, List<List<Load>> loads, List<List<GeometryRestrain>> restrains)
        {
            if (shapes is null)
                throw new ArgumentNullException(nameof(shapes));

            // Garantisce la stessa lunghezza delle liste, ma non che siano liste di non nulli
            if (shapes.Count != platePropertyNames.Count)
                throw new ArgumentException($"Size of {nameof(shapes)} and {nameof(platePropertyNames)} are different");
            if (shapes.Count != loads.Count)
                throw new ArgumentException($"Size of {nameof(shapes)} and {nameof(loads)} are different");
            if (shapes.Count != restrains.Count)
                throw new ArgumentException($"Size of {nameof(shapes)} and {nameof(restrains)} are different");


            for (int i = 0; i < shapes.Count; i++)
            {
                if (shapes[i] is null)
                    throw new ArgumentNullException(nameof(shapes));

                if (!string.IsNullOrEmpty(platePropertyNames[i]) || !string.IsNullOrWhiteSpace(platePropertyNames[i]))
                    throw new ArgumentNullException(nameof(platePropertyNames));

                if (loads[i] is null)
                    throw new ArgumentNullException(nameof(loads));

                if (restrains[i] is null)
                    throw new ArgumentNullException(nameof(restrains));


                AddShape(shapes[i], platePropertyNames[i], options, loads[i], restrains[i]);
            }
        }

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
                                        List<Dictionary<ILineLoad, int[]>> vertexLineLoadMeshEntityMap,
                                        List<Dictionary<IAreaLoad, int[]>> plateLoadMeshEntityMap, List<Dictionary<GeometryRestrain, int[]>> restrainMeshEntityMap,
                                        out List<Dictionary<int, int>> nodesNewIndexMap,
                                        out List<Dictionary<int, int>> platesNewIndexMap,
                                        out List<Dictionary<int, int>> brickNewIndexMap)
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

                status = status && AddMesh(meshes[i], platePropertyNames[i], brickPropertyName[i], vertexLoadMeshEntityMap[i], vertexLineLoadMeshEntityMap[i], plateLoadMeshEntityMap[i], restrainMeshEntityMap[i],
                                            out singleNodesNewIndexMap, out singlePlatesNewIndexMap, out singleBrickNewIndexMap);

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
        /// <exception cref="KeyNotFoundException">If a <see cref="MeshVertex"/>.Id of <paramref name="restrainMeshEntityMap"/> is not found in the <paramref name="mesh"/> vertices ids</exception>
        /// <remarks>The instances of <see cref="LoadCaseBase"/> and <see cref="FreedomCase"/> will be replaced with the one in the <see cref="FemModel._loadCases"/> and <see cref="FemModel._freedomCases"/>  </remarks>
        public virtual bool AddMesh(Mesh mesh, string platePropertyName, string brickPropertyName,
                            Dictionary<IPointLoad, int[]> vertexLoadMeshEntityMap,
                            Dictionary<ILineLoad, int[]> vertexLineLoadMeshEntityMap,
                            Dictionary<IAreaLoad, int[]> plateLoadMeshEntityMap,
                            Dictionary<GeometryRestrain, int[]> restrainMeshEntityMap)
        {
            return AddMesh(mesh, platePropertyName, brickPropertyName, vertexLoadMeshEntityMap, vertexLineLoadMeshEntityMap, plateLoadMeshEntityMap, restrainMeshEntityMap, out _, out _, out _);
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
        /// <exception cref="KeyNotFoundException">If a <see cref="MeshVertex"/>.Id of <paramref name="restrainMeshEntityMap"/> is not found in the <paramref name="mesh"/> vertices ids</exception>
        /// <remarks>The instances of <see cref="LoadCaseBase"/> and <see cref="FreedomCase"/> will be replaced with the one in the <see cref="FemModel._loadCases"/> and <see cref="FemModel._freedomCases"/>  </remarks>
        public virtual bool AddMesh(Mesh mesh, string platePropertyName, string brickPropertyName, 
                            Dictionary<IPointLoad, int[]> vertexLoadMeshEntityMap, 
                            Dictionary<ILineLoad, int[]> vertexLineLoadMeshEntityMap,
                            Dictionary<IAreaLoad, int[]> plateLoadMeshEntityMap, 
                            Dictionary<GeometryRestrain, int[]> restrainMeshEntityMap,
                            out Dictionary<int, int> nodesNewIndexMap,
                            out Dictionary<int, int> platesNewIndexMap,
                            out Dictionary<int, int> brickNewIndexMap)
        {            

            nodesNewIndexMap = new Dictionary<int, int>(); // Mappa tra indici dei nodi dentro _nodes e indici dei vertici della mesh nel caso esistano già dentro _nodes.
            platesNewIndexMap = new Dictionary<int, int>();
            brickNewIndexMap = new Dictionary<int, int>();

            if (mesh is null)
                throw new ArgumentNullException(nameof(mesh));

            IPlateProperty plateProperty = null;
            BrickProperty brickProperty = null;

            // Aggiorno la lista proprietà
            if (mesh.Faces.Count != 0)
            {
                plateProperty = (IPlateProperty)GetPlateProperty(platePropertyName);
            }

            if (mesh.Volumes.Count != 0)
            {
                brickProperty = (BrickProperty)GetBrickProperty(brickPropertyName);
            }

            // Aggiunge nodi alla collection di nodi
            using (var enumerator = mesh.GetVerticesEnumerator())
            {
                for (int i = 0; i < mesh.VerticesCount; i++)
                {
                    enumerator.MoveNext();

                    int nodeIndex = _nodes.Add(new Node(enumerator.Current.Point));

                    nodesNewIndexMap[enumerator.Current.Id] = nodeIndex;
                }
            }

            // Aggiunge elementi FEM
            // Aggiunge Faces
            var faces = mesh.Faces.ToArray();
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                if (plateProperty is IPlateProperty ipp)
                {
                    var face = faces[i];

                    if (face.IsQuad)
                    {
                        var plate = new Plate(new Node[] { _nodes[nodesNewIndexMap[face.A]],
                                                           _nodes[nodesNewIndexMap[face.B]],
                                                           _nodes[nodesNewIndexMap[face.C]],
                                                           _nodes[nodesNewIndexMap[face.D]]}
                                                       );

                        plate.SetProperty((ElementProperty)plateProperty);

                        var plateIndex = _elements.Add(plate);
                        platesNewIndexMap[face.Id] = plateIndex;

                        //elementIndexes.platesId[i] = plateIndex;

                        //if (plateIndex != face.Id) // Se sono diversi vuol dire che esisteva già l'indice element .iD e la collection l'ha modificato
                        //    platesNewIndexMap[face.Id] = plateIndex;
                    }
                    else
                    {
                        var plate = new Plate(new Node[] { _nodes[nodesNewIndexMap[face.A]],
                                                           _nodes[nodesNewIndexMap[face.B]],
                                                           _nodes[nodesNewIndexMap[face.C]]}
                                                       );

                        plate.SetProperty((ElementProperty)plateProperty);

                        var plateIndex = _elements.Add(plate);
                        platesNewIndexMap[face.Id] = plateIndex;

                        //elementIndexes.platesId[i] = plateIndex;

                        //if (plateIndex != face.Id) // Se sono diversi vuol dire che esisteva già l'indice element .iD e la collection l'ha modificato
                        //    platesNewIndexMap[face.Id] = plateIndex;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            // Aggiunge Volumes
            var volumes = mesh.Volumes.ToArray();
            for (int i = 0; i < mesh.Volumes.Count; i++)
            {
                var volume = volumes[i];

                if (volume.IsQuadrangular)
                {
                    if (brickProperty is BrickProperty bp)
                    {

                        var brick = new Brick(new Node[] { _nodes[nodesNewIndexMap[volume.A]],
                                                           _nodes[nodesNewIndexMap[volume.B]],
                                                           _nodes[nodesNewIndexMap[volume.C]],
                                                           _nodes[nodesNewIndexMap[volume.D]],
                                                           _nodes[nodesNewIndexMap[volume.E]],
                                                           _nodes[nodesNewIndexMap[volume.F]],
                                                           _nodes[nodesNewIndexMap[volume.G]],
                                                           _nodes[nodesNewIndexMap[volume.H]]}
                                                       );

                        brick.SetProperty(bp);

                        var brickIndex = _elements.Add(brick);
                        brickNewIndexMap[volume.Id] = brickIndex;
                        //elementIndexes.volumesId[i] = brickIndex;

                        //if (brickIndex != volume.Id) // Se sono diversi vuol dire che esisteva già l'indice element .iD e la collection l'ha modificato
                        //    brickNewIndexMap[volume.Id] = brickIndex;
                    }
                    else
                        throw new NotImplementedException();
                }
                else
                {
                    if (brickProperty is BrickProperty bp)
                    {
                        var brick = new Brick(new Node[] { _nodes[nodesNewIndexMap[volume.A]],
                                                           _nodes[nodesNewIndexMap[volume.B]],
                                                           _nodes[nodesNewIndexMap[volume.C]],
                                                           _nodes[nodesNewIndexMap[volume.D]],
                                                           _nodes[nodesNewIndexMap[volume.E]],
                                                           _nodes[nodesNewIndexMap[volume.F]]}
                                                       );

                        brick.SetProperty(bp);

                        var brickIndex = _elements.Add(brick);
                        brickNewIndexMap[volume.Id] = brickIndex;

                        //elementIndexes.volumesId[i] = brickIndex;

                        //if (brickIndex != volume.Id) // Se sono diversi vuol dire che esisteva già l'indice element .iD e la collection l'ha modificato
                        //    brickNewIndexMap[volume.Id] = brickIndex;
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

                    Dictionary<LinearSolver.DOF, bool> restrains = geometryRestrain.GetRestrains();
                    Dictionary<LinearSolver.DOF, double> stiffneses = geometryRestrain.GetStiffnesses();
                    Dictionary<LinearSolver.DOF, double> displacements = geometryRestrain.GetImposedDisplacement();


                    FreedomCase freedomCase;
                    if (FreedomCaseExist(geometryRestrain.FreedomCase.Name))
                    {
                        freedomCase = GetFreedomCaseByName(geometryRestrain.FreedomCase.Name);
                        if (!freedomCase.Equals(geometryRestrain.FreedomCase))
                            throw new ArgumentException($"FreedomCase {geometryRestrain.FreedomCase.Name} is not equal to the one inside the FemModel");
                    }
                    else
                    {
                        if (AddFreedomCase(geometryRestrain.FreedomCase))
                            freedomCase = geometryRestrain.FreedomCase;
                        else
                            throw new InvalidOperationException();
                    }


                    NodeRestrainAttribute nra = new NodeRestrainAttribute(freedomCase.Name, geometryRestrain.CoordinateSystem);
                    NodeStiffnessAttribute nsa = new NodeStiffnessAttribute(freedomCase.Name, geometryRestrain.CoordinateSystem);

                    // TODO:  gestire il fatto che uno spostamento imposto può essere applicato in un grado di libertà vincolato
                    foreach (var restrain in restrains)
                    {
                        if (restrain.Value)
                            nra.AddExternalRestrain(restrain.Key);
                    }

                    foreach (var displacement in displacements)
                    {
                        nra.AddImposedDisplacement(displacement.Key, displacement.Value);
                    }

                    foreach (var stiffness in stiffneses)
                    {
                        nsa.AddStiffness(stiffness.Key, stiffness.Value);
                    }


                    foreach (var index in indexes)
                    {
                        int nodeId = nodesNewIndexMap.ContainsKey(index) ? nodesNewIndexMap[index] : index;

                        Node node = _nodes.GetElementById(nodeId); // se non trova l'indice viene lanciata una keynotfoundException

                        if (nra.Restrains.Count > 0)
                            node.AddAttribute(nra);

                        if (nsa.Stiffnesses.Count > 0)
                            node.AddAttribute(nsa);

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



                    foreach (var index in indexes)
                    {
                        int nodeId = nodesNewIndexMap.ContainsKey(index) ? nodesNewIndexMap[index] : index;

                        Node node = _nodes.GetElementById(nodeId); // se non trova l'indice viene lanciata una keynotfoundException

                        if (load is PointLoad pl)
                        {
                            NodeForceAttribute nfa = new NodeForceAttribute(loadCase.Name, pl.CoordinateSystem, pl.F1, pl.F2, pl.F3, pl.M1, pl.M2, pl.M3);
                            node.AddAttribute(nfa);
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


                    foreach (var index in indexes)
                    {
                        int nodeId = nodesNewIndexMap.ContainsKey(index) ? nodesNewIndexMap[index] : index;

                        Node node = _nodes.GetElementById(nodeId); // se non trova l'indice viene lanciata una keynotfoundException

                        if (load is LineLoad ll)
                        {
                            // carico è F/L (FL/L nel caso di momento)
                            // carico su nodo intermedio: F/L / (nnodi - 1)
                            // carico su nodo estremità: F/L / (nnodi - 1) / 2.0
                            // se per qualche motivo l'equals start/end non funziona, viene applicato più carico

                            var line = ll.GetGeometry();
                            var factor = lineLenght / (indexes.Count() - 1);

                            if (node.Equals(line.Start) || node.Equals(line.End))
                            {
                                factor = factor / 2.0;
                            }

                            NodeForceAttribute nfa = new NodeForceAttribute(ll.LoadCase.Name, ll.CoordinateSystem, 
                                                                            ll.F1 * factor, ll.F2 * factor, ll.F3 * factor, ll.M1 * factor, ll.M2 * factor, ll.M3 * factor);

                            node.AddAttribute(nfa);

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

                    foreach (var index in indexes)
                    {
                        int plateId = platesNewIndexMap.ContainsKey(index) ? platesNewIndexMap[index] : index;

                        FiniteElement finiteElement = _elements.GetElementById(plateId); // se non trova l'indice viene lanciata una keynotfoundException

                        
                        if (!(finiteElement is Plate plate))
                            throw new ArgumentException($"Element with id: {plateId} {index} is not a plate");

                        if (load is NormalAreaLoad pl)
                        {
                            PlateNormalPressureAttribute pna = new PlateNormalPressureAttribute(pl.LoadCase.Name, pl.Pressure);
                            plate.AddLoadCaseAttribute(pna);
                        }
                        else if (load is AreaLoad gal)
                        {
                            PlatePressureAttribute ppa = new PlatePressureAttribute(gal.LoadCase.Name, gal.CoordinateSystem, gal.P1, gal.P2, gal.P3);
                            plate.AddLoadCaseAttribute(ppa);
                        }
                        else
                            throw new NotImplementedException();

                    }
                }

            }


            return true;
        }

        

        public virtual Mesh GetMesh()
        {
            Mesh mesh = new Mesh();

            foreach (var element in _elements)
            {
                if (element is Plate p)
                {
                    mesh.AddFaceMesh(p.Nodes.Select(i => i.Position).ToArray());
                }
                else if (element is Brick b)
                {
                    mesh.AddFaceMesh(b.Nodes.Select(i => i.Position).ToArray());
                }
            }

            return mesh;
        }


        #endregion

        #endregion

        #region Edits

        [Obsolete("This method has not been implemented yet", false)]
        public void CleanMesh()
        {
            // Fare in modo che chiamando questo metodo i nodi uguali ma che avranno ID diverso vengano tolti dalla collection <see cref="FemModel._nodes"/> 
            // tranne uno, e che i riferimenti ai nodi dentro gli elementi vengano sostituiti con quelli dell'unico nodo rimasto 

            throw new NotImplementedException();
        }

        /// <inheritdoc cref="FemObjectCollection{T}.Remove(T)"/>
        public void RemoveElement(FiniteElement finiteElement)
        {
            _elements.Remove(finiteElement);
        }

        /// <inheritdoc cref="FemObjectCollection{T}.Remove(int)"/>
        public void RemoveElement(int id)
        {
            _elements.Remove(id);
        }

        #endregion

        #region Attribute Checks

        public bool LoadCaseExist(string loadCaseName)
        {
            return _loadCases.Contains(loadCaseName);
        }

        public bool FreedomCaseExist(string freedomCaseName)
        {
            return _freedomCases.Contains(freedomCaseName);
        }


        #endregion

        #region Solve

        public virtual void Solve()
        {
            throw new NotImplementedException();
        }


        public virtual void SolveStaged()
        {
            throw new NotImplementedException();
        }


        #endregion

        #region Results

        /// <returns>The results related to <paramref name="combination"/></returns>
        public IEnumerable<NodeResult> GetCombinationNodeDisplacementResults(Combination combination)
        {
            return _nodes.SelectMany(i => i.Results.Where(j => j.Case.Equals(combination) && j.Result is ResultDisplacement));
        }


        /// <returns>The results related to <paramref name="combination"/></returns>
        public IEnumerable<FiniteElementResult> GetCombinationElementStressResults(Combination combination)
        {
            return _elements.SelectMany(i => i.Results.Where(k => k.Case.Equals(combination) && k.Results.Where(m => m != null).First() is ResultStress));
        }


        #endregion

        #endregion



        #region Equals - HashCode - Operators


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            throw new NotImplementedException();
        }


        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
            return obj is FemModel model &&
                   base.Equals(obj);
        }


        public override int GetHashCode()
        {
            int hashCode = -23;
            throw new NotImplementedException();
            return hashCode;
        }


        public static bool operator ==(FemModel obj1, FemModel obj2)
        {
            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(FemModel obj1, FemModel obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion

    }
}
