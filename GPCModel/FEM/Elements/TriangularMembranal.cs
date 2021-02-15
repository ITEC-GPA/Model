using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Spatial.Euclidean;

namespace GPC.FEM.Elements
{
    public class TriangularMembranal : FiniteElement
    {
        double[] _vecXLocal = new double[3]; //versor X local in Global Coordinate Sys
        double[] _vecYLocal = new double[3]; //versor Y local in Global Coordinate Sys
        double[] _vecZLocal = new double[3]; //versor Z local in Global Coordinate Sys

        public TriangularMembranal(IEnumerable<Node> nodes) : base(nodes)
        {
            //recalled base(nodes)
            _DOF[FEMModel.DOF.DX] = true;
            _DOF[FEMModel.DOF.DY] = true;
            _DOF[FEMModel.DOF.DZ] = true;
            //a displacement in Local coordinate plane (Dx, Dy) can be a DX, DY, DZ in Global space!
        }

        public override void BuildMatrix()
        {
            /*
            * REFERENCE: CHAPTER 10 - THE FINITE ELEMENT METHOD IN ENGINEERING - SINGIRESU S.RAO
            */

            //Node 1 = Origin = Node i
            //Axis y assigned as Node 1 to Node 2, Node j = Node 2
            //Axis x ortogonal to axis y, Node k = node 3
            //calculation of matrix for transformation from Local to Global coordinates
            #region TransformationMatrixLocalCoordinatesToGlobalCoordinates
            Node nodeI = GlobalNodesElement.ElementAt(1 - 1);
            Node nodeJ = GlobalNodesElement.ElementAt(2 - 1);
            Node nodeK = GlobalNodesElement.ElementAt(3 - 1);

            double dij = Math.Sqrt(Math.Pow(nodeJ.X - nodeI.X, 2.0) + Math.Pow(nodeJ.Y - nodeI.Y, 2.0) + Math.Pow(nodeJ.Z - nodeI.Z, 2.0));
            double lij = (nodeJ.X - nodeI.X) / dij;
            double mij = (nodeJ.Y - nodeI.Y) / dij;
            double nij = (nodeJ.Z - nodeI.Z) / dij;

            double dip = lij * (nodeK.X - nodeI.X) + mij * (nodeK.Y - nodeI.Y) + nij * (nodeK.Z - nodeI.Z);
            Node nodeP = new Node(nodeI.X + lij * dip, nodeI.Y + mij * dip, nodeI.Z + nij * dip);
            double dpk = Math.Sqrt(Math.Pow(nodeK.X - nodeI.X, 2.0) + Math.Pow(nodeK.Y - nodeI.Y, 2.0) + Math.Pow(nodeK.Z - nodeI.Z, 2.0) - Math.Pow(dip, 2.0));

            double lpk = (nodeK.X - nodeP.X) / dpk;
            double mpk = (nodeK.Y - nodeP.Y) / dpk;
            double npk = (nodeK.Z - nodeP.Z) / dpk;

            DofGlobalToLocal = Matrix<double>.Build.Dense(6, 9);
            DofGlobalToLocal[0, 0] = lpk;
            DofGlobalToLocal[0, 1] = mpk;
            DofGlobalToLocal[0, 2] = npk;

            DofGlobalToLocal[1, 0] = lij;
            DofGlobalToLocal[1, 1] = mij;
            DofGlobalToLocal[1, 2] = nij;

            DofGlobalToLocal[2, 3] = lpk;
            DofGlobalToLocal[2, 4] = mpk;
            DofGlobalToLocal[2, 5] = npk;

            DofGlobalToLocal[3, 3] = lij;
            DofGlobalToLocal[3, 4] = mij;
            DofGlobalToLocal[3, 5] = nij;

            DofGlobalToLocal[4, 6] = lpk;
            DofGlobalToLocal[4, 7] = mpk;
            DofGlobalToLocal[4, 8] = npk;

            DofGlobalToLocal[5, 6] = lij;
            DofGlobalToLocal[5, 7] = mij;
            DofGlobalToLocal[5, 8] = nij;
            Console.WriteLine("Local To Global Matrix = " + DofGlobalToLocal.ToString());
            #endregion

            #region BMatrixDerivateOfShapeFunctionInLocalCoordinates

            #region CalculationOfLocalCoordinates
            //Search for 3 local axis
            Vector3D y = new Vector3D(nodeJ.X - nodeI.X, nodeJ.Y - nodeI.Y, nodeJ.Z - nodeI.Z);
            UnitVector3D vecy = y.Normalize();
            _vecYLocal = vecy.ToVector().ToArray();

            Vector3D x = new Vector3D(nodeK.X - nodeI.X, nodeK.Y - nodeI.Y, nodeK.Z - nodeI.Z);
            UnitVector3D vecx = x.Normalize();

            Vector3D z = x.CrossProduct(y);
            UnitVector3D vecz = z.Normalize();
            _vecZLocal = vecz.ToVector().ToArray();

            //recalculation of x that can be non-ortogonal
            x = y.CrossProduct(z);
            vecx = x.Normalize();
            _vecXLocal = vecx.ToVector().ToArray();

            //move to local axis
            //calculation in local nodes
            Vector3D v12 = new Vector3D(nodeJ.X - nodeI.X, nodeJ.Y - nodeI.Y, nodeJ.Z - nodeI.Z);
            Vector3D v13 = new Vector3D(nodeK.X - nodeI.X, nodeK.Y - nodeI.Y, nodeK.Z - nodeI.Z);

            Node node1 = new Node(0, 0, 0, nodeI.Label); //Origin GlobalNodes.ElementAt(1 - 1);
            Node node2 = new Node(v12.DotProduct(vecx), v12.DotProduct(vecy), v12.DotProduct(vecz), nodeJ.Label); //Axis y GlobalNodes.ElementAt(2 - 1);
            Node node3 = new Node(v13.DotProduct(vecx), v13.DotProduct(vecy), v13.DotProduct(vecz), nodeK.Label); //GlobalNodes.ElementAt(3 - 1);

            LocalNodesElement = new Node[] { node1, node2, node3 };
            #endregion

            #region ShapeFuction
            double dx32 = node3.X - node2.X;
            double dy21 = node2.Y - node1.Y;
            double dx21 = node2.X - node1.X;
            double dy32 = node3.Y - node2.Y;

            double dx31 = node3.X - node1.X;
            double dy31 = node3.Y - node1.X;

            double A = 1.0 / 2.0 * (dx32 * dy21 - dx21 * dy32);

            /*ShapeFunctions = new Polynome[3];
            for (int i = 0; i < nodes.Count(); i++) {
                Polynome1D px = new Polynome1D(1, "x");
                Polynome1D py = new Polynome1D(1, "y");
                ShapeFunctions[i] = new Polynome(new Polynome1D[] { px, py });
            }

            double[] shapeCoeff = new double[3];
            shapeCoeff[0] = 1.0 / (2.0 * A) * (dy32 * -node2.X - dx32 * -node2.Y);
            shapeCoeff[1] = 1.0 / (2.0 * A) * (dy32);
            shapeCoeff[2] = 1.0 / (2.0 * A) * (-dx32);
            ShapeFunctions.ElementAt(0).Coefficients = shapeCoeff;

            shapeCoeff[0] = 1.0 / (2.0 * A) * (-dy31 * -node3.X + dx31 * -node3.Y);
            shapeCoeff[1] = 1.0 / (2.0 * A) * (-dy31);
            shapeCoeff[2] = 1.0 / (2.0 * A) * (dx31);
            ShapeFunctions.ElementAt(1).Coefficients = shapeCoeff;

            shapeCoeff[0] = 1.0 / (2.0 * A) * (dy21 * -node1.X - dx21 * -node1.Y);
            shapeCoeff[1] = 1.0 / (2.0 * A) * (dy21);
            shapeCoeff[2] = 1.0 / (2.0 * A) * (-dx21);
            ShapeFunctions.ElementAt(2).Coefficients = shapeCoeff;*/
            #endregion

            B = Matrix<double>.Build.Dense(3, 6);
            B[0, 0] = dy32;
            B[0, 2] = -dy31;
            B[0, 4] = dy21;

            B[1, 1] = -dx32;
            B[1, 3] = dx31;
            B[1, 5] = -dx21;

            B[2, 0] = -dx32;
            B[2, 1] = dy32;
            B[2, 2] = dx31;
            B[2, 3] = -dy31;
            B[2, 4] = -dx21;
            B[2, 5] = dy21;
            Console.WriteLine("Matrix B = " + B.ToString());

            B = 1.0 / (2.0 * A) * B;
            Console.WriteLine("Matrix B = " + B.ToString());
            #endregion

            #region matrixD
            double E = 200000;
            double ni = 0.2;
            D = Matrix<double>.Build.Dense(3, 3);
            D[0, 0] = 1.0;
            D[0, 1] = ni;
            D[1, 0] = ni;
            D[1, 1] = 1.0;
            D[2, 2] = (1.0 - ni) / 2.0;
            D = E / (1.0 - ni * ni) * D;
            Console.WriteLine("D = " + D.ToString());
            #endregion

            #region stiffnessMatrixInLocalCoordinates
            double thk = 1; //mm
            double V = A * thk;
            KElementLocalCoord = V * B.Transpose() * D * B;
            Console.WriteLine("KElementLocalCoord = " + KElementLocalCoord.ToString());
            #endregion

            #region StiffnessMatrixInGlobalCoordinates
            KElementGlobalCoord = DofGlobalToLocal.Transpose() * KElementLocalCoord * DofGlobalToLocal;
            Console.WriteLine("KElementGlobalCoord = " + KElementGlobalCoord.ToString());

            /*
            int maxGdlPerNode = Enum.GetNames(typeof(FEMModel.DOF)).Length;
            if (GlobalNodesElement.Count() * maxGdlPerNode > KElementGlobalCoord.ColumnCount)
            {
                //set right dimension of KeGlobal according to max degree of freedom that can be used
                //KeGlobal actual: UX, UY, UZ foreach node (3 nodes beacause of triangular)
                for (int i = GlobalNodesElement.Count(); i > 0; i--)
                {
                    //Add RX field
                    KElementGlobalCoord = KElementGlobalCoord.InsertColumn(i * 3 - 1, Vector<double>.Build.Dense(KElementGlobalCoord.RowCount));
                    KElementGlobalCoord = KElementGlobalCoord.InsertRow(i * 3 - 1, Vector<double>.Build.Dense(KElementGlobalCoord.ColumnCount));
                    //Add RY field
                    KElementGlobalCoord = KElementGlobalCoord.InsertColumn(i * 3 - 1, Vector<double>.Build.Dense(KElementGlobalCoord.ColumnCount));
                    KElementGlobalCoord = KElementGlobalCoord.InsertRow(i * 3 - 1, Vector<double>.Build.Dense(KElementGlobalCoord.ColumnCount));
                    //Add RZ field
                    KElementGlobalCoord = KElementGlobalCoord.InsertColumn(i * 3 - 1, Vector<double>.Build.Dense(KElementGlobalCoord.RowCount));
                    KElementGlobalCoord = KElementGlobalCoord.InsertRow(i * 3 - 1, Vector<double>.Build.Dense(KElementGlobalCoord.ColumnCount));
                }
            }
            Console.WriteLine("KeGlobal with all gld = " + KElementGlobalCoord.ToString());
            */
            #endregion

            #region ElementStiffnessMatrixInGlobalCoordinatesToStiffnessMatrixOfSystem                
            PositionToGlobalSystemK = new Dictionary<Position, Position>();

            int[] posGlobal = new int[GlobalNodesElement.Count()]; //First gdl of GlobalNodes(i) are in KGlobal[posGlobal[i],posGlobal[i]]
            int[] posLocal = new int[LocalNodesElement.Count()]; //First gdl of LocalNodes(i)==GlobalNodes(i) are in KeGlobal[posLocal[i],posLocal[i]]
            
            for (int i = 0; i < GlobalNodesElement.Count(); i++)
            {
                //posGlobal[i] = (GlobalNodesElement.ElementAt(i).ID) * maxGdlPerNode;
                posGlobal[i] = FEM.FEMModel.GetPositionInKGlobal(GlobalNodesElement.ElementAt(i).ID, FEMModel.DOF.DX);
                posLocal[i] = i * base.NrDOFActive;
                //Console.WriteLine("K[" + posGlobal[i] + "," + posGlobal[i] + "] = k[" + posLocal[i] + "," + posLocal[i] + "]");
            }

            //create PositionToGlobalK, bind uniquely KeGlobal (stiffness matrix of element in Global coords) to KGlobal (stiffness matrix of entire system)
            //create also bind with global system result to global element result
            _positionGlobalDisplElementInGlobalDisplSystemVectorResult = new int[base.NrDOFActive * GlobalNodesElement.Length]; //3 = DX, DY, DZ GLOBAL
            for (int i = 0; i < GlobalNodesElement.Count(); i++) //calculation for each node i
            {
                for (int j = 0; j < base.NrDOFActive; j++) //each node i have degree of freedom
                {
                    _positionGlobalDisplElementInGlobalDisplSystemVectorResult[i * NrDOFActive + j] = posGlobal[i] + j;
                    for (int k = 0; k < GlobalNodesElement.Count(); k++) //each node i with its degree of freedom should be take in account with other node k. What hap in node k if force is applied in node i?
                    {
                        for (int l = 0; l < base.NrDOFActive; l++) //what hap to the degree of freedom of node k?
                        {
                            int rowLocal = posLocal[i] + j;
                            int colLocal = posLocal[k] + l;

                            int rowGlobal = posGlobal[i] + j;
                            int colGlobal = posGlobal[k] + l;
                            //Console.WriteLine(iter + " Node " + GlobalNodes.ElementAt(i).ID + " gld " + (FEMModel.GDL)j + " respect Node " + GlobalNodes.ElementAt(k).ID + " gdl " + (FEMModel.GDL)l + "=" + KeGlobal[rowLocal,colLocal]);

                            Position pLoc = new Position(rowLocal, colLocal);
                            Position pGlob = new Position(rowGlobal, colGlobal);
                            PositionToGlobalSystemK.Add(pLoc, pGlob);
                            //Console.WriteLine("kGlobal[" + rowLocal + "," + colLocal + "] -> KGlobal[" + rowGlobal + "," + colGlobal + "]");
                        }
                    }
                }
            }
            Console.WriteLine("PositionToGlobalK dimension : " + PositionToGlobalSystemK.Count);
            #endregion
        }

