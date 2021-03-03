using GPC.Geometry;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Properties;
using System;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    public class TriangleElement : Plate
    {
        #region variables
        private TriangularMembranal _membranal;
        private TriangularDK _flexural;
        private mnl.Matrix<double> _kElementGlobalCoord; //the sum of the 2 stiffness matrix of TriangularMembranal and TriangularDK
        #endregion

        #region properties
        //public TriangularMembranal Membranal => _membranal;
        //public TriangularDK Flexural => _flexural;
        public override mnl.Matrix<double> KElementGlobalCoord => _kElementGlobalCoord;
        #endregion

        public TriangleElement(Node[] nodes, PlateProperty property, int id) : base(nodes, property, id)
        {
            DOF.Add(LinearSolver.DOF.DX);
            DOF.Add(LinearSolver.DOF.DY);
            DOF.Add(LinearSolver.DOF.DZ);
            DOF.Add(LinearSolver.DOF.RX);
            DOF.Add(LinearSolver.DOF.RY);
            DOF.Add(LinearSolver.DOF.RZ);

            //kElementGlobal = 3 * 6 = 18x18
            _membranal = new TriangularMembranal(nodes, property, id);
            _flexural = new TriangularDK(nodes, property, id);
        }

        public override void BuildMatrix()
        {
            //Calculation of the stiffness matrix of the 2 elements: sum of membrane and bending in the right position
            _membranal.BuildMatrix();
            _flexural.BuildMatrix();

            //Sum of the stiffness directly in global coordinates
            mnl.Matrix<double> m = _membranal.KElementGlobalCoord;
            //write stiffness matrix of membranal element
            /*Console.WriteLine("membranal component global coordinates:");
            for (int r = 0; r < m.RowCount; r++)
            {
                for (int c = 0; c < m.ColumnCount; c++)
                {
                    Console.Write(m[r,c] + " ");
                }
                Console.WriteLine();
            }*/

            mnl.Matrix<double> b = _flexural.KElementGlobalCoord;
            //write stiffness matrix of flexural element
            /*Console.WriteLine("bending component global coordinates:");
            for (int r = 0; r < b.RowCount; r++)
            {
                for (int c = 0; c < b.ColumnCount; c++)
                {
                    Console.Write(b[r, c] + " ");
                }
                Console.WriteLine();
            }*/

            _kElementGlobalCoord = mnl.Matrix<double>.Build.Dense(18, 18); //3 nodes x 6 dof = 18
            _kElementGlobalCoord = b;

            //Add stiffness due to membrane element in the right position
            #region AddStiffnessMembraneNode1
            //3x3 of Node 1
            int rStartGlobal = 0;
            int cStartGlobal = 0;
            int rStartLocal = 0;
            int cStartLocal = 0;
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    _kElementGlobalCoord[rStartGlobal + r, cStartGlobal + c] = _kElementGlobalCoord[rStartGlobal + r, cStartGlobal + c] + m[rStartLocal + r, cStartLocal + c];
                }
            }

            //3 x 3 of node1 to Node2
            rStartGlobal = 6;
            cStartGlobal = 0;
            rStartLocal = 3;
            cStartLocal = 0;
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    _kElementGlobalCoord[rStartGlobal + r, cStartGlobal + c] = _kElementGlobalCoord[rStartGlobal + r, cStartGlobal + c] + m[rStartLocal + r, cStartLocal + c];
                    //next line used for the symmetric
                    _kElementGlobalCoord[cStartGlobal + c, rStartGlobal + r] = _kElementGlobalCoord[cStartGlobal + c, rStartGlobal + r] + m[cStartLocal + c, rStartLocal + r];
                }
            }

            //3 x 3 of node1 to Node3
            rStartGlobal = 12;
            cStartGlobal = 0;
            rStartLocal = 6;
            cStartLocal = 0;
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    _kElementGlobalCoord[rStartGlobal + r, cStartGlobal + c] = _kElementGlobalCoord[rStartGlobal + r, cStartGlobal + c] + m[rStartLocal + r, cStartLocal + c];
                    //next line used for the symmetric
                    _kElementGlobalCoord[cStartGlobal + c, rStartGlobal + r] = _kElementGlobalCoord[cStartGlobal + c, rStartGlobal + r] + m[cStartLocal + c, rStartLocal + r];
                }
            }
            #endregion

            #region AddStiffnessMembraneNode2

            //3x3 of Node 2
            rStartGlobal = 6;
            cStartGlobal = 6;
            rStartLocal = 3;
            cStartLocal = 3;
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    _kElementGlobalCoord[rStartGlobal + r, cStartGlobal + c] = _kElementGlobalCoord[rStartGlobal + r, cStartGlobal + c] + m[rStartLocal + r, cStartLocal + c];
                }
            }

            //3 x 3 of node2 to Node3
            rStartGlobal = 12;
            cStartGlobal = 6;
            rStartLocal = 6;
            cStartLocal = 3;
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    _kElementGlobalCoord[rStartGlobal + r, cStartGlobal + c] = _kElementGlobalCoord[rStartGlobal + r, cStartGlobal + c] + m[rStartLocal + r, cStartLocal + c];
                    //next line used for the symmetric
                    _kElementGlobalCoord[cStartGlobal + c, rStartGlobal + r] = _kElementGlobalCoord[cStartGlobal + c, rStartGlobal + r] + m[cStartLocal + c, rStartLocal + r];
                }
            }
            #endregion

            #region AddStiffnessMembraneNode3

            //3x3 of Node 3
            rStartGlobal = 12;
            cStartGlobal = 12;
            rStartLocal = 6;
            cStartLocal = 6;
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    _kElementGlobalCoord[rStartGlobal + r, cStartGlobal + c] = _kElementGlobalCoord[rStartGlobal + r, cStartGlobal + c] + m[rStartLocal + r, cStartLocal + c];
                }
            }
            #endregion

            /*Console.WriteLine("Final Stiffness Matrix");
            for (int r = 0; r < _kElementGlobalCoord.RowCount; r++)
            {
                for (int c = 0; c < _kElementGlobalCoord.ColumnCount; c++)
                {
                    Console.Write(_kElementGlobalCoord[r,c] + " ");
                }
                Console.WriteLine();
            }*/

        }

        public override mnl.Vector<double> GetGlobalCoordF()
        {
            //Return membranal + flexural node forces

            var fMembranal = _membranal.GetGlobalCoordF();
            var fDK = _flexural.GetGlobalCoordF();

            mnl.Vector<double> f = fDK;
            //Add effect of membranal element
            //Node1 force along DX, DY, DZ
            int globalStart = 0;
            int localStart = 0;
            for (int i = 0; i < 3; i++)
            {
                f[globalStart + i] = f[globalStart + i] + fMembranal[localStart + i];
            }

            //Node2
            globalStart = 6;
            localStart = 3;
            for (int i = 0; i < 3; i++)
            {
                f[globalStart + i] = f[globalStart + i] + fMembranal[localStart + i];
            }

            //Node3
            globalStart = 12;
            localStart = 6;
            for (int i = 0; i < 3; i++)
            {
                f[globalStart + i] = f[globalStart + i] + fMembranal[localStart + i];
            }
            return f;
        }

        public override void AddAttribute(IPlateLoadCaseAttribute attribute)
        {
            //the attribute will add to the 2 finite element, Membrane and Discrete Kirchoff (DK). The attribute will have its impact in each finite element.
            //The nodal forces will be added
            _membranal.AddAttribute(attribute);
            _flexural.AddAttribute(attribute);
        }

        public override void GetResults(double[] displacementsNodes, bool displacementsInGlobalCoordinates = true)
        {
            if (displacementsInGlobalCoordinates == false)
            {
                //essendo un elemento composto, non ha molto senso passare direttamente spostamenti in coordinate locali
                throw new NotImplementedException();
            }
            mnl.Vector<double> membranalGlobalDisplacements = mnl.Vector<double>.Build.Dense(3 * 3); //in plane displacement can be in DX, DY, DZ in global coordinates
            
            //node 1
            int startGlobal = 0;
            int startLocal = 0;
            for (int j = 0; j < 3; j++)
            {
                membranalGlobalDisplacements[startLocal + j] = displacementsNodes[startGlobal + j];
            }

            //node 2
            startGlobal = 6;
            startLocal = 3;
            for (int j = 0; j < 3; j++)
            {
                membranalGlobalDisplacements[startLocal + j] = displacementsNodes[startGlobal + j];
            }

            //node 3
            startGlobal = 12;
            startLocal = 6;
            for (int j = 0; j < 3; j++)
            {
                membranalGlobalDisplacements[startLocal + j] = displacementsNodes[startGlobal + j];
            }

            mnl.Vector<double> flexuralGlobalDisplacements = mnl.Vector<double>.Build.Dense(displacementsNodes); //dz + rx + ry can be in DX, DY, DZ, RX, RY, RZ in global coordinates

            //get results
            _membranal.GetResults(membranalGlobalDisplacements, true);
            _flexural.GetResults(flexuralGlobalDisplacements, true);
        }
    }
}
