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
using GPC.Model.Elements;
using GPC.Model.CoordinateSystems;

namespace GPC.Model.FEM
{
    public class Plate : FEMElement
    {
        #region Variables

        protected PlateProperty _property;
        protected PlateAnalysisType _analysisType;
        #endregion

        #region Properties
        public PlateProperty Property => _property;
        public PlateAnalysisType AnalysisType => _analysisType;
        #endregion

        #region Public Constructors
        public Plate(Guid guid, PlateProperty property, Node[] nodes)
            : base(guid)
        {
            _guid = guid;
            SetElement(nodes);
            SetLocalCoordinateSystem(0.0);
            _property = property;
            _integrator = null;
        }
        public Plate(Guid guid, PlateProperty property, FEMPlateIntegrator integrator, Node[] nodes)
            : base(guid, integrator)
        {
            _guid = guid;
            SetElement(nodes);
            SetLocalCoordinateSystem(0.0);
            _property = property;
            _integrator = integrator;
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
            _nodesGlobal = new Node[arrayNode.Length];
            _nodesLocal = new Node[arrayNode.Length];
            _nodesGlobal = arrayNode;
        }
        protected override void SetLocalCoordinateSystem(double rotationAngle)
        {
            _coordSys = new GPC.Model.CoordinateSystems.CoordinateSystem(Guid.Empty, _nodesGlobal[0].Position, _nodesGlobal[1].Position, _nodesGlobal[2].Position, rotationAngle);
            for (int nd = 0; nd < _nodesGlobal.Length; nd++)
            {
                Point3d localPoint = _coordSys.PointToLocal(_nodesGlobal[nd].Position);
                _nodesLocal[nd] = new Node(new Guid(), localPoint, _nodesGlobal[nd].NodeIndex, _nodesGlobal[nd].DoF);
            }
        }
        #endregion
    }
}
