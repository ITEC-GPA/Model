using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Elements;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Properties;
using GPC.Model.FEM.Collections;
using GPC.Model.FreedomCases;
using GPC.Model.LoadCases;
using GPC.Model.Restrains;
using GPC.Model.Loads;
using GPC.Model.Results;
using GPC.Model.Combinations;

namespace GPC.Model.FEM
{
    [Serializable]
    public class FemModel : ModelObject, ISerializable
    {

        public enum AnalysisType
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
        /// The nodes on this collection does not have duplicate ID but they can be duplicate (same point)
        /// </summary>
        protected FemObjectCollection<Node> _nodes;

        /// <summary>
        /// Collection of <see cref="FiniteElement"/>
        /// The element on this collection does not have duplicate ID but they can be duplicate (same point)
        /// </summary>
        protected FemObjectCollection<FiniteElement> _elements;

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
        /// Collection of <see cref="LoadCase"/> with unique name 
        /// </summary>
        protected UniqueNameCollection<LoadCase> _loadCases;

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

        // STAGE

        protected List<Stage> _stages;


        // CoordinatesSystem ? 


        // RISULTATI
        protected List<ResultNodeDisplacement> _resultNodeDisplacements;

        protected List<ResultNodeForce> _resultNodeForce;

        protected List<ResultPlateStress> _resultPlateStress;

        #endregion

        #region PROPERTIES

        public List<Stage> Stages => _stages;

        public List<ResultPlateStress> ResultPlateStresses => _resultPlateStress;
        public List<ResultNodeDisplacement> ResultNodeDisplacement => _resultNodeDisplacements;
        public List<ResultNodeForce> ResultNodeForce => _resultNodeForce;
        

        #endregion

        #region Constructors

        public FemModel()
            : this(string.Empty)
        {
            
        }

        public FemModel(string name) 
            : base(Guid.NewGuid(), name)
        {
            _nodes = new FemObjectCollection<Node>();
            _elements = new FemObjectCollection<FiniteElement>();
            _stages = new List<Stage>();

            _plateProperties = new UniqueNameCollection<PlateProperty>();
            _brickProperties = new UniqueNameCollection<BrickProperty>();
            
            _loadCases = new UniqueNameCollection<LoadCase>();
            _freedomCases = new UniqueNameCollection<FreedomCase>();
            _combinations = new UniqueNameCollection<Combination>();
                        
            _resultPlateStress = new List<ResultPlateStress>();
            _resultNodeForce = new List<ResultNodeForce>();
            _resultNodeDisplacements = new List<ResultNodeDisplacement>();


            //_stages.Add(new Stage("Stage 0", AnalysisType.Linear));
        }

        
        public FemModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Public methods


        #region Add Get Attributes


        /// <summary>
        /// 
        /// </summary>
        /// <returns>True if the property has been added. <para>False if a property with the same name is already present</para> </returns>
        public virtual bool AddProperty(ElementProperty elementProperty)
        {
            if (elementProperty is IPlateProperty)
            {
                if (_plateProperties.Contains(elementProperty))
                    return false;

                _plateProperties.Add((PlateProperty)elementProperty);
                return true;
            }
            else if (elementProperty is IBrickProperty)
            {
                if (_brickProperties.Contains(elementProperty))
                    return false;

                _brickProperties.Add((BrickProperty)elementProperty);
                return true;
            }
            else
            {
                throw new NotSupportedException($"Type: {elementProperty.GetType()} not suppoted");
            }
        }


        /// <inheritdoc cref="UniqueNameCollection{T}.GetElementByName(string)"/>
        public virtual ElementProperty GetPlateProperty(string name)
        {
            return _plateProperties.GetElementByName(name);
        }


        /// <inheritdoc cref="UniqueNameCollection{T}.GetElementByName(string)"/>
        public virtual ElementProperty GetBrickProperty(string name)
        {
            return _brickProperties.GetElementByName(name);
        }


        public virtual void AddGeometryRestrain()
        {
            throw new NotImplementedException();
        }


        /// <inheritdoc cref="UniqueNameCollection{T}.Add(T)"/>
        public virtual bool AddLoadCase(LoadCase loadCase)
        {
            return _loadCases.Add(loadCase);
        }


