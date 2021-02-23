using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry.Meshes;
using GPC.Model.Elements;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.FEM.Properties;
using GPC.Model.LoadCases;
using GPC.Model.Loads;

namespace GPC.Model.FEM
{
    public class FemModel : ModelObject
    {

        #region Variables
          
        private List<Node> _nodes;

        private List<Plate> _plates;

        private List<Brick> _bricks;

        private Dictionary<IPlateProperty, int> _plateProperties;
        private Dictionary<IBrickProperty, int> _brickProperties;

        private Dictionary<LoadCase, int> _loadCases;

        private Dictionary<IGeometryRestrain, int> _geometryRestrain;

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
            _nodes = new List<Node>();
            _plates = new List<Plate>();
            _bricks = new List<Brick>();

            _plateProperties = new Dictionary<IPlateProperty, int>();
            _loadCases = new Dictionary<LoadCase, int>();

            _geometryRestrain = new Dictionary<IGeometryRestrain, int>();
            _loads = new List<Load>();
        }

        public FemModel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            throw new NotImplementedException();
        }

        #endregion

        #region Public methods
        
        public virtual void AddShape()
        {
            throw new NotImplementedException();
        }


        public virtual void AddMesh(Mesh mesh, IPlateProperty plateProperty, IBrickProperty brickProperty)
        {

            throw new NotImplementedException();

            Dictionary<int, Node> _nodesIndex = new Dictionary<int, Node>();
            _nodesIndex = _nodes.ToDictionary(i => i.Index);

            Dictionary<int, int> newIndexMap = new Dictionary<int, int>(); // Mappa tra indici dei nodi e indici dei vertici della mesh nel caso esistano già dentro _nodes.


            if (!_plateProperties.ContainsKey(plateProperty))
                _plateProperties[plateProperty] = _plateProperties.Values.Max() + 1;

            if (!_brickProperties.ContainsKey(brickProperty))
                _brickProperties[brickProperty] = _brickProperties.Values.Max() + 1;


            foreach (var vertex in mesh.Vertices)
            {
                if (_nodesIndex.ContainsKey(vertex.Id))
                {
                    var id = _nodesIndex.Keys.Max();

                    _nodes.Add(new Node(vertex.Point, id));

                    _nodesIndex.Add(id, _nodes.Last());
                    newIndexMap[id] = vertex.Id;
                }
                else
                {
                    _nodes.Add(new Node(vertex.Point, vertex.Id));
                    _nodesIndex.Add(vertex.Id, _nodes.Last());
                }
            }

            // TODO: gestire il tipo di elemento finito
            foreach(var face in mesh.Faces)
            {
                if (plateProperty is null)
                    throw new ArgumentNullException(nameof(plateProperty));

                if (face.IsQuad)
                {
                    if (plateProperty is PlateProperty pp)
                    {
                        _plates.Add(new Plate(new Node[] { _nodesIndex[newIndexMap.ContainsKey(face.A) ? newIndexMap[face.A] : face.A],
                                                           _nodesIndex[newIndexMap.ContainsKey(face.B) ? newIndexMap[face.B] : face.B],
                                                           _nodesIndex[newIndexMap.ContainsKey(face.C) ? newIndexMap[face.C] : face.C],
                                                           _nodesIndex[newIndexMap.ContainsKey(face.D) ? newIndexMap[face.D] : face.D]}, 
                                                           pp, face.Id));
                    }
                    else
                        throw new NotImplementedException();
                }
                else
                {
                    if (plateProperty is PlateProperty pp)
                    {
                        _plates.Add(new Plate(new Node[] { _nodesIndex[newIndexMap.ContainsKey(face.A) ? newIndexMap[face.A] : face.A],
                                                           _nodesIndex[newIndexMap.ContainsKey(face.B) ? newIndexMap[face.B] : face.B],
                                                           _nodesIndex[newIndexMap.ContainsKey(face.C) ? newIndexMap[face.C] : face.C]},
                                                           pp, face.Id));
                    }
                    else
                        throw new NotImplementedException();
                }

            }

            // TODO: gestire il tipo di elemento finito
            foreach (var volume in mesh.Volumes)
            {
                if (brickProperty is null)
                    throw new ArgumentNullException(nameof(brickProperty));

                if (volume.IsQuadrangular)
                {
                    if (brickProperty is BrickProperty bp)
                    {
                        _bricks.Add(new Brick(new Node[] { _nodesIndex[newIndexMap.ContainsKey(volume.A) ? newIndexMap[volume.A] : volume.A],
                                                           _nodesIndex[newIndexMap.ContainsKey(volume.B) ? newIndexMap[volume.B] : volume.B],
                                                           _nodesIndex[newIndexMap.ContainsKey(volume.C) ? newIndexMap[volume.C] : volume.C],
                                                           _nodesIndex[newIndexMap.ContainsKey(volume.D) ? newIndexMap[volume.D] : volume.D],
                                                           _nodesIndex[newIndexMap.ContainsKey(volume.E) ? newIndexMap[volume.E] : volume.E],
                                                           _nodesIndex[newIndexMap.ContainsKey(volume.F) ? newIndexMap[volume.F] : volume.F],
                                                           _nodesIndex[newIndexMap.ContainsKey(volume.G) ? newIndexMap[volume.G] : volume.G],
                                                           _nodesIndex[newIndexMap.ContainsKey(volume.H) ? newIndexMap[volume.H] : volume.H]},
                                                           bp, volume.Id));
                    }
                    else
                        throw new NotImplementedException();
                }
                else
                {
                    if (brickProperty is BrickProperty bp)
                    {
                        _bricks.Add(new Brick(new Node[] { _nodesIndex[newIndexMap.ContainsKey(volume.A) ? newIndexMap[volume.A] : volume.A],
                                                           _nodesIndex[newIndexMap.ContainsKey(volume.B) ? newIndexMap[volume.B] : volume.B],
                                                           _nodesIndex[newIndexMap.ContainsKey(volume.C) ? newIndexMap[volume.C] : volume.C],
                                                           _nodesIndex[newIndexMap.ContainsKey(volume.D) ? newIndexMap[volume.D] : volume.D],
                                                           _nodesIndex[newIndexMap.ContainsKey(volume.E) ? newIndexMap[volume.E] : volume.E],
                                                           _nodesIndex[newIndexMap.ContainsKey(volume.F) ? newIndexMap[volume.F] : volume.F]},
                                                           bp, volume.Id));                    
                    }
                    else
                        throw new NotImplementedException();
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



        #endregion



    }
}
