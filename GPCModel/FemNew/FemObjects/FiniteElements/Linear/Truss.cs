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
    public class Truss : LinearElement
    {
        /// <inheritdoc/>
        public Truss(Node node1, Node node2, double angle = 0)
            : base(node1, node2, angle)
        {

        }

        protected Truss(SerializationInfo info, StreamingContext context)
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

            if (nodalDegreeOfFreedoms[0].DegreeOfFreedom.DegreeOfFreedomGlobalDirection == GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X &&
                nodalDegreeOfFreedoms[1].DegreeOfFreedom.DegreeOfFreedomGlobalDirection == GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z &&
                nodalDegreeOfFreedoms[2].DegreeOfFreedom.DegreeOfFreedomGlobalDirection == GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y &&
                nodalDegreeOfFreedoms[3].DegreeOfFreedom.DegreeOfFreedomGlobalDirection == GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X &&
                nodalDegreeOfFreedoms[4].DegreeOfFreedom.DegreeOfFreedomGlobalDirection == GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z &&
                nodalDegreeOfFreedoms[5].DegreeOfFreedom.DegreeOfFreedomGlobalDirection == GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y)
            {
                var submatrix = _coordinateSystem.TrfMatrix.RemoveColumn(3);
                matrix.SetSubMatrix(0, 0, submatrix);
                matrix.SetSubMatrix(3, 3, submatrix);
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
            TrussStiffnessMatrix trussStiffnessMatrix = new TrussStiffnessMatrix(this);

            NodalGlobalDegreeOfFreedom[] nodalGlobalDegreeOfFreedom = trussStiffnessMatrix.GetGlobalDegreeOfFreedom(NodeStart, NodeEnd);

            ElementStiffnessMatrix globalStiffnessMatrix = new ElementStiffnessMatrix(nodalGlobalDegreeOfFreedom, trussStiffnessMatrix.StiffnessMatrix);

            globalStiffnessMatrix = globalStiffnessMatrix.PrePostMultiply(GetTransformationMatrix(nodalGlobalDegreeOfFreedom));


            return globalStiffnessMatrix;
        }

    }
}
