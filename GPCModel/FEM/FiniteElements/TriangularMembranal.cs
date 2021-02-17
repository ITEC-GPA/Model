using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Spatial.Euclidean;

namespace GPC.Model.FEM.FiniteElements
{
    public class TriangularMembranal : FiniteElement, IEquatable<TriangularMembranal>
    {
        public TriangularMembranal(Node[] nodes, int id) : base(nodes, id)
        {
            //recalled base(nodes)
            _DOF[FEM.DOF.DX] = true;
            _DOF[FEM.DOF.DY] = true;
            _DOF[FEM.DOF.DZ] = true;
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
            Node nodeP = new Node(nodeI.X + lij * dip, nodeI.Y + mij * dip, nodeI.Z + nij * dip, -1);
            double dpk = Math.Sqrt(Math.Pow(nodeK.X - nodeI.X, 2.0) + Math.Pow(nodeK.Y - nodeI.Y, 2.0) + Math.Pow(nodeK.Z - nodeI.Z, 2.0) - Math.Pow(dip, 2.0));

            double lpk = (nodeK.X - nodeP.X) / dpk;
            double mpk = (nodeK.Y - nodeP.Y) / dpk;
            double npk = (nodeK.Z - nodeP.Z) / dpk;

            _dofGlobalToLocal = Matrix<double>.Build.Dense(6, 9);
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

            Node node1 = new Node(0, 0, 0, nodeI.ID, nodeI.Label); //Origin GlobalNodes.ElementAt(1 - 1);
            Node node2 = new Node(v12.DotProduct(vecx), v12.DotProduct(vecy), v12.DotProduct(vecz), nodeJ.ID, nodeJ.Label); //Axis y GlobalNodes.ElementAt(2 - 1);
            Node node3 = new Node(v13.DotProduct(vecx), v13.DotProduct(vecy), v13.DotProduct(vecz), nodeK.ID, nodeK.Label); //GlobalNodes.ElementAt(3 - 1);

            //_localNodesElement = new Node[] { node1, node2, node3 };
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

            _b = Matrix<double>.Build.Dense(3, 6);
            _b[0, 0] = dy32;
            _b[0, 2] = -dy31;
            _b[0, 4] = dy21;

            _b[1, 1] = -dx32;
            _b[1, 3] = dx31;
            _b[1, 5] = -dx21;

            _b[2, 0] = -dx32;
            _b[2, 1] = dy32;
            _b[2, 2] = dx31;
            _b[2, 3] = -dy31;
            _b[2, 4] = -dx21;
            _b[2, 5] = dy21;
            Console.WriteLine("Matrix B = " + _b.ToString());

            _b = 1.0 / (2.0 * A) * _b;
            Console.WriteLine("Matrix B = " + _b.ToString());
            #endregion

            #region matrixD
            double E = 200000;
            double ni = 0.2;
            _d = Matrix<double>.Build.Dense(3, 3);
            _d[0, 0] = 1.0;
            _d[0, 1] = ni;
            _d[1, 0] = ni;
            _d[1, 1] = 1.0;
            _d[2, 2] = (1.0 - ni) / 2.0;
            _d = E / (1.0 - ni * ni) * _d;
            Console.WriteLine("D = " + _d.ToString());
            #endregion

            #region stiffnessMatrixInLocalCoordinates
            double thk = 1; //mm
            double V = A * thk;
            _kElementLocalCoord = V * _b.Transpose() * _d * _b;
            Console.WriteLine("KElementLocalCoord = " + KElementLocalCoord.ToString());
            #endregion

        }

        public override void BuildF()
        {
            // implement force equivalent to node due to prestress, or temperature etc
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            return obj is TriangularMembranal membranal &&
                   base.Equals(obj) &&
                   _ID == membranal._ID &&
                   EqualityComparer<Node[]>.Default.Equals(GlobalNodesElement, membranal.GlobalNodesElement);
        }

        public bool Equals(TriangularMembranal other)
        {
            return Equals((object)other);
        }
        public override int GetHashCode()
        {
            int hashCode = -125827218;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + _ID.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Node[]>.Default.GetHashCode(GlobalNodesElement);
            return hashCode;
        }
    }
}