        /// <inheritdoc cref="UniqueNameCollection{T}.GetElementByName(string)(T)"/>
        public virtual LoadCase GetLoadCase(string name)
        {
            return _loadCases.GetElementByName(name);
        }


        /// <inheritdoc cref="UniqueNameCollection{T}.Add(T)"/>
        public virtual bool AddFredomCase(FreedomCase freedomCase)
        {
            return _freedomCases.Add(freedomCase);
        }


        /// <inheritdoc cref="UniqueNameCollection{T}.GetElementByName(string)(T)"/>
        public virtual FreedomCase GetFredomCase(string name)
        {
            return _freedomCases.GetElementByName(name);
        }


        public virtual void AddCombination(Combination combination)
        {
            if (!_combinations.Contains(combination))
            {
                _combinations.Add(combination);
            }
        }


        public virtual void AddCombinations(List<Combination> combinations)
        {
            foreach (var combination in combinations)
            {
                if (!_combinations.Contains(combination))
                {
                    _combinations.Add(combination);
                }
            }
        }


        /// <summary>
        /// Add a stage to the stage list. The stage will empty (without elements and nodes)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="analysisType"></param>
        /// <returns></returns>
        public virtual Stage AddStage(string name, AnalysisType analysisType)
        {
            Stage stage = new Stage(name, this, analysisType, false, null);
            _stages.Add(stage);
            return stage;
        }


        /// <summary>
        /// Add a stage the to the stage list. This stage will the copy of <paramref name="stageToCopy"/>
        /// </summary>
        /// <param name="stageToCopy"></param>
        /// <returns></returns>
        public virtual Stage AddStage(Stage stageToCopy)
        {
            Stage stage = new Stage(stageToCopy);
            _stages.Add(stage);
            return stage;
        }

        #endregion


        #region Add Get Geometry


        #region FiniteElements

        /// <summary> Add a <paramref name="finiteElement"/> to the FemModel</summary>
        /// <param name="finiteElement"></param>
        /// <param name="propertyName">The name of the property that will be assigned to the <paramref name="finiteElement"/></param>
        /// <returns></returns>
        /// <inheritdoc cref="GetPlateProperty(string)"/>
        /// <exception cref="ArgumentNullException">If the property list does not contain a property with a name equal to <paramref name="propertyName"/></exception>
        /// <exception cref="ArgumentNullException">If the nodes inside the <paramref name="finiteElement"/> are null</exception>
        public virtual void AddFiniteElement(FiniteElement finiteElement, string propertyName)
        {
            if (finiteElement != null)
            {
                if (finiteElement is Plate)
                {
                    ElementProperty property = GetPlateProperty(propertyName);

                    if (property is null)
                        throw new ArgumentOutOfRangeException($"The property list does not contain {propertyName}");

                    finiteElement.SetProperty(property);

                    AddNodes(finiteElement.Nodes);

                    _elements.Add(finiteElement);
                }
                else if (finiteElement is Brick)
                {
                    ElementProperty property = GetBrickProperty(propertyName);

                    if (property is null)
                        throw new ArgumentOutOfRangeException($"The property list does not contain {propertyName}");

                    finiteElement.SetProperty(property);

                    AddNodes(finiteElement.Nodes);

                    _elements.Add(finiteElement);
                }
                else
                {
                    throw new NotSupportedException(finiteElement.GetType().ToString());
                }

                foreach (var node in finiteElement.Nodes)
                {
                    _nodes.Add(node);
                }

            }
        }


        /// <inheritdoc cref="FemModel.AddFiniteElement(FiniteElement, string)"/>
        public virtual void AddFiniteElements(FiniteElement[] finiteElements, string propertyName)
        {
            foreach (var element in finiteElements)
            {
                AddFiniteElement(element, propertyName);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <inheritdoc cref="FemObjectCollection{T}.GetElementById(int)"/>
        public virtual FiniteElement GetFiniteElement(int index)
        {
            return _elements[index];
        }


        public virtual IEnumerator<FiniteElement> GetElementsEnumerator()
        {
            return _elements.GetEnumerator();
        }


        /// <returns>True if <paramref name="finiteElement"/> is contained in the <see cref="FemModel._elements"/> collections </returns>
        public virtual bool ContainsFiniteElement(FiniteElement finiteElement)
        {
            return _elements.Contains(finiteElement);
        }

        #endregion


        #region Nodes

        /// <inheritdoc cref="FemObjectCollection{T}.Add(T)"/>
        protected virtual int AddNode(Node node)
        {
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
                }

                return indexes;
            }
            throw new ArgumentNullException();
        }


