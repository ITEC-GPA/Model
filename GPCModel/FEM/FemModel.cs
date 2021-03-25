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
        protected List<IPlateProperty> _plateProperties;
        protected List<IBrickProperty> _brickProperties;

        // CARICHI
        protected List<Load> _loads;
        protected List<LoadCase> _loadCases;
        protected List<Combination> _combinations;

        // FREEDOM CASES 
        protected List<FreedomCase> _freedomCases;

        // RISULTATI
        protected List<ResultNodeDisplacement> _resultNodeDisplacements;

        protected List<ResultNodeForce> _resultNodeForce;

        protected List<ResultPlateStress> _resultPlateStress;

        // STAGE
        protected List<Stage> _stages;

        // CoordinatesSystem ? 

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

            _plateProperties = new List<IPlateProperty>();
            _brickProperties = new List<IBrickProperty>();

            _loadCases = new List<LoadCase>();
            _freedomCases = new List<FreedomCase>();
            _combinations = new List<Combination>();

            _loads = new List<Load>();
            
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
        public virtual void AddMesh(Mesh mesh, IPlateProperty plateProperty, IBrickProperty brickProperty, Dictionary<IPointLoad, int[]> vertexLoadMeshEntityMap, Dictionary<ILineLoad, int[]> vertexLineLoadMeshEntityMap,
                                     Dictionary<IAreaLoad, int[]> plateLoadMeshEntityMap,  Dictionary<GeometryRestrain, int[]> restrainMeshEntityMap)
        {
            
            Dictionary<int, int> nodesNewIndexMap = new Dictionary<int, int>(); // Mappa tra indici dei nodi dentro _nodes e indici dei vertici della mesh nel caso esistano già dentro _nodes.
            Dictionary<int, int> platesNewIndexMap = new Dictionary<int, int>(); 
            Dictionary<int, int> brickNewIndexMap = new Dictionary<int, int>(); 


            // Aggiorno lista proprietà

            if (mesh.Faces.Count != 0)
            {
                if (plateProperty is null)
                    throw new ArgumentNullException(nameof(plateProperty));
                else
                {
                    if (!_plateProperties.Contains(plateProperty))
                        _plateProperties.Add(plateProperty);
                }
            }


            if (mesh.Volumes.Count != 0)
            {
                if (brickProperty is null )
                    throw new ArgumentNullException(nameof(brickProperty));
                else
                {
                    if (!_brickProperties.Contains(brickProperty))
                        _brickProperties.Add(brickProperty);
                } 
            }


            // Aggiunge nodi alla collection di nodi
            foreach(var vertex in mesh.Vertices)
            {
                var nodeIndex = _nodes.Add(new Node(vertex.Point, vertex.Id));

                
                if (nodeIndex != vertex.Id) // Se sono diversi vuol dire che esisteva già l'indice Vertex.iD e il vertice è stato aggiunto alla collection con un ID diverso.
                {
                    //nodesNewIndexMap[nodeIndex] = vertex.Id; // Mappa fra vecchio e nuovo

                    nodesNewIndexMap[vertex.Id] = nodeIndex; // Mappa fra vecchio e nuovo
                }
            }

            // Aggiunge elementi FEM

            foreach(var face in mesh.Faces)
            {
                if (face.IsQuad)
                {
                    if (plateProperty is PlateProperty pp)
                    {
                        var nodes = new Node[] { _nodes[nodesNewIndexMap.ContainsKey(face.A) ? nodesNewIndexMap[face.A] : face.A],
                                                                            _nodes[nodesNewIndexMap.ContainsKey(face.B) ? nodesNewIndexMap[face.B] : face.B],
                                                                            _nodes[nodesNewIndexMap.ContainsKey(face.C) ? nodesNewIndexMap[face.C] : face.C],
                                                                            _nodes[nodesNewIndexMap.ContainsKey(face.D) ? nodesNewIndexMap[face.D] : face.D] };

                        
                        var plateIndex = _elements.Add(new Plate(new Node[] { _nodes[nodesNewIndexMap.ContainsKey(face.A) ? nodesNewIndexMap[face.A] : face.A],
                                                                              _nodes[nodesNewIndexMap.ContainsKey(face.B) ? nodesNewIndexMap[face.B] : face.B],
                                                                              _nodes[nodesNewIndexMap.ContainsKey(face.C) ? nodesNewIndexMap[face.C] : face.C],
                                                                              _nodes[nodesNewIndexMap.ContainsKey(face.D) ? nodesNewIndexMap[face.D] : face.D]},
                                                                              pp, face.Id));

                        if (plateIndex != face.Id) // Se sono diversi vuol dire che esisteva già l'indice element .iD e la collection l'ha modificato
                            platesNewIndexMap[face.Id] = plateIndex;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    if (plateProperty is PlateProperty pp)
                    {
                        var plateIndex = _elements.Add(new Plate(new Node[] { _nodes[nodesNewIndexMap.ContainsKey(face.A) ? nodesNewIndexMap[face.A] : face.A],
                                                                              _nodes[nodesNewIndexMap.ContainsKey(face.B) ? nodesNewIndexMap[face.B] : face.B],
                                                                              _nodes[nodesNewIndexMap.ContainsKey(face.C) ? nodesNewIndexMap[face.C] : face.C]},
                                                                              pp, face.Id));

                        if (plateIndex != face.Id) // Se sono diversi vuol dire che esisteva già l'indice element .iD e la collection l'ha modificato
                            platesNewIndexMap[face.Id] = plateIndex;
                    }
                    else
                        throw new NotImplementedException();
                }
            }

            foreach(var volume in mesh.Volumes)
            {
                if (volume.IsQuadrangular)
                {
                    if (brickProperty is BrickProperty bp)
                    {
                        var brickIndex = _elements.Add(new Brick(new Node[] { _nodes[nodesNewIndexMap.ContainsKey(volume.A) ? nodesNewIndexMap[volume.A] : volume.A],
                                                                              _nodes[nodesNewIndexMap.ContainsKey(volume.B) ? nodesNewIndexMap[volume.B] : volume.B],
                                                                              _nodes[nodesNewIndexMap.ContainsKey(volume.C) ? nodesNewIndexMap[volume.C] : volume.C],
                                                                              _nodes[nodesNewIndexMap.ContainsKey(volume.D) ? nodesNewIndexMap[volume.D] : volume.D],
                                                                              _nodes[nodesNewIndexMap.ContainsKey(volume.E) ? nodesNewIndexMap[volume.E] : volume.E],
                                                                              _nodes[nodesNewIndexMap.ContainsKey(volume.F) ? nodesNewIndexMap[volume.F] : volume.F],
                                                                              _nodes[nodesNewIndexMap.ContainsKey(volume.G) ? nodesNewIndexMap[volume.G] : volume.G],
                                                                              _nodes[nodesNewIndexMap.ContainsKey(volume.H) ? nodesNewIndexMap[volume.H] : volume.H]},
                                                                              bp, volume.Id));

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
                        var brickIndex = _elements.Add(new Brick(new Node[] { _nodes[nodesNewIndexMap.ContainsKey(volume.A) ? nodesNewIndexMap[volume.A] : volume.A],
                                                                              _nodes[nodesNewIndexMap.ContainsKey(volume.B) ? nodesNewIndexMap[volume.B] : volume.B],
                                                                              _nodes[nodesNewIndexMap.ContainsKey(volume.C) ? nodesNewIndexMap[volume.C] : volume.C],
                                                                              _nodes[nodesNewIndexMap.ContainsKey(volume.D) ? nodesNewIndexMap[volume.D] : volume.D],
                                                                              _nodes[nodesNewIndexMap.ContainsKey(volume.E) ? nodesNewIndexMap[volume.E] : volume.E],
                                                                              _nodes[nodesNewIndexMap.ContainsKey(volume.F) ? nodesNewIndexMap[volume.F] : volume.F]},
                                                                              bp, volume.Id));
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

                    Dictionary<LinearSolver.DOF, bool>  restrains = geometryRestrain.GetRestrains();
                    Dictionary<LinearSolver.DOF, double>  stiffneses = geometryRestrain.GetStiffnesses();
                    Dictionary<LinearSolver.DOF, double>  displacements = geometryRestrain.GetImposedDisplacement();


                    NodeRestrainAttribute nra = new NodeRestrainAttribute(geometryRestrain.FreedomCase, geometryRestrain.CoordinateSystem);
                    NodeStiffnessAttribute nsa = new NodeStiffnessAttribute(geometryRestrain.FreedomCase, geometryRestrain.CoordinateSystem);

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

                        if (!_freedomCases.Contains(geometryRestrain.FreedomCase))
                            _freedomCases.Add(geometryRestrain.FreedomCase);
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

                    foreach (var index in indexes)
                    {
                        int nodeId = nodesNewIndexMap.ContainsKey(index) ? nodesNewIndexMap[index] : index;

                        Node node = _nodes.GetElementById(nodeId); // se non trova l'indice viene lanciata una keynotfoundException

                        if (load is PointLoad pl)
                        {
                            NodeForceAttribute nfa = new NodeForceAttribute(pl.LoadCase, pl.CoordinateSystem, pl.F1, pl.F2, pl.F3, pl.M1, pl.M2, pl.M3);
                            node.AddAttribute(nfa);

                            if (!_loadCases.Contains(pl.LoadCase))
                                _loadCases.Add(pl.LoadCase);
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

                    foreach (var index in indexes)
                    {
                        int nodeId = nodesNewIndexMap.ContainsKey(index) ? nodesNewIndexMap[index] : index;

                        Node node = _nodes.GetElementById(nodeId); // se non trova l'indice viene lanciata una keynotfoundException

                        if (load is LineLoad ll)
                        {
                            var l = ll.GetGeometry();

                            // carico è F/L o FL/L
                            // carico puntuale è F/L*L/nnodi
                            NodeForceAttribute nfa = new NodeForceAttribute(ll.LoadCase, ll.CoordinateSystem, ll.F1 * lineLenght / indexes.Count(), ll.F2 * lineLenght / indexes.Count(), 
                                                                            ll.F3 * lineLenght / indexes.Count(), ll.M1 * lineLenght / indexes.Count(), ll.M2 * lineLenght / indexes.Count(),
                                                                            ll.M3 * lineLenght / indexes.Count());
                            node.AddAttribute(nfa);

                            if (!_loadCases.Contains(ll.LoadCase))
                                _loadCases.Add(ll.LoadCase);
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
                            plate.AddAttribute(pna);

                            if (!_loadCases.Contains(pl.LoadCase))
                                _loadCases.Add(pl.LoadCase);
                        }
                        else if (load is AreaLoad gal)
                        {
                            PlatePressureAttribute ppa = new PlatePressureAttribute(gal.LoadCase, gal.CoordinateSystem, gal.P1, gal.P2, gal.P3);
                            plate.AddAttribute(ppa);

                            if (!_loadCases.Contains(gal.LoadCase))
                                _loadCases.Add(gal.LoadCase);
                        }
                        else
                            throw new NotImplementedException();

                    }
                }

            }
        }

        /// <summary>
        /// Add a finite element to the FemModel
        /// </summary>
        /// <param name="finiteElement"></param>
        public virtual void AddFiniteElement(FiniteElement finiteElement)
        {
            if (finiteElement != null)
            {
                foreach (var node in finiteElement.Nodes)
                {
                    _nodes.Add(node);
                } 
                
                _elements.Add(finiteElement);
                AddProperty(finiteElement.Property);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementProperty"></param>
        /// <returns>True if the property has been added or already contained in the FemModel</returns>
        protected virtual bool AddProperty(ElementProperty elementProperty)
        {
            if (elementProperty is IPlateProperty ipl)
            {
                if (!_plateProperties.Contains(ipl))
                {
                    _plateProperties.Add(ipl);
                }
                return true;
            }
            else if (elementProperty is IBrickProperty ibp)
            {
                if (!_brickProperties.Contains(ibp))
                {
                    _brickProperties.Add(ibp);
                }
                return true;
            }

            return false;
        }


        public virtual void AddLoad()
        {
            throw new NotImplementedException();
        }


        public virtual void AddGeometryRestrain()
        {
            throw new NotImplementedException();
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="finiteElement"></param>
        /// <returns>True if <paramref name="finiteElement"/> is contained in the <see cref="FemModel._elements"/> collections </returns>
        public virtual bool ContainsFiniteElement(FiniteElement finiteElement)
        {
            return _elements.Contains(finiteElement);
        }

        public void CleanMesh()
        {
            /// Fare in modo che chiamando questo metodo i nodi uguali ma che avranno ID diverso vengano tolti dalla collection <see cref="FemModel._nodes"/> 
            /// tranne uno, e che i riferimenti ai nodi dentro gli elementi vengano sostituiti con quelli dell'unico nodo rimasto 

            throw new NotImplementedException();
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
        

        public virtual Mesh GetMesh()
        {
            Mesh mesh = new Mesh();

            foreach(var element in _elements)
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
