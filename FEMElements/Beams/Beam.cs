using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM
{
    public class Beam : FEMElement
    {
        #region Variables
        protected Node _node1;
        protected Node _node2;
        #endregion

        #region Properties
        protected Node Node1 => _node1;
        protected Node Node2 => _node2;
        protected double Length => Node1.Position.DistanceTo(Node2.Position);
        #endregion

        #region Public Constructors
        public Beam(Guid guid, FEMIntegrator integrator, Node node1, Node node2)
            : base(guid, integrator)
        {
            _node1 = node1;
            _node2 = node2;
        }

        protected Beam(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Public Methods Override
        public override void ChooseIntegrator()
        {
        }
        #endregion

        #region Public Methods Specific
        #endregion

        #region Private Methods Specific
        #endregion
    }
}
