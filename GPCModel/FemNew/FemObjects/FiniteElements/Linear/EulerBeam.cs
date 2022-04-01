using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem.Attributes;
using GPC.Model.Fem.ElementStiffnessMatrices;
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

        public override FiniteElement Duplicate(ElementProperty property, List<LoadCaseAttribute> lcAttributes, List<FreedomCaseAttribute> fcAttributes)
        {
            throw new NotImplementedException();
        }

        public override FiniteElement Duplicate()
        {
            throw new NotImplementedException();
        }

        public override TransformationMatrix GetTransformationMatrix(NodalGlobalDegreeOfFreedom[] nodalDegreeOfFreedoms)
        {

            TransformationMatrix matrix = new TransformationMatrix(nodalDegreeOfFreedoms);

            if (nodalDegreeOfFreedoms[0].DegreeOfFreedom == DegreeOfFreedoms.GlobalDegreeOfFreedoms.DX &&
                nodalDegreeOfFreedoms[1].DegreeOfFreedom == DegreeOfFreedoms.GlobalDegreeOfFreedoms.DY &&
                nodalDegreeOfFreedoms[2].DegreeOfFreedom == DegreeOfFreedoms.GlobalDegreeOfFreedoms.DZ &&
                nodalDegreeOfFreedoms[6].DegreeOfFreedom == DegreeOfFreedoms.GlobalDegreeOfFreedoms.DX &&
                nodalDegreeOfFreedoms[7].DegreeOfFreedom == DegreeOfFreedoms.GlobalDegreeOfFreedoms.DY &&
                nodalDegreeOfFreedoms[8].DegreeOfFreedom == DegreeOfFreedoms.GlobalDegreeOfFreedoms.DZ)
            {
                var submatrix = _coordinateSystem.TrfMatrix.RemoveColumn(3);
                matrix.SetSubMatrix(0, 0, submatrix);
                matrix.SetSubMatrix(6, 6, submatrix);
            }
            else
            {
                // Bisogna editare la trfMatrix se l'ordine dei dof è scambiato
                throw new NotImplementedException();
            }


            if (nodalDegreeOfFreedoms[3].DegreeOfFreedom == DegreeOfFreedoms.GlobalDegreeOfFreedoms.RX &&
                nodalDegreeOfFreedoms[4].DegreeOfFreedom == DegreeOfFreedoms.GlobalDegreeOfFreedoms.RY &&
                nodalDegreeOfFreedoms[5].DegreeOfFreedom == DegreeOfFreedoms.GlobalDegreeOfFreedoms.RZ &&
                nodalDegreeOfFreedoms[9].DegreeOfFreedom == DegreeOfFreedoms.GlobalDegreeOfFreedoms.RX &&
                nodalDegreeOfFreedoms[10].DegreeOfFreedom == DegreeOfFreedoms.GlobalDegreeOfFreedoms.RY &&
                nodalDegreeOfFreedoms[11].DegreeOfFreedom == DegreeOfFreedoms.GlobalDegreeOfFreedoms.RZ)
            {
                var submatrix = _coordinateSystem.TrfMatrix.RemoveColumn(3);
                matrix.SetSubMatrix(3, 3, submatrix);
                matrix.SetSubMatrix(9, 9, submatrix);
            }
            else
            {
                // Bisogna editare la trfMatrix se l'ordine dei dof è scambiato
                throw new NotImplementedException();
            }

            return matrix;
        }


        protected internal override ElementStiffnessMatrix GetGlobalStiffnessMatrix()
        {
            EulerBernulliStifnessMatrix eulerBernulliLocalStifnessMatrix = new EulerBernulliStifnessMatrix(this);

            NodalGlobalDegreeOfFreedom[] nodalGlobalDegreeOfFreedom = eulerBernulliLocalStifnessMatrix.NodalDegreeOfFreedom.ToGlobal();

            ElementStiffnessMatrix globalStiffnessMatrix = new ElementStiffnessMatrix(nodalGlobalDegreeOfFreedom, eulerBernulliLocalStifnessMatrix.LocalStiffnessMatrix);

            globalStiffnessMatrix = globalStiffnessMatrix.PrePostMultiply(GetTransformationMatrix(nodalGlobalDegreeOfFreedom));


            return globalStiffnessMatrix;
        }

    }
}
