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
using GPC.Model.CoordinateSystem;

namespace GPC.Model.FEM
{
    public class Plate : FEMElement
    {
        #region Variables
        protected CoordinateSystemPlate _CoordSys;
        protected PlateProperty _property;
        #endregion 

        #region Properties
        public PlateProperty Property => _property;
        #endregion

        #region Public Constructors
        public Plate(Guid guid, PlateProperty property, Node[] nodes)
            : base(guid)
        {
            _guid = guid;
            SetElement(nodes);
            _property = property;
            _integrator = null;
            SetLocalCoordinateSystem(0.0);
        }
        public Plate(Guid guid, PlateProperty property, FEMPlateIntegrator integrator, Node[] nodes)
            : base(guid, integrator)
        {
            _guid = guid;
            SetElement(nodes);
            _property = property;
            _integrator = integrator;
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
            _nodesGlobal = new Node[arrayNode.Length];
            _nodesGlobal = arrayNode;
        }
        protected override void SetLocalCoordinateSystem(double rotationAngle)
        {

            Vector3d ZAxis = new Vector3d(0, 0, 1);
            Vector3d v11 = new Vector3d(1, 0, 0);
            Vector3d v22 = new Vector3d(0, 1, 0);
        }
        #endregion
    }
}
