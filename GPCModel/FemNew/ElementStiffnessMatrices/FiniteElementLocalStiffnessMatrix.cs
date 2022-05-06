using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem.FemObjects;
using GPC.Model.Fem.FemObjects.FiniteElements;
using GPC.Model.Fem.Materials;
using GPC.Model.Maths.Matrices;
using GPC.Model.Sections;

namespace GPC.Model.Fem.ElementStiffnessMatrices
{

    internal abstract class FiniteElementLocalStiffnessMatrix : ModelObjectId
    {

        protected Node[] _nodes;

        private readonly ElementLocalStiffnessMatrix _stiffnessMatrix;

        protected NodalLocalDegreeOfFreedom[] _nodalDegreeOfFreedoms;


        public ElementLocalStiffnessMatrix StiffnessMatrix => _stiffnessMatrix;

        public NodalLocalDegreeOfFreedom[] NodalDegreeOfFreedom => _nodalDegreeOfFreedoms;


        public FiniteElementLocalStiffnessMatrix(Node[] nodes, FiniteElement element)
        {
            _nodes = nodes;

            _stiffnessMatrix = GetStiffnessMatrix(element);

            _nodalDegreeOfFreedoms = GetNodalDegreeOfFreedom();
        }

        protected abstract NodalLocalDegreeOfFreedom[] GetNodalDegreeOfFreedom();

        public abstract NodalGlobalDegreeOfFreedom[] GetGlobalDegreeOfFreedom(Node[] nodes);

        protected abstract ElementLocalStiffnessMatrix GetStiffnessMatrix(FiniteElement element);


    }
}
