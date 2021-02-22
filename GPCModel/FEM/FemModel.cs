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

        private Dictionary<IPlateProperty, int> _plateProperties;

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

        public void AddMesh(Mesh mesh, IPlateProperty property)
        {

        }

        #endregion



    }
}
