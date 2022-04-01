using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem;
using GPC.Model.Fem.FemObjects;
using GPC.Model.Fem.FemObjects.FiniteElements;
using GPC.Model.Fem.Materials;
using GPC.Model.Sections;

namespace GPC.Model.Fem.ElementStiffnessMatrices
{


    internal class EulerBernulliStifnessMatrix : FiniteElementLocalStiffnessMatrix
    {


        public Node NodeStart => _localNodes[0];
        public Node NodeEnd => _localNodes.Last();


        public EulerBernulliStifnessMatrix(EulerBeam beam)
            : base(new Node[] { new Node(new Geometry.Point3d(0, 0, 0), 1), new Node(new Geometry.Point3d(0, 0, beam.Length), 0) }, beam)
        {

        }

        protected override ElementStiffnessMatrix GetStiffnessMatrix(FiniteElement element)
        {
            return GetStiffnessMatrix((EulerBeam)element);
        }

        protected ElementStiffnessMatrix GetStiffnessMatrix(EulerBeam element)
        {
            Section section = element.Section;
            IsotropicFemMaterial material = section.GetIsotropicFemMaterial();


            double lenght = element.Length;
            double E = material.E;
            double G = material.G;

            double A = section.Area;
            double Ixx = section.J11;
            double Iyy = section.J22;
            double J = section.Jt;


            NodalLocalDegreeOfFreedom[] nodalDegree = GetNodalDegreeOfFreedom();

            ElementStiffnessMatrix matrix = new ElementStiffnessMatrix(nodalDegree);

            // Diagonale
            matrix.SetElementAt(nodalDegree[0], E * A / lenght);
            matrix.SetElementAt(nodalDegree[1], 12.0 * E * Ixx / Math.Pow(lenght, 3));
            matrix.SetElementAt(nodalDegree[2], 12.0 * E * Iyy / Math.Pow(lenght, 3));
            matrix.SetElementAt(nodalDegree[3], G * J / lenght);
            matrix.SetElementAt(nodalDegree[4], 4.0 * E * Iyy / lenght);
            matrix.SetElementAt(nodalDegree[5], 4.0 * E * Ixx / lenght);


            matrix.SetElementAt(nodalDegree[6], E * A / lenght);
            matrix.SetElementAt(nodalDegree[7], 12.0 * E * Ixx / Math.Pow(lenght, 3));
            matrix.SetElementAt(nodalDegree[8], 12.0 * E * Iyy / Math.Pow(lenght, 3));
            matrix.SetElementAt(nodalDegree[9], G * J / lenght);
            matrix.SetElementAt(nodalDegree[10], 4.0 * E * Iyy / lenght);
            matrix.SetElementAt(nodalDegree[11], 4.0 * E * Ixx / lenght);


            // Fuori Diagonale
            matrix.SetElementAtSymmetric(nodalDegree[4], nodalDegree[2], -6.0 * E * Iyy / Math.Pow(lenght, 2));
            matrix.SetElementAtSymmetric(nodalDegree[5], nodalDegree[1], +6.0 * E * Ixx / Math.Pow(lenght, 2));
            matrix.SetElementAtSymmetric(nodalDegree[6], nodalDegree[0], -E * A / lenght);

            matrix.SetElementAtSymmetric(nodalDegree[7], nodalDegree[1], -12.0 * E * Ixx / Math.Pow(lenght, 3));
            matrix.SetElementAtSymmetric(nodalDegree[7], nodalDegree[5], -6.0 * E * Iyy / Math.Pow(lenght, 2));

            matrix.SetElementAtSymmetric(nodalDegree[8], nodalDegree[2], -12.0 * E * Iyy / Math.Pow(lenght, 3));
            matrix.SetElementAtSymmetric(nodalDegree[8], nodalDegree[4], +6.0 * E * Iyy / Math.Pow(lenght, 2));

            matrix.SetElementAtSymmetric(nodalDegree[9], nodalDegree[4], -G * J / lenght);

            matrix.SetElementAtSymmetric(nodalDegree[10], nodalDegree[2], -6.0 * E * Iyy / Math.Pow(lenght, 2));
            matrix.SetElementAtSymmetric(nodalDegree[10], nodalDegree[4], +2.0 * E * Iyy / lenght);
            matrix.SetElementAtSymmetric(nodalDegree[10], nodalDegree[8], +6.0 * E * Iyy / Math.Pow(lenght, 2));

            matrix.SetElementAtSymmetric(nodalDegree[11], nodalDegree[1], +6.0 * E * Ixx / Math.Pow(lenght, 2));
            matrix.SetElementAtSymmetric(nodalDegree[11], nodalDegree[5], +2.0 * E * Ixx / lenght);
            matrix.SetElementAtSymmetric(nodalDegree[11], nodalDegree[7], -6.0 * E * Ixx / Math.Pow(lenght, 2));


            return matrix;
        }


