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
using MathNet.Spatial.Units;
using System.IO;

namespace GPC.Model.FEM
{
    public class Beam : FEMElement
    {
        #region Variables
        protected Node _node1;
        protected Node _node2;
        protected Section _section;
        #endregion

        #region Properties

        public Node Node1 => _node1;

        public Node Node2 => _node2;

        public double Length => Node1.Position.DistanceTo(Node2.Position);
        
        #endregion

        #region Public Constructors
        public Beam(Guid guid, Section section, Node[] nodes)
            : base(guid, section)
        {
            _guid = guid;
            SetElement(nodes);
            SetLocalCoordinateSystem(0.0);
            _section = section;
            _integrator = new FEMBeamIntegrator(new Guid(), this);
            BuildElementDoF();
            _integrator.StartIntegration(this);
        }
        public override void TInGlobal()
        {
        }
        public override void FInGlobal()
        {
        }
        public override void ChooseIntegrator()
        {
        }
        #endregion

        #region Public Methods Specific
        #endregion

        #region Private Methods Specific
        protected override void SetElement(Node[] arrayNode)
        {
            _node1 = arrayNode[0];
            _node2 = arrayNode[1];
            _nodesGlobal = new Node[arrayNode.Length];
            _nodesLocal = new Node[arrayNode.Length];
            _nodesGlobal = arrayNode;
        }


        protected override void SetLocalCoordinateSystem(double rotationAngle)
        {
            Vector3d ZAxis = new Vector3d(0, 0, 1);
            Vector3d v11 = new Vector3d(1, 0, 0);
            Vector3d v22 = new Vector3d(0, 1, 0);
            Vector3d v33 = new Vector3d((_node2.Position.X - _node1.Position.X), (_node2.Position.Y - _node1.Position.Y), (_node2.Position.Z - _node1.Position.Z));
            double checkVert = ZAxis.CrossProduct(v33).Length;

            if (checkVert < 1.0E-12)
            {
                v11 = new Vector3d(0, 1, 0);
                v22 = new Vector3d(0, 1, 0);
                v33 = new Vector3d(0, 0, 1);

                v11.Unitize();
                v22.Unitize();
                v33.Unitize();
            }
            else
            {
                v11 = ZAxis.CrossProduct(v33);
                v22 = v33.CrossProduct(v11);
                v11.Unitize();
                v22.Unitize();
                v33.Unitize();
            }

            _coordSys = new CoordinateSystem(_nodesGlobal[0].Position, v33, v22, v11, rotationAngle, string.Empty, Guid.Empty);

            for (int nd = 0; nd < _nodesGlobal.Length; nd++)
            {
                var p = _coordSys.PointToLocal(_nodesGlobal[nd].Position);
                _nodesLocal[nd] = new Node(new Guid(), p, _nodesGlobal[nd].NodeIndex, _nodesGlobal[nd].DoF);
            }        
        }
        #endregion
    }
}
