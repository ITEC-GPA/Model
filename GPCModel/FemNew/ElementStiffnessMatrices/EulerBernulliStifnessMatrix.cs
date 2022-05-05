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


        public Node NodeStart => _nodes[0];
        public Node NodeEnd => _nodes.Last();


        public EulerBernulliStifnessMatrix(EulerBeam beam)
            : base(new Node[] { new Node(new Geometry.Point3d(0, 0, 0), 1), new Node(new Geometry.Point3d(0, 0, beam.Length), 2) }, beam)
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
            double I11 = section.J11;
            double I22 = section.J22;
            double J = section.Jt;

            NodalLocalDegreeOfFreedom[] nodalDegree = GetNodalDegreeOfFreedom();

            ElementStiffnessMatrix matrix = new ElementStiffnessMatrix(nodalDegree);


            int shear1DofIndex = 0;
            int shear2DofIndex = 1;
            int axialDofIndex = 2;

            int totalDof = 6;
            int displacementsDof = 3;

            // Diagonale
            matrix.SetElementAt(nodalDegree[shear1DofIndex], 12.0 * E * I22 / Math.Pow(lenght, 3));
            matrix.SetElementAt(nodalDegree[shear2DofIndex], 12.0 * E * I11 / Math.Pow(lenght, 3));
            matrix.SetElementAt(nodalDegree[axialDofIndex], E * A / lenght);
            matrix.SetElementAt(nodalDegree[shear1DofIndex + displacementsDof], 4.0 * E * I11 / lenght);
            matrix.SetElementAt(nodalDegree[shear2DofIndex + displacementsDof], 4.0 * E * I22 / lenght);
            matrix.SetElementAt(nodalDegree[axialDofIndex + displacementsDof], G * J / lenght);


            matrix.SetElementAt(nodalDegree[shear1DofIndex + totalDof], 12.0 * E * I22 / Math.Pow(lenght, 3));
            matrix.SetElementAt(nodalDegree[shear2DofIndex + totalDof], 12.0 * E * I11 / Math.Pow(lenght, 3));
            matrix.SetElementAt(nodalDegree[axialDofIndex + totalDof], E * A / lenght);
            matrix.SetElementAt(nodalDegree[shear1DofIndex + displacementsDof + totalDof], 4.0 * E * I11 / lenght);
            matrix.SetElementAt(nodalDegree[shear2DofIndex + displacementsDof + totalDof], 4.0 * E * I22 / lenght);
            matrix.SetElementAt(nodalDegree[axialDofIndex + displacementsDof + totalDof], G * J / lenght);


            // Fuori Diagonale
            matrix.SetElementAtSymmetric(nodalDegree[shear1DofIndex], nodalDegree[shear2DofIndex + displacementsDof], +6.0 * E * I22 / Math.Pow(lenght, 2));
            matrix.SetElementAtSymmetric(nodalDegree[shear1DofIndex], nodalDegree[shear1DofIndex + totalDof], -12.0 * E * I22 / Math.Pow(lenght, 3));
            matrix.SetElementAtSymmetric(nodalDegree[shear1DofIndex], nodalDegree[shear2DofIndex + totalDof + displacementsDof], +6.0 * E * I22 / Math.Pow(lenght, 2));

            matrix.SetElementAtSymmetric(nodalDegree[shear2DofIndex], nodalDegree[shear1DofIndex + displacementsDof], -6.0 * E * I11 / Math.Pow(lenght, 2));
            matrix.SetElementAtSymmetric(nodalDegree[shear2DofIndex], nodalDegree[shear2DofIndex + totalDof], -12.0 * E * I11 / Math.Pow(lenght, 3));
            matrix.SetElementAtSymmetric(nodalDegree[shear2DofIndex], nodalDegree[shear1DofIndex + totalDof + displacementsDof], -6.0 * E * I11 / Math.Pow(lenght, 2));

            matrix.SetElementAtSymmetric(nodalDegree[axialDofIndex], nodalDegree[axialDofIndex + totalDof], -E * A / lenght);

            matrix.SetElementAtSymmetric(nodalDegree[shear1DofIndex + displacementsDof], nodalDegree[shear2DofIndex + totalDof], +6.0 * E * I11 / Math.Pow(lenght, 2));
            matrix.SetElementAtSymmetric(nodalDegree[shear1DofIndex + displacementsDof], nodalDegree[shear1DofIndex + totalDof + displacementsDof], +2.0 * E * I11 / lenght);

            matrix.SetElementAtSymmetric(nodalDegree[shear2DofIndex + displacementsDof], nodalDegree[shear1DofIndex + totalDof], -6.0 * E * I22 / Math.Pow(lenght, 2));
            matrix.SetElementAtSymmetric(nodalDegree[shear2DofIndex + displacementsDof], nodalDegree[shear2DofIndex + totalDof + displacementsDof], +2.0 * E * I22 / lenght);

            matrix.SetElementAtSymmetric(nodalDegree[axialDofIndex + displacementsDof], nodalDegree[axialDofIndex + displacementsDof + totalDof], -G * J / lenght);

            matrix.SetElementAtSymmetric(nodalDegree[shear1DofIndex + totalDof], nodalDegree[shear2DofIndex + totalDof + displacementsDof], -6.0 * E * I22 / Math.Pow(lenght, 2));

            matrix.SetElementAtSymmetric(nodalDegree[shear2DofIndex + totalDof], nodalDegree[shear1DofIndex + totalDof + displacementsDof], +6.0 * E * I11 / Math.Pow(lenght, 2));



            return matrix;
        }


        protected override NodalLocalDegreeOfFreedom[] GetNodalDegreeOfFreedom()
        {
            NodalLocalDegreeOfFreedom[] nodalDegreeOfFreedoms = new NodalLocalDegreeOfFreedom[12];
            
            nodalDegreeOfFreedoms[0]  = new NodalLocalDegreeOfFreedom(NodeStart, new DegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis3));
            nodalDegreeOfFreedoms[1]  = new NodalLocalDegreeOfFreedom(NodeStart, new DegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis2));
            nodalDegreeOfFreedoms[2]  = new NodalLocalDegreeOfFreedom(NodeStart, new DegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis1));
            nodalDegreeOfFreedoms[3]  = new NodalLocalDegreeOfFreedom(NodeStart, new DegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Rotation, DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis3));
            nodalDegreeOfFreedoms[4]  = new NodalLocalDegreeOfFreedom(NodeStart, new DegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Rotation, DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis2));
            nodalDegreeOfFreedoms[5]  = new NodalLocalDegreeOfFreedom(NodeStart, new DegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Rotation, DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis1));
                                      
            nodalDegreeOfFreedoms[6]  = new NodalLocalDegreeOfFreedom(NodeEnd,  new DegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis3));
            nodalDegreeOfFreedoms[7]  = new NodalLocalDegreeOfFreedom(NodeEnd,  new DegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis2));
            nodalDegreeOfFreedoms[8]  = new NodalLocalDegreeOfFreedom(NodeEnd,  new DegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Displacement, DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis1));
            nodalDegreeOfFreedoms[9]  = new NodalLocalDegreeOfFreedom(NodeEnd,  new DegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Rotation, DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis3));
            nodalDegreeOfFreedoms[10] = new NodalLocalDegreeOfFreedom(NodeEnd,  new DegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Rotation, DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis2));
            nodalDegreeOfFreedoms[11] = new NodalLocalDegreeOfFreedom(NodeEnd,  new DegreeOfFreedom(DegreeOfFreedom.DegreeOfFreedomTypes.Rotation, DegreeOfFreedom.DegreeOfFreedomLocalDirections.Axis1));


            return nodalDegreeOfFreedoms;
        }

        public override NodalGlobalDegreeOfFreedom[] GetGlobalDegreeOfFreedom(Node[] nodes)
        {
            return GetGlobalDegreeOfFreedom(nodes[0], nodes.Last());
        }

        public NodalGlobalDegreeOfFreedom[] GetGlobalDegreeOfFreedom(Node nodeStart, Node NodeEnd)
        {
            var localDegrees = GetNodalDegreeOfFreedom();

            NodalGlobalDegreeOfFreedom[] nodalDegreeOfFreedoms = new NodalGlobalDegreeOfFreedom[12];

            nodalDegreeOfFreedoms[0] = new NodalGlobalDegreeOfFreedom(nodeStart, new GlobalDegreeOfFreedom(localDegrees[0].DegreeOfFreedom));
            nodalDegreeOfFreedoms[1] = new NodalGlobalDegreeOfFreedom(nodeStart, new GlobalDegreeOfFreedom(localDegrees[1].DegreeOfFreedom));
            nodalDegreeOfFreedoms[2] = new NodalGlobalDegreeOfFreedom(nodeStart, new GlobalDegreeOfFreedom(localDegrees[2].DegreeOfFreedom));
            nodalDegreeOfFreedoms[3] = new NodalGlobalDegreeOfFreedom(nodeStart, new GlobalDegreeOfFreedom(localDegrees[3].DegreeOfFreedom));
            nodalDegreeOfFreedoms[4] = new NodalGlobalDegreeOfFreedom(nodeStart, new GlobalDegreeOfFreedom(localDegrees[4].DegreeOfFreedom));
            nodalDegreeOfFreedoms[5] = new NodalGlobalDegreeOfFreedom(nodeStart, new GlobalDegreeOfFreedom(localDegrees[5].DegreeOfFreedom));

            nodalDegreeOfFreedoms[6] = new NodalGlobalDegreeOfFreedom(NodeEnd,  new GlobalDegreeOfFreedom(localDegrees[6].DegreeOfFreedom));
            nodalDegreeOfFreedoms[7] = new NodalGlobalDegreeOfFreedom(NodeEnd,  new GlobalDegreeOfFreedom(localDegrees[7].DegreeOfFreedom));
            nodalDegreeOfFreedoms[8] = new NodalGlobalDegreeOfFreedom(NodeEnd,  new GlobalDegreeOfFreedom(localDegrees[8].DegreeOfFreedom));
            nodalDegreeOfFreedoms[9] = new NodalGlobalDegreeOfFreedom(NodeEnd,  new GlobalDegreeOfFreedom(localDegrees[9].DegreeOfFreedom));
            nodalDegreeOfFreedoms[10] = new NodalGlobalDegreeOfFreedom(NodeEnd, new GlobalDegreeOfFreedom(localDegrees[10].DegreeOfFreedom));
            nodalDegreeOfFreedoms[11] = new NodalGlobalDegreeOfFreedom(NodeEnd, new GlobalDegreeOfFreedom(localDegrees[11].DegreeOfFreedom));


            return nodalDegreeOfFreedoms;
        }


    }

}
