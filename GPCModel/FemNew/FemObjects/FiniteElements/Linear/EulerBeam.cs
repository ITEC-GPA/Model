using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem.Attributes;
using GPC.Model.Fem.Properties;
using GPC.Model.Fem.StiffnessMatrix;

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

        public override FiniteElement Duplicate(ElementProperty property, List<LoadCaseAttribute> lcAttributes, List<FreedomCaseAttribute> fcAttributes)
        {
            throw new NotImplementedException();
        }

        public override FiniteElement Duplicate()
        {
            throw new NotImplementedException();
        }

        public override TransformationMatrix GetTransformationMatrix(NodalLocalDegreeOfFreedom[] nodalDegreeOfFreedoms)
        {

            for (int i = 0; i < nodalDegreeOfFreedoms.Length; i++)
            {

            }




            TransformationMatrix matrix = new TransformationMatrix(nodalDegreeOfFreedoms);

            var submatrix = _coordinateSystem.TrfMatrix.RemoveColumn(3);

            matrix.SetSubMatrix(0, 0, submatrix);
            matrix.SetSubMatrix(3, 3, submatrix);
            matrix.SetSubMatrix(6, 6, submatrix);
            matrix.SetSubMatrix(9, 9, submatrix);

            return matrix;
        }


        private protected override ElementStiffnessMatrix GetGlobalStiffnessMatrix()
        {
            EulerBernulliStifnessMatrix eulerBernulliLocalStifnessMatrix = new EulerBernulliStifnessMatrix(this);


            ElementStiffnessMatrix localStiffnessMatrix = eulerBernulliLocalStifnessMatrix.LocalStiffnessMatrix;
            NodalLocalDegreeOfFreedom[] nodalDegreeOfFreedom = eulerBernulliLocalStifnessMatrix.NodalDegreeOfFreedom;
            TransformationMatrix transformationMatrix = GetTransformationMatrix(nodalDegreeOfFreedom);




            throw new NotImplementedException();
        }

    }
}
