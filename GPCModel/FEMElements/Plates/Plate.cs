using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Materials;
using GPC.Model.Sections;
using MathNet.Numerics.LinearAlgebra;
using GPC.Geometry;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using System.IO;

namespace GPC.Model.FEM
{
    public class Plate : FEMElement
    {

        #region Variables
        protected Node[] _nodes;
        protected Section _section;
        protected Material _material;
        protected FEMIntegrator _Integrator;
        protected GPC.Geometry.CoordinateSystem _CoordSys;

        #endregion

        #region Properties

        public Node[] Nodes => _nodes;

        public Section Section => _section;

        public Material Material => _material;

        #endregion

        #region Public Constructors
        public Plate(Guid guid, Section section, Material material, FEMIntegrator integrator, Node[] nodes)
            : base(guid, integrator)
        {
            _guid = guid;
            SetElement(nodes);
            _section = section;
            _material = material;
            _Integrator = integrator;
            SetLocalCoordinateSystem(0.0);
        }

        protected Plate(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        #region Public Methods Override
        public override void ElementIncidence()
        {
        }
        protected override void SetElement(Node[] arrayNode)
        {
            _nodes = new Node[arrayNode.Length];
            _nodes = arrayNode;
        }
        protected override void SetLocalCoordinateSystem(double rotationAngle)
        {
        }
        #endregion
    }
}
