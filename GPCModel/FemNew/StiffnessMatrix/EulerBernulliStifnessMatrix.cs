using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem;
using GPC.Model.Fem.FemObjects;
using GPC.Model.Fem.FemObjects.FiniteElements;
using GPC.Model.Fem.Materials;
using GPC.Model.Fem.StiffnessMatrix;
using GPC.Model.Sections;

namespace GPC.Model.Fem.StiffnessMatrix
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


            NodalDegreeOfFreedom[] nodalDegree = GetNodalDegreeOfFreedom();

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


        internal override NodalDegreeOfFreedom[] GetNodalDegreeOfFreedom()
        {
            NodalDegreeOfFreedom[] nodalDegreeOfFreedoms = new NodalDegreeOfFreedom[12];
            nodalDegreeOfFreedoms[0] = new NodalDegreeOfFreedom(NodeStart, DegreeOfFreedom.DX);
            nodalDegreeOfFreedoms[1] = new NodalDegreeOfFreedom(NodeStart, DegreeOfFreedom.DY);
            nodalDegreeOfFreedoms[2] = new NodalDegreeOfFreedom(NodeStart, DegreeOfFreedom.DZ);
            nodalDegreeOfFreedoms[3] = new NodalDegreeOfFreedom(NodeStart, DegreeOfFreedom.RX);
            nodalDegreeOfFreedoms[4] = new NodalDegreeOfFreedom(NodeStart, DegreeOfFreedom.RY);
            nodalDegreeOfFreedoms[5] = new NodalDegreeOfFreedom(NodeStart, DegreeOfFreedom.RZ);

            nodalDegreeOfFreedoms[6] = new NodalDegreeOfFreedom(NodeEnd, DegreeOfFreedom.DX);
            nodalDegreeOfFreedoms[7] = new NodalDegreeOfFreedom(NodeEnd, DegreeOfFreedom.DY);
            nodalDegreeOfFreedoms[8] = new NodalDegreeOfFreedom(NodeEnd, DegreeOfFreedom.DZ);
            nodalDegreeOfFreedoms[9] = new NodalDegreeOfFreedom(NodeEnd, DegreeOfFreedom.RX);
            nodalDegreeOfFreedoms[10] = new NodalDegreeOfFreedom(NodeEnd, DegreeOfFreedom.RY);
            nodalDegreeOfFreedoms[11] = new NodalDegreeOfFreedom(NodeEnd, DegreeOfFreedom.RZ);


            return nodalDegreeOfFreedoms;
        }
    }

}
