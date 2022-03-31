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
    public class Truss : LinearElement
    {
        public Truss(Node node1, Node node2)
            : base(node1, node2)
        {

        }

        protected Truss(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        internal override NodalDegreeOfFreedom[] GetNodalDegreeOfFreedom()
        {

            NodalDegreeOfFreedom[] nodalDegreeOfFreedoms = new NodalDegreeOfFreedom[6];
            nodalDegreeOfFreedoms[0] = new NodalDegreeOfFreedom(NodeStart, DegreeOfFreedom.DX);
            nodalDegreeOfFreedoms[1] = new NodalDegreeOfFreedom(NodeStart, DegreeOfFreedom.DY);
            nodalDegreeOfFreedoms[2] = new NodalDegreeOfFreedom(NodeStart, DegreeOfFreedom.DZ);

            nodalDegreeOfFreedoms[3] = new NodalDegreeOfFreedom(NodeEnd, DegreeOfFreedom.DX);
            nodalDegreeOfFreedoms[4] = new NodalDegreeOfFreedom(NodeEnd, DegreeOfFreedom.DY);
            nodalDegreeOfFreedoms[5] = new NodalDegreeOfFreedom(NodeEnd, DegreeOfFreedom.DZ);


            return nodalDegreeOfFreedoms;
        }

        public override FiniteElement Duplicate(ElementProperty property, List<LoadCaseAttribute> lcAttributes, List<FreedomCaseAttribute> fcAttributes)
        {
            throw new NotImplementedException();
        }

        public override FiniteElement Duplicate()
        {
            throw new NotImplementedException();
        }

        public override TransformationMatrix GetTransformationMatrix()
        {
            TransformationMatrix matrix = new TransformationMatrix(GetNodalDegreeOfFreedom());

            var submatrix = _localCoordinateSystem.TrfMatrix.RemoveColumn(3);

            matrix.SetSubMatrix(0, 0, submatrix);
            matrix.SetSubMatrix(3, 3, submatrix);

            return matrix;
        }
    }
}
