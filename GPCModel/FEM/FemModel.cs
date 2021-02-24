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

namespace GPC.Model.FEM
{
    public class FemModel : ModelObject
    {

        #region Variables
        /// <summary>
        /// Collection of <see cref="Node"/>
        /// </summary>
        private FemObjectCollection<Node> _nodes;

        /// <summary>
        /// Collection of <see cref="FiniteElement"/>
        /// </summary>
        private FemObjectCollection<FiniteElement> _elements;


        private Dictionary<IPlateProperty, int> _plateProperties;
        private Dictionary<IBrickProperty, int> _brickProperties;

        private Dictionary<LoadCase, int> _loadCases;
        private Dictionary<FreedomCase, int> _freedomCases;

        private Dictionary<GeometryRestrain, int> _geometryRestrain;

        private List<Load> _loads;

        // CoordinatesSystem ? 

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

            _geometryRestrain = new Dictionary<GeometryRestrain, int>();
            _loads = new List<Load>();
        }

        public FemModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Generate planar mesh from a shapes. Mesh options need to be setted by <see cref="GenerateMeshOptions"/>
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="plateProperty"></param>
        /// <param name="brickProperty"></param>
        /// <param name="loads"></param>
        /// <param name="restrains"></param>
        public virtual void AddShape(Shape shape, IPlateProperty plateProperty, IBrickProperty brickProperty, List<Load> loads, List<GeometryRestrain> restrains)
        {
            //Dictionary<Shape, List<GeometryBase>> embeddedGeometries = new Dictionary<Shape, List<GeometryBase>>();

            var embeddedGeometries = new HashSet<GeometryBase>(); // geometrie uniche da passare al meshatore
            var geometryLoadMap = new Dictionary<GeometryBase, List<Load>>(); // associazione fra geometria e carichi
            var geometryRestrainMap = new Dictionary<GeometryBase, List<GeometryRestrain>>(); // associazione fra geometria e restrain

            foreach (var load in loads) // per ogni carico embedda la geometria
            {
                if (load is LineLoad ll)
                {
                    var geom = ll.GetGeometry();
                    if (!geometryLoadMap.ContainsKey(geom))
                    {
                        geometryLoadMap[geom] = new List<Load>() { load };
                        embeddedGeometries.Add(geom);
                    }
                    else
                        geometryLoadMap[geom].Add(load);
                }
                else if (load is PointLoad pl)
                {
                    var geom = pl.GetGeometry();
                    if (!geometryLoadMap.ContainsKey(geom))
                    {
                        geometryLoadMap[geom] = new List<Load>() { load };
                        embeddedGeometries.Add(geom);
                    }
                    else
                        geometryLoadMap[geom].Add(load);
                }
                else if (load is GlobalAreaLoad gal)
                {
                    throw new NotImplementedException($"Load type: {load.GetType()} not implemented");
                    //embeddedGeometries.Add(gal.GetGeometry());
                }
                else if (load is NormalAreaLoad nal)
                {
                    throw new NotImplementedException($"Load type: {load.GetType()} not implemented");
                    //embeddedGeometries.Add(nal.GetGeometry());
                }
                else
                    throw new NotSupportedException($"Load type: {load.GetType()} not supported");                        
            }


            foreach (var restrain in restrains) // per ogni carico embedda la geometria
            {
                if (restrain is LineRestrain lr)
                {
                    var geom = lr.GetGeometry();
                    if (!geometryRestrainMap.ContainsKey(geom))
                    {
                        geometryRestrainMap[geom] = new List<GeometryRestrain>() { restrain };
                        embeddedGeometries.Add(geom);
                    }
                    else
                        geometryRestrainMap[geom].Add(restrain);
                }
                else if (restrain is PointRestrain pr)
                {
                    var geom = pr.GetGeometry();
                    if (!geometryRestrainMap.ContainsKey(geom))
                    {
                        geometryRestrainMap[geom] = new List<GeometryRestrain>() { restrain };
                        embeddedGeometries.Add(geom);
                    }
                    else
                        geometryRestrainMap[geom].Add(restrain);
                }
                else
                    throw new NotSupportedException($"Restrain type: {restrain.GetType()} not supported");
            }

            List<Mesh> meshes = Mesh.Generate(new List<Shape> { shape }, new Dictionary<Shape, GeometryBase[]>() { [shape] = embeddedGeometries.ToArray() }, out Dictionary<Mesh, Dictionary<GeometryBase, int[]>> embeddedGeometriesVertexMap);

            if (meshes.Count > 1) // Non è possibile ma controlliamo lo stesso
                throw new Exception();



        }


