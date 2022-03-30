using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem.Attributes;
using GPC.Model.Fem.Properties;

namespace GPC.Model.Fem.FemObjects.FiniteElements
{
    public class EulerBeam : LinearElement
    {

        public EulerBeam(Node node1, Node node2)
            : base(node1, node2)
        {

        }

        protected EulerBeam(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }


        protected override NodalDegreeOfFreedom[] GetNodalDegreeOfFreedom()
        {
            NodalDegreeOfFreedom[] nodalDegreeOfFreedoms = new NodalDegreeOfFreedom[12];
            nodalDegreeOfFreedoms[0] = new NodalDegreeOfFreedom(NodeStart, DegreeOfFreedom.DX);
            nodalDegreeOfFreedoms[1] = new NodalDegreeOfFreedom(NodeStart, DegreeOfFreedom.DY);
            nodalDegreeOfFreedoms[2] = new NodalDegreeOfFreedom(NodeStart, DegreeOfFreedom.DZ);
            nodalDegreeOfFreedoms[3] = new NodalDegreeOfFreedom(NodeStart, DegreeOfFreedom.RX);
            nodalDegreeOfFreedoms[4] = new NodalDegreeOfFreedom(NodeStart, DegreeOfFreedom.RY);
            nodalDegreeOfFreedoms[5] = new NodalDegreeOfFreedom(NodeStart, DegreeOfFreedom.RZ);

            nodalDegreeOfFreedoms[6] = new NodalDegreeOfFreedom(NodeEnd, DegreeOfFreedom.DX);
            nodalDegreeOfFreedoms[7] = new NodalDegreeOfFreedom(NodeEnd, DegreeOfFreedom.DY);
            nodalDegreeOfFreedoms[8] = new NodalDegreeOfFreedom(NodeEnd, DegreeOfFreedom.DZ);
            nodalDegreeOfFreedoms[9] = new NodalDegreeOfFreedom(NodeEnd, DegreeOfFreedom.RX);
            nodalDegreeOfFreedoms[10] = new NodalDegreeOfFreedom(NodeEnd, DegreeOfFreedom.RY);
            nodalDegreeOfFreedoms[11] = new NodalDegreeOfFreedom(NodeEnd, DegreeOfFreedom.RZ);


            return nodalDegreeOfFreedoms;
        }


        public override void BuildMatrix()
        {
            throw new NotImplementedException();
        }

        public override FiniteElement Duplicate(ElementProperty property, List<LoadCaseAttribute> lcAttributes, List<FreedomCaseAttribute> fcAttributes)
        {
            throw new NotImplementedException();
        }

        public override FiniteElement Duplicate()
        {
            throw new NotImplementedException();
        }
    }
}
