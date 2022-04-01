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

namespace GPC.Model.Fem.ElementStiffnessMatrices
{

    internal abstract class FiniteElementLocalStiffnessMatrix : ModelObjectId
    {

        protected Node[] _localNodes;

        private readonly ElementStiffnessMatrix _localStiffnessMatrix;

        protected NodalLocalDegreeOfFreedom[] _nodalDegreeOfFreedoms;


        public ElementStiffnessMatrix LocalStiffnessMatrix => _localStiffnessMatrix;

        public NodalLocalDegreeOfFreedom[] NodalDegreeOfFreedom => _nodalDegreeOfFreedoms;


        public FiniteElementLocalStiffnessMatrix(Node[] localNodes, FiniteElement element)
        {
            _localNodes = localNodes;

            _localStiffnessMatrix = GetStiffnessMatrix(element);

            _nodalDegreeOfFreedoms = GetNodalDegreeOfFreedom();
        }

        protected abstract NodalLocalDegreeOfFreedom[] GetNodalDegreeOfFreedom();
        public abstract NodalGlobalDegreeOfFreedom[] GetGlobalDegreeOfFreedom(Node[] nodes);

        protected abstract ElementStiffnessMatrix GetStiffnessMatrix(FiniteElement element);


    }
}
