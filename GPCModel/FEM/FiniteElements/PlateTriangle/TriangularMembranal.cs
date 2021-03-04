using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.FEM.Properties;
using GPC.Model.FEM.Attributes;
using mnl = MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    public class TriangularMembranal : Plate, IEquatable<TriangularMembranal>
    {
        #region variables
        protected double _areaElement;

        protected mnl.Matrix<double> _b;
        #endregion

        public TriangularMembranal(Node[] nodes, PlateProperty property, int id) : base(nodes, property, id)
        {
            //recalled base(nodes)
            _DOF.Add(LinearSolver.DOF.DX);
            _DOF.Add(LinearSolver.DOF.DY);
            _DOF.Add(LinearSolver.DOF.DZ);
            //a displacement in Local coordinate plane (Dx, Dy) can be a DX, DY, DZ in Global space!

            //Local matrix: 3 nodes x 2(dX, dY) gdl = 6x6 matrix
            //Global matrix: 3 nodes x 3(DX, DY, DZ) gdl = 9x9 matrix

            //DofGlobalToLocal^T * kLocal * DofGlobalToLocal
            //   [9x6]               [6x6]     [6x9]
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

            _dofGlobalToLocal = mnl.Matrix<double>.Build.Dense(6, 9);
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
            //Console.WriteLine("Local To Global Matrix = " + DofGlobalToLocal.ToString());
            #endregion

            Node[] localNodes = LocalNodes(); //take global node and transform in local nodes
            Node node1 = localNodes[0];
            Node node2 = localNodes[1];
            Node node3 = localNodes[2];

            #region ShapeFuction
            double dx32 = node3.Position.X - node2.Position.X;
            double dy21 = node2.Position.Y - node1.Position.Y;
            double dx21 = node2.Position.X - node1.Position.X;
            double dy32 = node3.Position.Y - node2.Position.Y;

            double dx31 = node3.Position.X - node1.Position.X;
            double dy31 = node3.Position.Y - node1.Position.X;

            _areaElement = 1.0 / 2.0 * (dx32 * dy21 - dx21 * dy32);
            #endregion

            #region BMatrixDerivateOfShapeFunctionInLocalCoordinates
            _b = mnl.Matrix<double>.Build.Dense(3, 6);
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
            //Console.WriteLine("Matrix B = " + _b.ToString());

            _b = 1.0 / (2.0 * _areaElement) * _b;
            //Console.WriteLine("Matrix B = " + _b.ToString());
            #endregion

            #region matrixD
            double E = ((PlateProperty)_property).GetE();
            double ni = ((PlateProperty)_property).GetNi();

            _d = mnl.Matrix<double>.Build.Dense(3, 3);
            _d[0, 0] = 1.0;
            _d[0, 1] = ni;
            _d[1, 0] = ni;
            _d[1, 1] = 1.0;
            _d[2, 2] = (1.0 - ni) / 2.0;
            _d = E / (1.0 - ni * ni) * _d;
            //Console.WriteLine("D = " + _d.ToString());
            #endregion

            #region stiffnessMatrixInLocalCoordinates
            double thk = ((PlateProperty)_property).MembraneThickness;
            double V = _areaElement * thk;
            _kElementLocalCoord = V * _b.Transpose() * _d * _b;
            //Console.WriteLine("KElementLocalCoord = " + KElementLocalCoord.ToString());
            #endregion
        }

        public override Matrix<double> GetB(double csi = 0, double eta = 0, double zeta = 0)
        {
            return _b;
        }

        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            mnl.Vector<double> _fLocalCoord = mnl.Vector<double>.Build.Dense(2 * Nodes.Length); //2 = DOF in local : DX and DY
            foreach (IPlateLoadCaseAttribute iAttribute in _attributesLoadCase)
            {
                if (iAttribute is PlatePressureAttribute)
                {
                    PlatePressureAttribute attribute = (PlatePressureAttribute)iAttribute;
                    //calcultation of pressures in local coordinate system of the element
                    Vector3d dirX = attribute.CoordinateSystem.V1;
                    dirX.Unitize();
                    Vector3d dirY = attribute.CoordinateSystem.V2;
                    dirY.Unitize();
                    Vector3d dirZ = attribute.CoordinateSystem.V3;
                    dirZ.Unitize();

                    Vector3d x = LocalCoordinateSystem.V1;
                    dirX.Unitize();
                    Vector3d y = LocalCoordinateSystem.V2;
                    dirY.Unitize();
                    Vector3d z = LocalCoordinateSystem.V3;
                    dirZ.Unitize();

                    //Set in local coordinates
                    double px = attribute.P1 * dirX.DotProduct(x) + attribute.P2 * dirY.DotProduct(x) + attribute.P3 * dirZ.DotProduct(x);
                    double py = attribute.P1 * dirX.DotProduct(y) + attribute.P2 * dirY.DotProduct(y) + attribute.P3 * dirZ.DotProduct(y);
                    double pz = attribute.P1 * dirX.DotProduct(z) + attribute.P2 * dirY.DotProduct(z) + attribute.P3 * dirZ.DotProduct(z);

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
        /// <summary>
        /// According to RAO, order of nodes are CLOCKWISE
        /// </summary>
        /// <returns></returns>
        protected Node[] LocalNodes()
        {
            Node nodeI = Nodes[0];
            Node nodeJ = Nodes[1];
            Node nodeK = Nodes[2];

            #region CalculationOfLocalCoordinates
            //Search for 3 local axis
            Vector3d y = new Vector3d(nodeJ.Position.X - nodeI.Position.X, nodeJ.Position.Y - nodeI.Position.Y, nodeJ.Position.Z - nodeI.Position.Z);
            Vector3d vecy = new Vector3d(y);
            vecy.Unitize();

            Vector3d x = new Vector3d(nodeK.Position.X - nodeI.Position.X, nodeK.Position.Y - nodeI.Position.Y, nodeK.Position.Z - nodeI.Position.Z);
            Vector3d vecx = new Vector3d(x);
            vecx.Unitize();

            Vector3d z = x.CrossProduct(y);
            Vector3d vecz = new Vector3d(z);
            vecz.Unitize();

            //recalculation of x that can be non-ortogonal
            x = y.CrossProduct(z);
            vecx = new Vector3d(x);
            vecx.Unitize();;
            _localCoordinateSystem = new CoordinateSystem(new Point3d(0, 0, 0), vecx, vecy);

            //move to local axis
            //calculation in local nodes
            Vector3d v12 = new Vector3d(nodeJ.Position.X - nodeI.Position.X, nodeJ.Position.Y - nodeI.Position.Y, nodeJ.Position.Z - nodeI.Position.Z);
            Vector3d v13 = new Vector3d(nodeK.Position.X - nodeI.Position.X, nodeK.Position.Y - nodeI.Position.Y, nodeK.Position.Z - nodeI.Position.Z);

            Node[] localNodes = new Node[3];
            localNodes[0] = new Node(0, 0, 0, nodeI.Id, nodeI.Name); //Origin GlobalNodes.ElementAt(1 - 1);
            localNodes[1] = new Node(v12.DotProduct(vecx), v12.DotProduct(vecy), v12.DotProduct(vecz), nodeJ.Id, nodeJ.Name); //Axis y GlobalNodes.ElementAt(2 - 1);
            localNodes[2] = new Node(v13.DotProduct(vecx), v13.DotProduct(vecy), v13.DotProduct(vecz), nodeK.Id, nodeK.Name); //GlobalNodes.ElementAt(3 - 1);
            #endregion
            return localNodes;
        }

        public override void GetResults(double[] globalDisplacementsNodes, out double[] localDisplacements, out mnl.Matrix<double>[] globalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon)
        {
            localDisplacements = GetLocalDisplacement(globalDisplacementsNodes);
            mnl.Vector<double> vecLocalDispl = mnl.Vector<double>.Build.Dense(localDisplacements);
        
            #region CalculationOfStressAndDeformationsInLocalCoordinates
            mnl.Vector<double> epsilonLocal = GetB() * vecLocalDispl; //epsilon_xx; epsilon_yy; epsilon_xy
            mnl.Vector<double> stressLocal = D * epsilonLocal; //sigma_xx; sigma_yy; tau_xy
            /*Console.WriteLine("Strains in Local coordinates:" + epsilon.ToString());
            Console.WriteLine("Stress in Local coordinates:" + stress.ToString());*/
            #endregion

            #region ConvertInGlobalCoordinates
            //Define Couchy Tensor
            mnl.Matrix<double> epsilonLocalCouchy = mnl.Matrix<double>.Build.Dense(3, 3);
            epsilonLocalCouchy[0, 0] = epsilonLocal[0]; //epsilon_xx
            epsilonLocalCouchy[1, 1] = epsilonLocal[1]; //epsilon_yy

            epsilonLocalCouchy[0, 1] = epsilonLocal[2]; //epsilon_xy
            epsilonLocalCouchy[1, 0] = epsilonLocal[2]; //epsilon_yx
            //epsilonCouchy[2, 2] = -ni / E * (sigma_xx + sigma_yy) + alpha * Temperature ; //epsilon_zz
            //Console.WriteLine("Epsilon local coordinate:" + epsilonCouchy.ToString());

            mnl.Matrix<double> stressLocalCouchy = mnl.Matrix<double>.Build.Dense(3, 3);
            stressLocalCouchy[0, 0] = stressLocal[0]; //sigma_xx
            stressLocalCouchy[1, 1] = stressLocal[1]; //sigma_yy

            stressLocalCouchy[0, 1] = stressLocal[2]; //sigma_xy
            stressLocalCouchy[1, 0] = stressLocal[2]; //sigma_yx
            //Console.WriteLine("Stress local coordinate:" + stressCouchy.ToString());

            //Rotation matrix
            mnl.Matrix<double> rotation = mnl.Matrix<double>.Build.Dense(3, 3);
            CoordinateSystem versorsLocalAxis = LocalCoordinateSystem;
            Vector3d xVersor = versorsLocalAxis.V1;
            Vector3d yVersor = versorsLocalAxis.V2;
            Vector3d zVersor = versorsLocalAxis.V3;

            rotation[0, 0] = xVersor.X;
            rotation[0, 1] = yVersor.X;
            rotation[0, 2] = zVersor.X;

            rotation[1, 0] = xVersor.Y;
            rotation[1, 1] = yVersor.Y;
            rotation[1, 2] = zVersor.Y;

            rotation[2, 0] = xVersor.Z;
            rotation[2, 1] = yVersor.Z;
            rotation[2, 2] = zVersor.Z;
            //Console.WriteLine("Rotation matrix tensor:" + rotation.ToString());

            //Second order tensor -> Trotated = Q * T * Q^T
            mnl.Matrix<double> epsilonGlobalCouchy = rotation * epsilonLocalCouchy * rotation.Transpose();
            Console.WriteLine("Epsilon in global coordinates = " + epsilonGlobalCouchy);

            mnl.Matrix<double> stressGlobalCouchy = rotation * stressLocalCouchy * rotation.Transpose();
            Console.WriteLine("Stress in global coordinates = " + stressGlobalCouchy);
            #endregion

            localPseudoDeformation = new mnl.Matrix<double>[1] { epsilonLocalCouchy };
            globalPseudoDeformation = new mnl.Matrix<double>[1] { epsilonGlobalCouchy };

            localStress = new mnl.Matrix<double>[1] { stressLocalCouchy };
            globalStress = new mnl.Matrix<double>[1] { stressGlobalCouchy };

            double thickness = ((PlateProperty)_property).MembraneThickness;
            globalForces = new mnl.Matrix<double>[1] { thickness * stressGlobalCouchy };
            localForces = new mnl.Matrix<double>[1] { thickness * stressLocalCouchy };
                        
            globalEpsilon = new mnl.Matrix<double>[1] { epsilonGlobalCouchy };
            localEpsilon = new mnl.Matrix<double>[1] { stressGlobalCouchy };
        }

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