        public override void BuildF()
        {
            // implement force equivalent to node due to prestress, or temperature etc
            throw new NotImplementedException();
        }

        public override void CalcResults(double[] Displacements)
        {
            //Read displacements in global coordinates
            base.CalcResults(Displacements);

            #region CalculationOfStressAndDeformationsInLocalCoordinates
            //check this for bending moments, N and other....
            Vector<double> epsilon = Vector<double>.Build.Dense(3); //epsilon_xx; epsilon_yy; epsilon_xy
            Vector<double> stress = Vector<double>.Build.Dense(epsilon.Count); //sigma_xx; sigma_yy; tau_xy

            Vector<double> vecLocalDispl = Vector<double>.Build.Dense(_nodeDisplacementLocalCoordinates);
            epsilon = B * vecLocalDispl;
            stress = D * epsilon;
            Console.WriteLine("Strains in Local coordinates:" + epsilon.ToString());
            Console.WriteLine("Stress in Local coordinates:" + stress.ToString());
            #endregion

            #region ConvertInGlobalCoordinates
            //Define Couchy Tensor
            Matrix<double> epsilonCouchy = Matrix<double>.Build.Dense(3, 3);
            epsilonCouchy[0, 0] = epsilon[0]; //epsilon_xx
            epsilonCouchy[1, 1] = epsilon[1]; //epsilon_yy
            epsilonCouchy[0, 1] = epsilon[2]; //epsilon_xy
            epsilonCouchy[1, 0] = epsilon[2]; //epsilon_yx
            Console.WriteLine("Epsilon local coordinate:" + epsilonCouchy.ToString());

            Matrix<double> stressCouchy = Matrix<double>.Build.Dense(3, 3);
            stressCouchy[0, 0] = stress[0]; //sigma_xx
            stressCouchy[1, 1] = stress[1]; //sigma_yy
            stressCouchy[0, 1] = stress[2]; //sigma_xy
            stressCouchy[1, 0] = stress[2]; //sigma_yx
            Console.WriteLine("Stress local coordinate:" + stressCouchy.ToString());

            //Rotation matrix
            Matrix<double> rotation = Matrix<double>.Build.Dense(3, 3);
            rotation[0, 0] = _vecXLocal[0];
            rotation[0, 1] = _vecYLocal[0];
            rotation[0, 2] = _vecZLocal[0];

            rotation[1, 0] = _vecXLocal[1];
            rotation[1, 1] = _vecYLocal[1];
            rotation[1, 2] = _vecYLocal[1];

            rotation[2, 0] = _vecXLocal[2];
            rotation[2, 1] = _vecYLocal[2];
            rotation[2, 2] = _vecZLocal[2];
            Console.WriteLine("Rotation matrix tensor:" + rotation.ToString());

            //ATTENTION: Second order tensor -> Trotated = Q * T * Q^T
            Matrix<double> epsilonGlobalCoord = rotation * epsilonCouchy * rotation.Transpose();
            Console.WriteLine("Epsilon in global coordinates = " + epsilonGlobalCoord);
            Matrix<double> sigmaGlobalCoord = rotation * stressCouchy * rotation.Transpose();
            Console.WriteLine("Stress in global coordinates = " + sigmaGlobalCoord);
            #endregion
        }
    }
}
