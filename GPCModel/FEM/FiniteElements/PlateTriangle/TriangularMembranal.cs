using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.FEM.Properties;
using GPC.Model.FEM.Attributes;
using MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    public class TriangularMembranal : TriangleElement, IEquatable<TriangularMembranal>
    {
        public TriangularMembranal(Node[] nodes, PlateProperty property, int id) : base(nodes, property, id)
        {
            //recalled base(nodes)
            _DOF.Add(LinearSolver.DOF.DX);
            _DOF.Add(LinearSolver.DOF.DY);
            _DOF.Add(LinearSolver.DOF.DZ);
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
            Node nodeI = Nodes.ElementAt(1 - 1);
            Node nodeJ = Nodes.ElementAt(2 - 1);
            Node nodeK = Nodes.ElementAt(3 - 1);

            double dij = Math.Sqrt(Math.Pow(nodeJ.Position.X - nodeI.Position.X, 2.0) + Math.Pow(nodeJ.Position.Y - nodeI.Position.Y, 2.0) + Math.Pow(nodeJ.Position.Z - nodeI.Position.Z, 2.0));
            double lij = (nodeJ.Position.X - nodeI.Position.X) / dij;
            double mij = (nodeJ.Position.Y - nodeI.Position.Y) / dij;
            double nij = (nodeJ.Position.Z - nodeI.Position.Z) / dij;

            double dip = lij * (nodeK.Position.X - nodeI.Position.X) + mij * (nodeK.Position.Y - nodeI.Position.Y) + nij * (nodeK.Position.Z - nodeI.Position.Z);
            Node nodeP = new Node(nodeI.Position.X + lij * dip, nodeI.Position.Y + mij * dip, nodeI.Position.Z + nij * dip, -1);
            double dpk = Math.Sqrt(Math.Pow(nodeK.Position.X - nodeI.Position.X, 2.0) + Math.Pow(nodeK.Position.Y - nodeI.Position.Y, 2.0) + Math.Pow(nodeK.Position.Z - nodeI.Position.Z, 2.0) - Math.Pow(dip, 2.0));

            double lpk = (nodeK.Position.X - nodeP.Position.X) / dpk;
            double mpk = (nodeK.Position.Y - nodeP.Position.Y) / dpk;
            double npk = (nodeK.Position.Z - nodeP.Position.Z) / dpk;

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

            LocalNodes(nodeI, nodeJ, nodeK, out Node node1, out Node node2, out Node node3); //take global node and transform in local nodes

            #region ShapeFuction
            double dx32 = node3.Position.X - node2.Position.X;
            double dy21 = node2.Position.Y - node1.Position.Y;
            double dx21 = node2.Position.X - node1.Position.X;
            double dy32 = node3.Position.Y - node2.Position.Y;

            double dx31 = node3.Position.X - node1.Position.X;
            double dy31 = node3.Position.Y - node1.Position.X;

            _areaElement = 1.0 / 2.0 * (dx32 * dy21 - dx21 * dy32);

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

            #region BMatrixDerivateOfShapeFunctionInLocalCoordinates
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

            _b = 1.0 / (2.0 * _areaElement) * _b;
            Console.WriteLine("Matrix B = " + _b.ToString());
            #endregion

            #region matrixD
            double E = ((PlateProperty)_property).GetE();
            double ni = ((PlateProperty)_property).GetNi();

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
            double thk = ((PlateProperty)_property).MembraneThickness;
            double V = _areaElement * thk;
            _kElementLocalCoord = V * _b.Transpose() * _d * _b;
            Console.WriteLine("KElementLocalCoord = " + KElementLocalCoord.ToString());
            #endregion
        }

        protected override Vector<double> BuildFLocalCoord()
        {
            Vector<double> _fLocalCoord = Vector<double>.Build.Dense(2 * Nodes.Length); //2 = DOF in local : DX and DY
            foreach (IPlateLoadCaseAttribute iAttribute in _attributes)
            {
                if (iAttribute is PlatePressureAttribute)
                {
                    PlatePressureAttribute attribute = (PlatePressureAttribute)iAttribute;
                    //calcultation of pressures in local coordinate system of the element
                    Vector3d dirX = attribute.CoordinateSystem.V11;
                    dirX.Unitize();
                    Vector3d dirY = attribute.CoordinateSystem.V22;
                    dirY.Unitize();
                    Vector3d dirZ = attribute.CoordinateSystem.V33;
                    dirZ.Unitize();

                    Vector3d x = LocalCoordinateSystem.V11;
                    dirX.Unitize();
                    Vector3d y = LocalCoordinateSystem.V22;
                    dirY.Unitize();
                    Vector3d z = LocalCoordinateSystem.V33;
                    dirZ.Unitize();

                    //Set in local coordinates
                    double px = attribute.P1 * dirX.DotProduct(x) + attribute.P2 * dirY.DotProduct(x) + attribute.P3 * dirZ.DotProduct(x);
                    double py = attribute.P1 * dirX.DotProduct(y) + attribute.P2 * dirY.DotProduct(y) + attribute.P3 * dirZ.DotProduct(y);
                    double pz = attribute.P1 * dirX.DotProduct(z) + attribute.P2 * dirY.DotProduct(z) + attribute.P3 * dirZ.DotProduct(z);

                    if (pz != 0.0)
                    {
                        throw new Exception("this finite element can't support out of plane pressure");
                    }

                    //Pressure --> node force
                    Vector3d f = new Vector3d(px * _areaElement / 3.0, py * _areaElement / 3.0, pz * _areaElement / 3.0); //force applied in each node

                    for (int i = 0; i < _fLocalCoord.Count; i=i+2)
                    {
                        _fLocalCoord[i] = f.X;
                        _fLocalCoord[i+1] = f.Y;
                    }
                }
            }
            return _fLocalCoord;
        }

        /*protected void LocalNodes(Node nodeI, Node nodeJ, Node nodeK, out Node node1, out Node node2, out Node node3)
        {
            #region CalculationOfLocalCoordinates
            //Search for 3 local axis
            Vector3d y = new Vector3d(nodeJ.Position.X - nodeI.Position.X, nodeJ.Position.Y - nodeI.Position.Y, nodeJ.Position.Z - nodeI.Position.Z);
            Vector3d vecy = new Vector3d(y);
            vecy.Unitize();

            Vector3d x = new Vector3d(nodeK.Position.X - nodeI.Position.X, nodeK.Position.Y - nodeI.Position.Y, nodeK.Position.Z - nodeI.Position.Z);
            Vector3d vecx = new Vector3d(x);
            vecx.Unitize();

            Vector3d z = x.CrossProduct(y);
            //UnitVector3D vecz = z.Normalize();
            //_vecZLocal = vecz.ToVector().ToArray();
            Vector3d vecz = new Vector3d(z);
            vecz.Unitize();

            //recalculation of x that can be non-ortogonal
            x = y.CrossProduct(z);
            vecx = new Vector3d(x);
            vecx.Unitize();
            //_vecXLocal = vecx.ToVector().ToArray();
            _localCoordinateSystem = new Geometry.CoordinateSystem(new Point3d(0, 0, 0), vecx, vecy);

            //move to local axis
            //calculation in local nodes
            Vector3d v12 = new Vector3d(nodeJ.Position.X - nodeI.Position.X, nodeJ.Position.Y - nodeI.Position.Y, nodeJ.Position.Z - nodeI.Position.Z);
            Vector3d v13 = new Vector3d(nodeK.Position.X - nodeI.Position.X, nodeK.Position.Y - nodeI.Position.Y, nodeK.Position.Z - nodeI.Position.Z);

            node1 = new Node(0, 0, 0, nodeI.Id, nodeI.Name); //Origin GlobalNodes.ElementAt(1 - 1);
            node2 = new Node(v12.DotProduct(vecx), v12.DotProduct(vecy), v12.DotProduct(vecz), nodeJ.Id, nodeJ.Name); //Axis y GlobalNodes.ElementAt(2 - 1);
            node3 = new Node(v13.DotProduct(vecx), v13.DotProduct(vecy), v13.DotProduct(vecz), nodeK.Id, nodeK.Name); //GlobalNodes.ElementAt(3 - 1);
            #endregion
        }*/

        public override bool Equals(object obj)
        {
            return obj is TriangularMembranal membranal &&
                   base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 624022166 + base.GetHashCode();
        }

        public bool Equals(TriangularMembranal other)
        {
            return Equals((object)other);
        }
    }
}
