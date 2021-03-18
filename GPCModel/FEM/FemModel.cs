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
    public class FemModel : ModelObject
    {

        #region Variables
        /// <summary>
        /// Collection of <see cref="Node"/>
        /// </summary>
        protected FemObjectCollection<Node> _nodes;

        /// <summary>
        /// Collection of <see cref="FiniteElement"/>
        /// </summary>
        protected FemObjectCollection<FiniteElement> _elements;

        // indice del valore dei dizionari parte da 1
        protected Dictionary<IPlateProperty, int> _plateProperties;
        protected Dictionary<IBrickProperty, int> _brickProperties;

        protected Dictionary<LoadCase, int> _loadCases;
        protected Dictionary<Combination, int> _combinations;

        protected Dictionary<FreedomCase, int> _freedomCases; 

        protected List<Load> _loads;

        protected List<ResultNodeDisplacement> _resultNodeDisplacements;
        protected List<ResultNodeForce> _resultNodeForce;

        protected List<ResultPlateStress> _resultPlateStress;


        // CoordinatesSystem ? 

        #endregion

        #region MyRegion

        public List<ResultPlateStress> ResultPlateStresses => _resultPlateStress;
        public List<ResultNodeDisplacement> ResultNodeDisplacement => _resultNodeDisplacements;
        public List<ResultNodeForce> ResultNodeForce => _resultNodeForce;

        #endregion


        #region Constructors

        public FemModel()
            : this(string.Empty)
        {
            
        }

        public FemModel(string name) : base(Guid.NewGuid(), name)
        {
            _nodes = new FemObjectCollection<Node>();
            _elements = new FemObjectCollection<FiniteElement>();

            _plateProperties = new Dictionary<IPlateProperty, int>();
            _brickProperties = new Dictionary<IBrickProperty, int>();

            _loadCases = new Dictionary<LoadCase, int>();
            _freedomCases = new Dictionary<FreedomCase, int>();
            _combinations = new Dictionary<Combination, int>();

            _loads = new List<Load>();
            
            _resultPlateStress = new List<ResultPlateStress>();
            _resultNodeForce = new List<ResultNodeForce>();
            _resultNodeDisplacements = new List<ResultNodeDisplacement>();

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
                    if (!_plateProperties.ContainsKey(plateProperty))
                        _plateProperties[plateProperty] = _plateProperties.Values.DefaultIfEmpty().Max() + 1;
                }
            }


            if (mesh.Volumes.Count != 0)
            {
                if (brickProperty is null )
                    throw new ArgumentNullException(nameof(brickProperty));
                else
                {
                    if (!_brickProperties.ContainsKey(brickProperty))
                        _brickProperties[brickProperty] = _brickProperties.Values.DefaultIfEmpty().Max() + 1;
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

                        if (!_freedomCases.ContainsKey(geometryRestrain.FreedomCase))
                            _freedomCases[geometryRestrain.FreedomCase] = _freedomCases.Values.Count > 0 ? _freedomCases.Values.Max() + 1 : 1;
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

                            if (!_loadCases.ContainsKey(pl.LoadCase))
                                _loadCases[pl.LoadCase] = _loadCases.Values.DefaultIfEmpty().Max() + 1;
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

                            if (!_loadCases.ContainsKey(ll.LoadCase))
                                _loadCases[ll.LoadCase] = _loadCases.Values.DefaultIfEmpty().Max() + 1;
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

                            if (!_loadCases.ContainsKey(pl.LoadCase))
                                _loadCases[pl.LoadCase] = _loadCases.Values.DefaultIfEmpty().Max() + 1;
                        }
                        else if (load is AreaLoad gal)
                        {
                            PlatePressureAttribute ppa = new PlatePressureAttribute(gal.LoadCase, gal.CoordinateSystem, gal.P1, gal.P2, gal.P3);
                            plate.AddAttribute(ppa);

                            if (!_loadCases.ContainsKey(gal.LoadCase))
                                _loadCases[gal.LoadCase] = _loadCases.Values.DefaultIfEmpty().Max() + 1;
                        }
                        else
                            throw new NotImplementedException();

                    }
                }

            }
        }


        public virtual void AddPlate()
        {
            throw new NotImplementedException();
        }

        public virtual void AddBrick()
        {
            throw new NotImplementedException();
        }

        public virtual void AddBeam()
        {
            throw new NotImplementedException();
        }

        public virtual void AddLoad()
        {
            throw new NotImplementedException();
        }

        public virtual void AddGeometryRestrain()
        {
            throw new NotImplementedException();
        }

        public virtual void AddCombination(Combination combination)
        {
            if (!_combinations.ContainsKey(combination))
            {
                _combinations.Add(combination, _combinations.Values.DefaultIfEmpty().Max() + 1);
            }
        }

        public virtual void AddCombinations(List<Combination> combinations)
        {
            int index = _combinations.Values.DefaultIfEmpty().Max();
            foreach (var combination in combinations)
            {
                if (!_combinations.ContainsKey(combination))
                {
                    _combinations.Add(combination, index++);
                }
            }
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

        #endregion

    }
}
