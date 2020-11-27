using GPC.Geometry;
using GPC.Model.Elements;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.FEM
{
    public class Plate : FEMElement
    {
        #region Variables

        protected PlateProperty _property;

        protected PlateAnalysisType _analysisType;
        protected double _A;
        #endregion

        #region Properties

        public PlateProperty Property => _property;

        public PlateAnalysisType AnalysisType => _analysisType;
        public double A => _A;
        #endregion


        #region Properties

        #endregion

        #region Public Contructors
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
            BuildElementDoF();
            _integrator.StartIntegration(this);
        }

        protected Plate(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion 

        #region Public Methods Override

        protected override void SetElement(Node[] arrayNode)
        {
            _nodesGlobal = new Node[arrayNode.Length];
            _nodesLocal = new Node[arrayNode.Length];
            _nodesGlobal = arrayNode;
        }

        protected override void SetLocalCoordinateSystem(double rotationAngle)
        {
            _coordSys = new CoordinateSystem(_nodesGlobal[0].Position, _nodesGlobal[1].Position, _nodesGlobal[2].Position, rotationAngle, string.Empty, Guid.Empty);
            for (int nd = 0; nd < _nodesGlobal.Length; nd++)
            {
                Point3d localPoint = _coordSys.PointToLocal(_nodesGlobal[nd].Position);
                _nodesLocal[nd] = new Node(new Guid(), localPoint, _nodesGlobal[nd].NodeIndex, _nodesGlobal[nd].DoF);
            }
        }

        public virtual void CalcArea(double A)
        {
            _A = A;
        }
        #endregion
    }
}