        public virtual Node GetNode(int index)
        {
            return _nodes.GetElementById(index);
        }


        public virtual IEnumerator<Node> GetNodesEnumerator()
        {
            return _nodes.GetEnumerator();
        }


        #endregion


        #region Mesh and shapes


        /// <summary>
        /// Generate planar mesh from a shapes. Mesh options need to be setted by <see cref="Mesh.GenerateMeshOptions"/>
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="options"></param>
        /// <param name="plateProperty"></param>
        /// <param name="loads"></param>
        /// <param name="restrains"></param>
        public virtual void AddShape(Shape shape, IPlateProperty plateProperty, Mesh.GenerateOptions options, List<Load> loads, List<GeometryRestrain> restrains)
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

            bool status = Mesh.Generate(new List<Shape> { shape }, new Dictionary<Shape, GeometryBase[]>() { [shape] = embeddedGeometries.ToArray() }, options, out List<Mesh> meshes, out Mesh.GenerateMeshStatus generateMeshStatus);

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

            AddMesh(meshes.First(), plateProperty, null, vertexLoadMeshEntityMap, vertexLineLoadMeshEntityMap, null, restrainMeshEntityMap);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="shapes"></param>
        /// <param name="options"></param>
        /// <param name="plateProperties"></param>
        /// <param name="loads"></param>
        /// <param name="restrains"></param>
        public virtual void AddShapes(List<Shape> shapes, List<IPlateProperty> plateProperties, Mesh.GenerateOptions options, List<List<Load>> loads, List<List<GeometryRestrain>> restrains)
        {
            if (shapes is null)
                throw new ArgumentNullException(nameof(shapes));

            // Garantisce la stessa lunghezza delle liste, ma non che siano liste di non nulli
            if (shapes.Count != plateProperties.Count)
                throw new ArgumentException($"Size of {nameof(shapes)} and {nameof(plateProperties)} are different");
            if (shapes.Count != loads.Count)
                throw new ArgumentException($"Size of {nameof(shapes)} and {nameof(loads)} are different");
            if (shapes.Count != restrains.Count)
                throw new ArgumentException($"Size of {nameof(shapes)} and {nameof(restrains)} are different");


            for (int i = 0; i < shapes.Count; i++)
            {
                if (shapes[i] is null)
                    throw new ArgumentNullException(nameof(shapes));

                if (plateProperties[i] is null)
                    throw new ArgumentNullException(nameof(plateProperties));

                if (loads[i] is null)
                    throw new ArgumentNullException(nameof(loads));

                if (restrains[i] is null)
                    throw new ArgumentNullException(nameof(restrains));


                AddShape(shapes[i], plateProperties[i], options, loads[i], restrains[i]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="meshes"></param>
        /// <param name="plateProperties"></param>
        /// <param name="brickProperties"></param>
        /// <param name="vertexLoadMeshEntityMap"></param>
        /// <param name="plateLoadMeshEntityMap"></param>
        /// <param name="restrainMeshEntityMap"></param>
        /// <exception cref="ArgumentException">If list of argument does not match</exception>
        public virtual void AddMeshes(List<Mesh> meshes, List<IPlateProperty> plateProperties, List<IBrickProperty> brickProperties, List<Dictionary<IPointLoad, int[]>> vertexLoadMeshEntityMap,
                                        List<Dictionary<ILineLoad, int[]>> vertexLineLoadMeshEntityMap,
                                        List<Dictionary<IAreaLoad, int[]>> plateLoadMeshEntityMap, List<Dictionary<GeometryRestrain, int[]>> restrainMeshEntityMap)
        {

            if (meshes is null)
                throw new ArgumentNullException(nameof(meshes));

            // Garantisce la stessa lunghezza delle liste, ma non che siano liste di non nulli
            if (meshes.Select(i => i.Faces.Count).Max() != 0 && meshes.Count != plateProperties.Count)
                throw new ArgumentException($"Size of {nameof(meshes)} and {nameof(plateProperties)} are different");
            if (meshes.Select(i => i.Volumes.Count).Max() != 0 && meshes.Count != brickProperties.Count)
                throw new ArgumentException($"Size of {nameof(meshes)} and {nameof(brickProperties)} are different");
            if (meshes.Count != vertexLoadMeshEntityMap.Count)
                throw new ArgumentException($"Size of {nameof(meshes)} and {nameof(vertexLoadMeshEntityMap)} are different");
            if (meshes.Count != vertexLineLoadMeshEntityMap.Count)
                throw new ArgumentException($"Size of {nameof(meshes)} and {nameof(vertexLineLoadMeshEntityMap)} are different");
            if (meshes.Select(i => i.Faces.Count).Max() != 0 && meshes.Count != plateLoadMeshEntityMap.Count)
                throw new ArgumentException($"Size of {nameof(meshes)} and {nameof(plateLoadMeshEntityMap)} are different");
            if (meshes.Count != restrainMeshEntityMap.Count)
                throw new ArgumentException($"Size of {nameof(meshes)} and {nameof(restrainMeshEntityMap)} are different");


            for (int i = 0; i < meshes.Count; i++)
            {
                if (meshes[i] is null)
                    throw new ArgumentNullException(nameof(meshes));

                if (plateProperties[i] is null)
                    throw new ArgumentNullException(nameof(plateProperties));

                if (brickProperties[i] is null)
                    throw new ArgumentNullException(nameof(brickProperties));

                if (vertexLoadMeshEntityMap[i] is null)
                    throw new ArgumentNullException(nameof(vertexLoadMeshEntityMap));

                if (vertexLineLoadMeshEntityMap[i] is null)
                    throw new ArgumentNullException(nameof(vertexLineLoadMeshEntityMap));

                if (plateLoadMeshEntityMap[i] is null)
                    throw new ArgumentNullException(nameof(plateLoadMeshEntityMap));

                if (restrainMeshEntityMap[i] is null)
                    throw new ArgumentNullException(nameof(restrainMeshEntityMap));

                AddMesh(meshes[i], plateProperties[i], brickProperties[i], vertexLoadMeshEntityMap[i], vertexLineLoadMeshEntityMap[i], plateLoadMeshEntityMap[i], restrainMeshEntityMap[i]);
            }

        }


        /// <summary>
        /// Add a mesh to the Fem model
        /// </summary>
        /// <param name="mesh"></param> 
        /// <param name="plateProperty"></param>
        /// <param name="brickProperty"></param>
        /// <param name="vertexLoadMeshEntityMap">Map between <see cref="IPointLoad"/> and <see cref="MeshVertex.Id"/></param>
        /// <param name="vertexLineLoadMeshEntityMap">Map between <see cref="ILineLoad"/> and <see cref="MeshVertex.Id"/></param>
        /// <param name="plateLoadMeshEntityMap">Map between <see cref="IAreaLoad"/> and <see cref="MeshFace.Id"/></param>
        /// <param name="restrainMeshEntityMap">Map between IGeometryRestrain and <see cref="MeshVertex.Id"/></param>
        /// <exception cref="KeyNotFoundException">If a <see cref="MeshVertex.Id"/> of <paramref name="restrainMeshEntityMap"/> is not found in the <paramref name="mesh"/> vertices ids</exception>
        public virtual void AddMesh(Mesh mesh, IPlateProperty plateProperty, IBrickProperty brickProperty, 
                                    Dictionary<IPointLoad, int[]> vertexLoadMeshEntityMap, Dictionary<ILineLoad, int[]> vertexLineLoadMeshEntityMap,
                                    Dictionary<IAreaLoad, int[]> plateLoadMeshEntityMap, Dictionary<GeometryRestrain, int[]> restrainMeshEntityMap)
        {

            Dictionary<int, int> nodesNewIndexMap = new Dictionary<int, int>(); // Mappa tra indici dei nodi dentro _nodes e indici dei vertici della mesh nel caso esistano già dentro _nodes.
            Dictionary<int, int> platesNewIndexMap = new Dictionary<int, int>();
            Dictionary<int, int> brickNewIndexMap = new Dictionary<int, int>();


            if (mesh is null)
            {
                throw new ArgumentNullException(nameof(mesh));
            }

            // Aggiorno la lista proprietà
            if (mesh.Faces.Count != 0)
            {
                if (plateProperty is null)
                    throw new ArgumentNullException(nameof(plateProperty));

                if (!AddProperty((ElementProperty)plateProperty))
                    throw new ArgumentException($"A property with name {(plateProperty as PlateProperty).Name} already exist");
            }


            if (mesh.Volumes.Count != 0)
            {
                if (brickProperty is null)
                    throw new ArgumentNullException(nameof(brickProperty));

                if (!AddProperty((ElementProperty)brickProperty))
                    throw new ArgumentException($"A property with name {(brickProperty as BrickProperty).Name} already exist");
            }


            // Aggiunge nodi alla collection di nodi
            foreach (var vertex in mesh.Vertices)
            {
                var nodeIndex = _nodes.Add(new Node(vertex.Point));

                if (nodeIndex != vertex.Id) // Se sono diversi vuol dire che esisteva già l'indice Vertex.iD e il vertice è stato aggiunto alla collection con un ID diverso.
                {
                    nodesNewIndexMap[vertex.Id] = nodeIndex; // Mappa fra vecchio e nuovo
                }
            }


            // Aggiunge elementi FEM
            foreach (var face in mesh.Faces)
            {
                if (plateProperty is IPlateProperty ipp)
                {
                    if (face.IsQuad)
                    {
                        var plate = new Plate(new Node[] { _nodes[nodesNewIndexMap.ContainsKey(face.A) ? nodesNewIndexMap[face.A] : face.A],
                                                           _nodes[nodesNewIndexMap.ContainsKey(face.B) ? nodesNewIndexMap[face.B] : face.B],
                                                           _nodes[nodesNewIndexMap.ContainsKey(face.C) ? nodesNewIndexMap[face.C] : face.C],
                                                           _nodes[nodesNewIndexMap.ContainsKey(face.D) ? nodesNewIndexMap[face.D] : face.D]}
                                                       );

                        plate.SetProperty((ElementProperty)plateProperty);

                        var plateIndex = _elements.Add(plate);

                        if (plateIndex != face.Id) // Se sono diversi vuol dire che esisteva già l'indice element .iD e la collection l'ha modificato
                            platesNewIndexMap[face.Id] = plateIndex;
                    }
                    else
                    {

                        var plate = new Plate(new Node[] { _nodes[nodesNewIndexMap.ContainsKey(face.A) ? nodesNewIndexMap[face.A] : face.A],
                                                           _nodes[nodesNewIndexMap.ContainsKey(face.B) ? nodesNewIndexMap[face.B] : face.B],
                                                           _nodes[nodesNewIndexMap.ContainsKey(face.C) ? nodesNewIndexMap[face.C] : face.C]}
                                                       );

                        plate.SetProperty((ElementProperty)plateProperty);

                        var plateIndex = _elements.Add(plate);

                        if (plateIndex != face.Id) // Se sono diversi vuol dire che esisteva già l'indice element .iD e la collection l'ha modificato
                            platesNewIndexMap[face.Id] = plateIndex;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }

            }


            foreach (var volume in mesh.Volumes)
            {
                if (volume.IsQuadrangular)
                {
                    if (brickProperty is BrickProperty bp)
                    {

                        var brick = new Brick(new Node[] { _nodes[nodesNewIndexMap.ContainsKey(volume.A) ? nodesNewIndexMap[volume.A] : volume.A],
                                                           _nodes[nodesNewIndexMap.ContainsKey(volume.B) ? nodesNewIndexMap[volume.B] : volume.B],
                                                           _nodes[nodesNewIndexMap.ContainsKey(volume.C) ? nodesNewIndexMap[volume.C] : volume.C],
                                                           _nodes[nodesNewIndexMap.ContainsKey(volume.D) ? nodesNewIndexMap[volume.D] : volume.D],
                                                           _nodes[nodesNewIndexMap.ContainsKey(volume.E) ? nodesNewIndexMap[volume.E] : volume.E],
                                                           _nodes[nodesNewIndexMap.ContainsKey(volume.F) ? nodesNewIndexMap[volume.F] : volume.F],
                                                           _nodes[nodesNewIndexMap.ContainsKey(volume.G) ? nodesNewIndexMap[volume.G] : volume.G],
                                                           _nodes[nodesNewIndexMap.ContainsKey(volume.H) ? nodesNewIndexMap[volume.H] : volume.H]}
                                                       );
                        
                        brick.SetProperty(bp);

                        var brickIndex = _elements.Add(brick);
                        
                        if (brickIndex != volume.Id) // Se sono diversi vuol dire che esisteva già l'indice element .iD e la collection l'ha modificato
                            brickNewIndexMap[volume.Id] = brickIndex;
                    }
                    else
                        throw new NotImplementedException();
                }
                else
                {
                    if (brickProperty is BrickProperty bp)
                    {
                        var brick = new Brick(new Node[] { _nodes[nodesNewIndexMap.ContainsKey(volume.A) ? nodesNewIndexMap[volume.A] : volume.A],
                                                           _nodes[nodesNewIndexMap.ContainsKey(volume.B) ? nodesNewIndexMap[volume.B] : volume.B],
                                                           _nodes[nodesNewIndexMap.ContainsKey(volume.C) ? nodesNewIndexMap[volume.C] : volume.C],
                                                           _nodes[nodesNewIndexMap.ContainsKey(volume.D) ? nodesNewIndexMap[volume.D] : volume.D],
                                                           _nodes[nodesNewIndexMap.ContainsKey(volume.E) ? nodesNewIndexMap[volume.E] : volume.E],
                                                           _nodes[nodesNewIndexMap.ContainsKey(volume.F) ? nodesNewIndexMap[volume.F] : volume.F]}
                                                       );

                        brick.SetProperty(bp);

                        var brickIndex = _elements.Add(brick);

                        if (brickIndex != volume.Id) // Se sono diversi vuol dire che esisteva già l'indice element .iD e la collection l'ha modificato
                            brickNewIndexMap[volume.Id] = brickIndex;
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
                    if (AddFredomCase(geometryRestrain.FreedomCase))
                        freedomCase = geometryRestrain.FreedomCase;
                    else 
                        freedomCase = GetFredomCase(geometryRestrain.FreedomCase.Name); // se è già presente, mi prendo l'istanza di quello già presente


                    NodeRestrainAttribute nra = new NodeRestrainAttribute(freedomCase, geometryRestrain.CoordinateSystem);
                    NodeStiffnessAttribute nsa = new NodeStiffnessAttribute(freedomCase, geometryRestrain.CoordinateSystem);

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

                    LoadCase loadCase;
                    if (AddLoadCase(lc))
                        loadCase = lc;
                    else
                        loadCase = GetLoadCase(lc.Name); // se è già presente, mi prendo l'istanza di quello già presente


                    foreach (var index in indexes)
                    {
                        int nodeId = nodesNewIndexMap.ContainsKey(index) ? nodesNewIndexMap[index] : index;

                        Node node = _nodes.GetElementById(nodeId); // se non trova l'indice viene lanciata una keynotfoundException

                        if (load is PointLoad pl)
                        {
                            NodeForceAttribute nfa = new NodeForceAttribute(loadCase, pl.CoordinateSystem, pl.F1, pl.F2, pl.F3, pl.M1, pl.M2, pl.M3);
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

                    LoadCase loadCase;
                    if (AddLoadCase(lc))
                        loadCase = lc;
                    else
                        loadCase = GetLoadCase(lc.Name); // se è già presente, mi prendo l'istanza di quello già presente

                    foreach (var index in indexes)
                    {
                        int nodeId = nodesNewIndexMap.ContainsKey(index) ? nodesNewIndexMap[index] : index;

                        Node node = _nodes.GetElementById(nodeId); // se non trova l'indice viene lanciata una keynotfoundException

                        if (load is LineLoad ll)
                        {
                            var l = ll.GetGeometry();

                            // carico è F/L o FL/L
                            // carico puntuale è F/L*L/nnodi
                            var factor = lineLenght / indexes.Count();
                            NodeForceAttribute nfa = new NodeForceAttribute(ll.LoadCase, ll.CoordinateSystem, 
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

                    LoadCase loadCase;
                    if (AddLoadCase(lc))
                        loadCase = lc;
                    else
                        loadCase = GetLoadCase(lc.Name); // se è già presente, mi prendo l'istanza di quello già presente

                    foreach (var index in indexes)
                    {
                        int plateId = platesNewIndexMap.ContainsKey(index) ? platesNewIndexMap[index] : index;

                        FiniteElement finiteElement = _elements.GetElementById(plateId); // se non trova l'indice viene lanciata una keynotfoundException

                        Plate plate = finiteElement as Plate;

                        if (plate is null)
                            throw new ArgumentException($"Element with id: {plateId} {index} is not a plate");

                        if (load is NormalAreaLoad pl)
                        {
                            PlateNormalPressureAttribute pna = new PlateNormalPressureAttribute(pl.LoadCase, pl.Pressure);
                            plate.AddLoadCaseAttribute(pna);
                        }
                        else if (load is AreaLoad gal)
                        {
                            PlatePressureAttribute ppa = new PlatePressureAttribute(gal.LoadCase, gal.CoordinateSystem, gal.P1, gal.P2, gal.P3);
                            plate.AddLoadCaseAttribute(ppa);
                        }
                        else
                            throw new NotImplementedException();

                    }
                }

            }

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
            /// Fare in modo che chiamando questo metodo i nodi uguali ma che avranno ID diverso vengano tolti dalla collection <see cref="FemModel._nodes"/> 
            /// tranne uno, e che i riferimenti ai nodi dentro gli elementi vengano sostituiti con quelli dell'unico nodo rimasto 

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

        #endregion

        #region Public method override 


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            throw new NotImplementedException();
        }


        public override bool Equals(object obj)
        {



            return obj is FemModel model &&
                   base.Equals(obj) &&
                   EqualityComparer<FemObjectCollection<Node>>.Default.Equals(_nodes, model._nodes) &&
                   EqualityComparer<FemObjectCollection<FiniteElement>>.Default.Equals(_elements, model._elements) &&
                   EqualityComparer<List<IPlateProperty>>.Default.Equals(_plateProperties, model._plateProperties) &&
                   EqualityComparer<List<IBrickProperty>>.Default.Equals(_brickProperties, model._brickProperties) &&
                   EqualityComparer<List<Load>>.Default.Equals(_loads, model._loads) &&
                   EqualityComparer<List<LoadCase>>.Default.Equals(_loadCases, model._loadCases) &&
                   EqualityComparer<List<Combination>>.Default.Equals(_combinations, model._combinations) &&
                   EqualityComparer<List<FreedomCase>>.Default.Equals(_freedomCases, model._freedomCases) &&
                   EqualityComparer<List<ResultNodeDisplacement>>.Default.Equals(_resultNodeDisplacements, model._resultNodeDisplacements) &&
                   EqualityComparer<List<ResultNodeForce>>.Default.Equals(_resultNodeForce, model._resultNodeForce) &&
                   EqualityComparer<List<ResultPlateStress>>.Default.Equals(_resultPlateStress, model._resultPlateStress) &&
                   EqualityComparer<List<Stage>>.Default.Equals(_stages, model._stages);
        }


        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<FemObjectCollection<Node>>.Default.GetHashCode(_nodes);
            hashCode = hashCode * -17 + EqualityComparer<FemObjectCollection<FiniteElement>>.Default.GetHashCode(_elements);
            hashCode = hashCode * -17 + EqualityComparer<List<IPlateProperty>>.Default.GetHashCode(_plateProperties);
            hashCode = hashCode * -17 + EqualityComparer<List<IBrickProperty>>.Default.GetHashCode(_brickProperties);
            hashCode = hashCode * -17 + EqualityComparer<List<Load>>.Default.GetHashCode(_loads);
            hashCode = hashCode * -17 + EqualityComparer<List<LoadCase>>.Default.GetHashCode(_loadCases);
            hashCode = hashCode * -17 + EqualityComparer<List<Combination>>.Default.GetHashCode(_combinations);
            hashCode = hashCode * -17 + EqualityComparer<List<FreedomCase>>.Default.GetHashCode(_freedomCases);
            hashCode = hashCode * -17 + EqualityComparer<List<ResultNodeDisplacement>>.Default.GetHashCode(_resultNodeDisplacements);
            hashCode = hashCode * -17 + EqualityComparer<List<ResultNodeForce>>.Default.GetHashCode(_resultNodeForce);
            hashCode = hashCode * -17 + EqualityComparer<List<ResultPlateStress>>.Default.GetHashCode(_resultPlateStress);
            hashCode = hashCode * -17 + EqualityComparer<List<Stage>>.Default.GetHashCode(_stages);
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
