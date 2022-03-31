using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Maths.Matrices;
using GPC.Model.Fem.FemObjects;
using GPC.Model.Sections;
using GPC.Model.Fem.FemObjects.FiniteElements;
using GPC.Model.Fem.Materials;

namespace GPC.Model.Fem.StiffnessMatrix
{

    internal abstract class FiniteElementLocalStiffnessMatrix : ModelObjectId
    {

        protected Node[] _localNodes;

        private readonly ElementStiffnessMatrix _localStiffnessMatrix;


        public ElementStiffnessMatrix LocalStiffnessMatrix => _localStiffnessMatrix;

        public FiniteElementLocalStiffnessMatrix(Node[] localNodes, EulerBeam beam)
        {
            _localNodes = localNodes;

            _localStiffnessMatrix = GetStiffnessMatrix(beam);
        }

        internal abstract NodalDegreeOfFreedom[] GetNodalDegreeOfFreedom();

        protected abstract ElementStiffnessMatrix GetStiffnessMatrix(FiniteElement element);


    }

}