        protected override NodalLocalDegreeOfFreedom[] GetNodalDegreeOfFreedom()
        {
            NodalLocalDegreeOfFreedom[] nodalDegreeOfFreedoms = new NodalLocalDegreeOfFreedom[12];
            nodalDegreeOfFreedoms[0] = new NodalLocalDegreeOfFreedom(NodeStart, DegreeOfFreedoms.LocalDegreeOfFreedoms.D1);
            nodalDegreeOfFreedoms[1] = new NodalLocalDegreeOfFreedom(NodeStart, DegreeOfFreedoms.LocalDegreeOfFreedoms.D2);
            nodalDegreeOfFreedoms[2] = new NodalLocalDegreeOfFreedom(NodeStart, DegreeOfFreedoms.LocalDegreeOfFreedoms.D3);
            nodalDegreeOfFreedoms[3] = new NodalLocalDegreeOfFreedom(NodeStart, DegreeOfFreedoms.LocalDegreeOfFreedoms.R1);
            nodalDegreeOfFreedoms[4] = new NodalLocalDegreeOfFreedom(NodeStart, DegreeOfFreedoms.LocalDegreeOfFreedoms.R2);
            nodalDegreeOfFreedoms[5] = new NodalLocalDegreeOfFreedom(NodeStart, DegreeOfFreedoms.LocalDegreeOfFreedoms.R3);

            nodalDegreeOfFreedoms[6] = new NodalLocalDegreeOfFreedom(NodeEnd,  DegreeOfFreedoms.LocalDegreeOfFreedoms.D1);
            nodalDegreeOfFreedoms[7] = new NodalLocalDegreeOfFreedom(NodeEnd,  DegreeOfFreedoms.LocalDegreeOfFreedoms.D2);
            nodalDegreeOfFreedoms[8] = new NodalLocalDegreeOfFreedom(NodeEnd,  DegreeOfFreedoms.LocalDegreeOfFreedoms.D3);
            nodalDegreeOfFreedoms[9] = new NodalLocalDegreeOfFreedom(NodeEnd,  DegreeOfFreedoms.LocalDegreeOfFreedoms.R1);
            nodalDegreeOfFreedoms[10] = new NodalLocalDegreeOfFreedom(NodeEnd, DegreeOfFreedoms.LocalDegreeOfFreedoms.R2);
            nodalDegreeOfFreedoms[11] = new NodalLocalDegreeOfFreedom(NodeEnd, DegreeOfFreedoms.LocalDegreeOfFreedoms.R3);


            return nodalDegreeOfFreedoms;
        }

        public override NodalGlobalDegreeOfFreedom[] GetGlobalDegreeOfFreedom(Node[] nodes)
        {
            return GetGlobalDegreeOfFreedom(nodes[0], nodes.Last());
        }

        public NodalGlobalDegreeOfFreedom[] GetGlobalDegreeOfFreedom(Node nodeStart, Node NodeEnd)
        {
            NodalGlobalDegreeOfFreedom[] nodalDegreeOfFreedoms = new NodalGlobalDegreeOfFreedom[12];

            nodalDegreeOfFreedoms[0] = new NodalGlobalDegreeOfFreedom(nodeStart, DegreeOfFreedoms.GlobalDegreeOfFreedoms.DX);
            nodalDegreeOfFreedoms[1] = new NodalGlobalDegreeOfFreedom(nodeStart, DegreeOfFreedoms.GlobalDegreeOfFreedoms.DY);
            nodalDegreeOfFreedoms[2] = new NodalGlobalDegreeOfFreedom(nodeStart, DegreeOfFreedoms.GlobalDegreeOfFreedoms.DZ);
            nodalDegreeOfFreedoms[3] = new NodalGlobalDegreeOfFreedom(nodeStart, DegreeOfFreedoms.GlobalDegreeOfFreedoms.RX);
            nodalDegreeOfFreedoms[4] = new NodalGlobalDegreeOfFreedom(nodeStart, DegreeOfFreedoms.GlobalDegreeOfFreedoms.RY);
            nodalDegreeOfFreedoms[5] = new NodalGlobalDegreeOfFreedom(nodeStart, DegreeOfFreedoms.GlobalDegreeOfFreedoms.RZ);

            nodalDegreeOfFreedoms[6] = new NodalGlobalDegreeOfFreedom(NodeEnd, DegreeOfFreedoms.GlobalDegreeOfFreedoms.DX);
            nodalDegreeOfFreedoms[7] = new NodalGlobalDegreeOfFreedom(NodeEnd, DegreeOfFreedoms.GlobalDegreeOfFreedoms.DY);
            nodalDegreeOfFreedoms[8] = new NodalGlobalDegreeOfFreedom(NodeEnd, DegreeOfFreedoms.GlobalDegreeOfFreedoms.DZ);
            nodalDegreeOfFreedoms[9] = new NodalGlobalDegreeOfFreedom(NodeEnd, DegreeOfFreedoms.GlobalDegreeOfFreedoms.RX);
            nodalDegreeOfFreedoms[10] = new NodalGlobalDegreeOfFreedom(NodeEnd, DegreeOfFreedoms.GlobalDegreeOfFreedoms.RY);
            nodalDegreeOfFreedoms[11] = new NodalGlobalDegreeOfFreedom(NodeEnd, DegreeOfFreedoms.GlobalDegreeOfFreedoms.RZ);


            return nodalDegreeOfFreedoms;
        }


    }

}
