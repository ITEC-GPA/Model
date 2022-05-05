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

        /// <inheritdoc/>
        public EulerBeam(Node node1, Node node2, double angle = 0)
            : base(node1, node2, angle)
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
            if (nodalDegreeOfFreedoms[0].DegreeOfFreedom.DegreeOfFreedomGlobalDirection == GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X &&
                nodalDegreeOfFreedoms[1].DegreeOfFreedom.DegreeOfFreedomGlobalDirection == GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y &&
                nodalDegreeOfFreedoms[2].DegreeOfFreedom.DegreeOfFreedomGlobalDirection == GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z &&
                nodalDegreeOfFreedoms[3].DegreeOfFreedom.DegreeOfFreedomGlobalDirection == GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X &&
                nodalDegreeOfFreedoms[4].DegreeOfFreedom.DegreeOfFreedomGlobalDirection == GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y &&
                nodalDegreeOfFreedoms[5].DegreeOfFreedom.DegreeOfFreedomGlobalDirection == GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z)
            {

                if (nodalDegreeOfFreedoms[0].DegreeOfFreedom.DegreeOfFreedomGlobalDirection == GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X &&
                    nodalDegreeOfFreedoms[1].DegreeOfFreedom.DegreeOfFreedomGlobalDirection == GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y &&
                    nodalDegreeOfFreedoms[2].DegreeOfFreedom.DegreeOfFreedomGlobalDirection == GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z &&
                    nodalDegreeOfFreedoms[3].DegreeOfFreedom.DegreeOfFreedomGlobalDirection == GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.X &&
                    nodalDegreeOfFreedoms[4].DegreeOfFreedom.DegreeOfFreedomGlobalDirection == GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Y &&
                    nodalDegreeOfFreedoms[5].DegreeOfFreedom.DegreeOfFreedomGlobalDirection == GlobalDegreeOfFreedom.DegreeOfFreedomGlobalDirections.Z)
                {
                    return new TransformationMatrix(nodalDegreeOfFreedoms, _coordinateSystem);
                }
                else
                {
                    // Bisogna editare la trfMatrix se l'ordine dei dof è scambiato
                    throw new NotImplementedException();
                }
            }
            else
            {
                // Bisogna editare la trfMatrix se l'ordine dei dof è scambiato
                throw new NotImplementedException();
            }


        }


        protected internal override ElementStiffnessMatrix GetGlobalStiffnessMatrix()
        {
            EulerBernulliStifnessMatrix eulerBernulliLocalStifnessMatrix = new EulerBernulliStifnessMatrix(this);

            NodalGlobalDegreeOfFreedom[] nodalGlobalDegreeOfFreedom = eulerBernulliLocalStifnessMatrix.GetGlobalDegreeOfFreedom(NodeStart, NodeEnd);
            
            ElementStiffnessMatrix globalStiffnessMatrix = new ElementStiffnessMatrix(nodalGlobalDegreeOfFreedom, eulerBernulliLocalStifnessMatrix.StiffnessMatrix);

            globalStiffnessMatrix = globalStiffnessMatrix.PrePostMultiply(GetTransformationMatrix(nodalGlobalDegreeOfFreedom));


            return globalStiffnessMatrix;
        }

    }
}
