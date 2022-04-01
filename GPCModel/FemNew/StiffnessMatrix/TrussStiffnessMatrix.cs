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
    internal class TrussStiffnessMatrix : FiniteElementLocalStiffnessMatrix
    {

        public Node NodeStart => _localNodes[0];
        public Node NodeEnd => _localNodes.Last();

        public TrussStiffnessMatrix(Node[] localNodes, EulerBeam beam) 
            : base(localNodes, beam)
        {

        }

        protected override ElementStiffnessMatrix GetStiffnessMatrix(FiniteElement element)
        {
            return GetStiffnessMatrix(element);
        }

        protected ElementStiffnessMatrix GetStiffnessMatrix(EulerBeam element)
        {
            Section section = element.Section;
            IsotropicFemMaterial material = section.GetIsotropicFemMaterial();


            double lenght = element.Length;
            double E = material.E;

            double A = section.Area;


            NodalLocalDegreeOfFreedom[] nodalDegree = GetNodalDegreeOfFreedom();

            ElementStiffnessMatrix matrix = new ElementStiffnessMatrix(nodalDegree);

            // Diagonale
            double ka = E * A / lenght;
            matrix.SetElementAt(nodalDegree[0], + ka);
            matrix.SetElementAt(nodalDegree[1], 1.0);
            matrix.SetElementAt(nodalDegree[2], 1.0);
            matrix.SetElementAt(nodalDegree[3], + ka);
            matrix.SetElementAt(nodalDegree[4], 1.0);
            matrix.SetElementAt(nodalDegree[5], 1.0);

            // Fuori Diagonale
            matrix.SetElementAtSymmetric(nodalDegree[3], nodalDegree[0], - ka);


            return matrix;
        }

        protected override NodalLocalDegreeOfFreedom[] GetNodalDegreeOfFreedom()
        {

            NodalLocalDegreeOfFreedom[] nodalDegreeOfFreedoms = new NodalLocalDegreeOfFreedom[6];
            nodalDegreeOfFreedoms[0] = new NodalLocalDegreeOfFreedom(NodeStart, DegreeOfFreedoms.LocalDegreeOfFreedoms.D1);
            nodalDegreeOfFreedoms[1] = new NodalLocalDegreeOfFreedom(NodeStart, DegreeOfFreedoms.LocalDegreeOfFreedoms.D2);
            nodalDegreeOfFreedoms[2] = new NodalLocalDegreeOfFreedom(NodeStart, DegreeOfFreedoms.LocalDegreeOfFreedoms.D3);

            nodalDegreeOfFreedoms[3] = new NodalLocalDegreeOfFreedom(NodeEnd, DegreeOfFreedoms.LocalDegreeOfFreedoms.D1);
            nodalDegreeOfFreedoms[4] = new NodalLocalDegreeOfFreedom(NodeEnd, DegreeOfFreedoms.LocalDegreeOfFreedoms.D2);
            nodalDegreeOfFreedoms[5] = new NodalLocalDegreeOfFreedom(NodeEnd, DegreeOfFreedoms.LocalDegreeOfFreedoms.D3);


            return nodalDegreeOfFreedoms;
        }
    }
}