        /// <summary>
        /// Generate planar meshes from a List of shapes. Mesh options need to be setted by <see cref="GenerateMeshOptions"/>
        /// </summary>
        /// <param name="shapes"></param>
        /// <param name="plateProperties"></param>
        /// <param name="brickProperties"></param>
        /// <param name="loads"></param>
        /// <param name="restrains"></param>
        public virtual void AddShapes(List<Shape> shapes, List<IPlateProperty> plateProperties, List<IBrickProperty> brickProperties, List<List<Load>> loads, List<List<GeometryRestrain>> restrains)
        {
            throw new NotImplementedException();
        }


        public virtual void AddMeshes(List<Mesh> meshes, List<IPlateProperty> plateProperties, List<IBrickProperty> brickProperties, Dictionary<Mesh, Dictionary<Load, int[]>> loadMeshEntityMap, 
                                        Dictionary<Mesh, Dictionary<GeometryRestrain, int[]>> restrainMeshEntityMap)
        {



        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param> 
        /// <param name="plateProperty"></param>
        /// <param name="brickProperty"></param>
        /// <param name="loadMeshEntityMap"></param>
        /// <param name="restrainMeshEntityMap">Map between IGeometryRestrain and MeshVertex.Id</param>
        public virtual void AddMesh(Mesh mesh, IPlateProperty plateProperty, IBrickProperty brickProperty, Dictionary<Load, int[]> loadMeshEntityMap, Dictionary<GeometryRestrain, int[]> restrainMeshEntityMap )
        {
            Dictionary<int, int> nodesNewIndexMap = new Dictionary<int, int>(); // Mappa tra indici dei nodi dentro _nodes e indici dei vertici della mesh nel caso esistano già dentro _nodes.
            Dictionary<int, int> platesNewIndexMap = new Dictionary<int, int>(); // Mappa tra indici dei nodi dentro _nodes e indici dei vertici della mesh nel caso esistano già dentro _nodes.


            // Aggiorno lista proprietà
            if (plateProperty is null && mesh.Faces.Count == 0)
                throw new ArgumentNullException(nameof(plateProperty));
            else
            {
                if (!_plateProperties.ContainsKey(plateProperty))
                    _plateProperties[plateProperty] = _plateProperties.Values.Max() + 1;
            }


            if (brickProperty is null && mesh.Faces.Count == 0)
                throw new ArgumentNullException(nameof(brickProperty));
            else
            {
                if (!_brickProperties.ContainsKey(brickProperty))
                    _brickProperties[brickProperty] = _brickProperties.Values.Max() + 1;
            }


            // Aggiunge nodi alla collection di nodi
            foreach(var vertex in mesh.Vertices)
            {
                var nodeIndex = _nodes.Add(new Node(vertex.Point, vertex.Id));

                if (nodeIndex != vertex.Id) // Se sono diversi vuol dire che esisteva già l'indice Vertex.iD e la collection l'ha modificato
                {
                    nodesNewIndexMap[nodeIndex] = vertex.Id;
                }
            }

            // Aggiunge elementi FEM

            foreach(var face in mesh.Faces)
            {
                if (face.IsQuad)
                {
                    if (plateProperty is PlateProperty pp)
                    {
                        _elements.Add(new Plate(new Node[] { _nodes[nodesNewIndexMap.ContainsKey(face.A) ? nodesNewIndexMap[face.A] : face.A],
                                                             _nodes[nodesNewIndexMap.ContainsKey(face.B) ? nodesNewIndexMap[face.B] : face.B],
                                                             _nodes[nodesNewIndexMap.ContainsKey(face.C) ? nodesNewIndexMap[face.C] : face.C],
                                                             _nodes[nodesNewIndexMap.ContainsKey(face.D) ? nodesNewIndexMap[face.D] : face.D]},
                                                             pp, face.Id));
                    }
                    else
                        throw new NotImplementedException();
                }
                else
                {
                    if (plateProperty is PlateProperty pp)
                    {
                        _elements.Add(new Plate(new Node[] { _nodes[nodesNewIndexMap.ContainsKey(face.A) ? nodesNewIndexMap[face.A] : face.A],
                                                             _nodes[nodesNewIndexMap.ContainsKey(face.B) ? nodesNewIndexMap[face.B] : face.B],
                                                             _nodes[nodesNewIndexMap.ContainsKey(face.C) ? nodesNewIndexMap[face.C] : face.C]},
                                                             pp, face.Id));
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
                        _elements.Add(new Brick(new Node[] { _nodes[nodesNewIndexMap.ContainsKey(volume.A) ? nodesNewIndexMap[volume.A] : volume.A],
                                                             _nodes[nodesNewIndexMap.ContainsKey(volume.B) ? nodesNewIndexMap[volume.B] : volume.B],
                                                             _nodes[nodesNewIndexMap.ContainsKey(volume.C) ? nodesNewIndexMap[volume.C] : volume.C],
                                                             _nodes[nodesNewIndexMap.ContainsKey(volume.D) ? nodesNewIndexMap[volume.D] : volume.D],
                                                             _nodes[nodesNewIndexMap.ContainsKey(volume.E) ? nodesNewIndexMap[volume.E] : volume.E],
                                                             _nodes[nodesNewIndexMap.ContainsKey(volume.F) ? nodesNewIndexMap[volume.F] : volume.F],
                                                             _nodes[nodesNewIndexMap.ContainsKey(volume.G) ? nodesNewIndexMap[volume.G] : volume.G],
                                                             _nodes[nodesNewIndexMap.ContainsKey(volume.H) ? nodesNewIndexMap[volume.H] : volume.H]},
                                                             bp, volume.Id));
                    }
                    else
                        throw new NotImplementedException();
                }
                else
                {
                    if (brickProperty is BrickProperty bp)
                    {
                        _elements.Add(new Brick(new Node[] { _nodes[nodesNewIndexMap.ContainsKey(volume.A) ? nodesNewIndexMap[volume.A] : volume.A],
                                                             _nodes[nodesNewIndexMap.ContainsKey(volume.B) ? nodesNewIndexMap[volume.B] : volume.B],
                                                             _nodes[nodesNewIndexMap.ContainsKey(volume.C) ? nodesNewIndexMap[volume.C] : volume.C],
                                                             _nodes[nodesNewIndexMap.ContainsKey(volume.D) ? nodesNewIndexMap[volume.D] : volume.D],
                                                             _nodes[nodesNewIndexMap.ContainsKey(volume.E) ? nodesNewIndexMap[volume.E] : volume.E],
                                                             _nodes[nodesNewIndexMap.ContainsKey(volume.F) ? nodesNewIndexMap[volume.F] : volume.F]},
                                                             bp, volume.Id));
                    }
                    else
                        throw new NotImplementedException();
                }

            }

            //foreach(var kvp in restrainMeshEntityMap)
            //{
            //    IGeometryRestrain geometryRestrain = kvp.Key;
            //    int[] indexes = kvp.Value;

            //    foreach (var index in indexes)
            //    {
            //        Node node = _nodes.GetElementById(index); // se non trova l'indice viene lanciata una keynotfoundException

            //        if (geometryRestrain is PointRestrain pr)
            //        {
            //            pr.fr
            //            node.AddAttribute(new NodeRestrainAttribute());
            //        }


            //    }
            //}


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
