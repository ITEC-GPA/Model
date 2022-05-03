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
    internal class TrussStiffnessMatrix : FiniteElementLocalStiffnessMatrix
    {

        public Node NodeStart => _nodes[0];
        public Node NodeEnd => _nodes.Last();


        public TrussStiffnessMatrix(Truss truss)
            : base(new Node[] { new Node(new Geometry.Point3d(0, 0, 0), 1), new Node(new Geometry.Point3d(0, 0, truss.Length), 2) }, truss)
        {

        }


        protected override ElementStiffnessMatrix GetStiffnessMatrix(FiniteElement element)
        {
            return GetStiffnessMatrix((Truss)element);
        }

        protected ElementStiffnessMatrix GetStiffnessMatrix(Truss element)
        {
            Section section = element.Section;
            IsotropicFemMaterial material = section.GetIsotropicFemMaterial();


            double lenght = element.Length;
            double E = material.E;

            double A = section.Area;


            NodalLocalDegreeOfFreedom[] nodalDegree = GetNodalDegreeOfFreedom();

            ElementStiffnessMatrix matrix = new ElementStiffnessMatrix(nodalDegree);

            int axialDofIndex = 2;
            int totalDof = 3;


            // Diagonale
            double ka = E * A / lenght;
            matrix.SetElementAt(nodalDegree[axialDofIndex], + ka);
            matrix.SetElementAt(nodalDegree[axialDofIndex + totalDof], + ka);

            // Fuori Diagonale
            matrix.SetElementAtSymmetric(nodalDegree[axialDofIndex], nodalDegree[axialDofIndex + totalDof], - ka);

            Console.Write(matrix);
            return matrix;
        }

        protected override NodalLocalDegreeOfFreedom[] GetNodalDegreeOfFreedom()
        {

            NodalLocalDegreeOfFreedom[] nodalDegreeOfFreedoms = new NodalLocalDegreeOfFreedom[6];
            nodalDegreeOfFreedoms[0] = new NodalLocalDegreeOfFreedom(NodeStart, new DegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis3));
            nodalDegreeOfFreedoms[1] = new NodalLocalDegreeOfFreedom(NodeStart, new DegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis2));
            nodalDegreeOfFreedoms[2] = new NodalLocalDegreeOfFreedom(NodeStart, new DegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis1));

            nodalDegreeOfFreedoms[3] = new NodalLocalDegreeOfFreedom(NodeEnd, new DegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis3));
            nodalDegreeOfFreedoms[4] = new NodalLocalDegreeOfFreedom(NodeEnd, new DegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis2));
            nodalDegreeOfFreedoms[5] = new NodalLocalDegreeOfFreedom(NodeEnd, new DegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis1));


            return nodalDegreeOfFreedoms;
        }


        public override NodalGlobalDegreeOfFreedom[] GetGlobalDegreeOfFreedom(Node[] nodes)
        {
            return GetGlobalDegreeOfFreedom(nodes[0], nodes.Last());
        }


        public NodalGlobalDegreeOfFreedom[] GetGlobalDegreeOfFreedom(Node nodeStart, Node nodeEnd)
        {
            var localDegrees = GetNodalDegreeOfFreedom();

            NodalGlobalDegreeOfFreedom[] nodalDegreeOfFreedoms = new NodalGlobalDegreeOfFreedom[6];

            nodalDegreeOfFreedoms[0] = new NodalGlobalDegreeOfFreedom(nodeStart, new GlobalDegreeOfFreedom(localDegrees[0].DegreeOfFreedom));
            nodalDegreeOfFreedoms[1] = new NodalGlobalDegreeOfFreedom(nodeStart, new GlobalDegreeOfFreedom(localDegrees[1].DegreeOfFreedom));
            nodalDegreeOfFreedoms[2] = new NodalGlobalDegreeOfFreedom(nodeStart, new GlobalDegreeOfFreedom(localDegrees[2].DegreeOfFreedom));
            nodalDegreeOfFreedoms[3] = new NodalGlobalDegreeOfFreedom(nodeEnd, new GlobalDegreeOfFreedom(localDegrees[3].DegreeOfFreedom));
            nodalDegreeOfFreedoms[4] = new NodalGlobalDegreeOfFreedom(nodeEnd, new GlobalDegreeOfFreedom(localDegrees[4].DegreeOfFreedom));
            nodalDegreeOfFreedoms[5] = new NodalGlobalDegreeOfFreedom(nodeEnd, new GlobalDegreeOfFreedom(localDegrees[5].DegreeOfFreedom));


            return nodalDegreeOfFreedoms;
        }
    }
}
