using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.FEM.Properties;
using GPC.Model.Elements;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    public class TriangularFlexural : FiniteElement, IEquatable<TriangularFlexural>
    {
        //calculated and used in BuildMatrix and used also in BuildF
        //private double _areaElement;
        private mnl.Matrix<double> _etaInv;

        public TriangularFlexural(Node[] nodes, PlateProperty property, int id) : base(nodes, property, id)
        {
            //recalled base(nodes)
            _DOF.Add(LinearSolver.DOF.RX);
            _DOF.Add(LinearSolver.DOF.RY);
            _DOF.Add(LinearSolver.DOF.RZ);
            //a rotation in Local coordinate plane (rx, ry) can be a RX, RY, RZ in Global space!
        }

        public override void BuildMatrix()
        {
            /*
            * REFERENCE: CHAPTER 10.5 - THE FINITE ELEMENT METHOD IN ENGINEERING - SINGIRESU S.RAO
            */

            //Node 1 = Origin = Node i
            //Axis y assigned as Node 1 to Node 2, Node j = Node 2
            //Axis x ortogonal to axis y, Node k = node 3
            Node nodeI = GlobalNodesElement.ElementAt(1 - 1);
            Node nodeJ = GlobalNodesElement.ElementAt(2 - 1);
            Node nodeK = GlobalNodesElement.ElementAt(3 - 1);

            //calculation of local nodes
            LocalNodes(nodeI, nodeJ, nodeK, out Node node1, out Node node2, out Node node3);

            //calculation of matrix eta q = eta * alpha
            //alpha = constant of polynome
            //q = displacements:
            //q1 = w(x0,y0)
            //q2 = dw/dy(x0,y0)
            //q3 = -dw/dx(x0,y0)
            //...
            //q9 = -dw/dx(x2,y2)
            int n = GlobalNodesElement.Length * _DOF.Count;


            mnl.Matrix<double> eta = mnl.Matrix<double>.Build.Dense(n, n);
            eta[0, 0] = 1.0;

            eta[1, 2] = 1.0;

            eta[2, 1] = -1.0;

            eta[3, 0] = 1.0;
            eta[3, 2] = node2.Position.Y;
            eta[3, 5] = Math.Pow(node2.Position.Y, 2.0);
            eta[3, 8] = Math.Pow(node2.Position.Y, 3.0);

            eta[4, 2] = 1.0;
            eta[4, 5] = 2.0 * node2.Position.Y;
            eta[4, 8] = 3.0 * Math.Pow(node2.Position.Y, 2.0);

            eta[5, 1] = -1.0;
            eta[5, 4] = -node2.Position.Y;
            eta[5, 7] = -Math.Pow(node2.Position.Y, 2.0);

            eta[6, 0] = 1.0;
            eta[6, 1] = node3.Position.X;
            eta[6, 2] = node3.Position.Y;
            eta[6, 3] = Math.Pow(node3.Position.X, 2.0);
            eta[6, 4] = node3.Position.X * node3.Position.Y;
            eta[6, 5] = Math.Pow(node3.Position.Y, 2.0);
            eta[6, 6] = Math.Pow(node3.Position.X, 3.0);
            eta[6, 7] = Math.Pow(node3.Position.X, 3.0) * node3.Position.Y + node3.Position.X * Math.Pow(node3.Position.Y, 2.0);
            eta[6, 8] = Math.Pow(node3.Position.Y, 2.0);

            eta[7, 2] = 1.0;
            eta[7, 4] = node3.Position.X;
            eta[7, 5] = 2.0 * node3.Position.Y;
            eta[7, 7] = 2.0 * node3.Position.X * node3.Position.Y + Math.Pow(node3.Position.X, 2.0);
            eta[7, 8] = 3.0 * Math.Pow(node3.Position.Y, 3.0);
            
            eta[8, 1] = -1.0;
            eta[8, 3] = -2.0 * node3.Position.X;
            eta[8, 4] = - node3.Position.Y;
            eta[8, 6] = -3.0 * Math.Pow(node3.Position.X,2.0);
            eta[8, 7] = - Math.Pow(node3.Position.Y, 2.0) + 2.0 * node3.Position.X * node3.Position.Y;

            _etaInv = eta.Inverse();

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
            Console.WriteLine("D = " + _d.ToString());
            #endregion

            double tb = ((PlateProperty)Property).BendingThickness;
            double D = E * Math.Pow(tb, 3.0) / (12.0 * (1.0 - Math.Pow(ni, 2.0))); //flexural rigidity

            double integraldA = 1.0 / 2.0 * node3.Position.X * node2.Position.Y;
            double integralXdA = 1.0 / 6.0 * Math.Pow(node3.Position.X, 2.0) * node2.Position.Y;
            double integralYdA = 1.0 / 6.0 * node3.Position.X * node2.Position.Y * (node2.Position.Y + node3.Position.Y);
            double integralXXdA = 1.0 / 12.0 * Math.Pow(node3.Position.X, 3.0) * node2.Position.Y;
            double integralXYdA = 1.0 / 24.0 * Math.Pow(node3.Position.X, 2.0) * node2.Position.Y * (node2.Position.Y + 2.0 * node3.Position.Y);
            double integralYYdA = 1.0 / 12.0 * node3.Position.X * node2.Position.Y * (Math.Pow(node2.Position.Y, 2.0) + node2.Position.Y * node3.Position.Y + Math.Pow(node3.Position.Y, 3.0));

            mnl.Matrix<double> integratedBTraspDB = mnl.Matrix<double>.Build.Dense(n, n);

            integratedBTraspDB[3, 3] = 4.0 * integraldA;

            integratedBTraspDB[4, 4] = 2.0*(1-ni) * integraldA;

            integratedBTraspDB[5, 3] = 4.0 * ni * integraldA;
            integratedBTraspDB[5, 5] = 4.0 * integraldA;
            integratedBTraspDB[3, 5] = integratedBTraspDB[5, 3];

            integratedBTraspDB[6, 3] = 12.0 * integralXdA;
            integratedBTraspDB[6, 5] = 12.0 * ni * integralXdA;
            integratedBTraspDB[6, 6] = 36.0 * integralXXdA;
            integratedBTraspDB[3, 6] = _kElementLocalCoord[6, 3];
            integratedBTraspDB[5, 6] = _kElementLocalCoord[6, 5];

            integratedBTraspDB[7, 3] = 4.0 * (ni * integralXdA + integralYdA);
            integratedBTraspDB[7, 4] = 4.0 * (1.0 - ni) * (integralXdA + integralYdA);
            integratedBTraspDB[7, 5] = 4.0 * (integralXdA + ni * integralYdA);
            integratedBTraspDB[7, 6] = 12.0 * (ni * integralXXdA + integralXYdA);
            integratedBTraspDB[7, 7] = (12.0 - 8.0 * ni) * (integralXXdA + 2.0 * integralXYdA + integralYYdA)  - 8.0 * (1.0 - ni) * integralXYdA;
            integratedBTraspDB[3, 7] = _kElementLocalCoord[7, 3];
            integratedBTraspDB[4, 7] = _kElementLocalCoord[7, 4];
            integratedBTraspDB[5, 7] = _kElementLocalCoord[7, 5];
            integratedBTraspDB[6, 7] = _kElementLocalCoord[7, 6];

            integratedBTraspDB[8, 3] = 12.0 * ni * integralYdA;
            integratedBTraspDB[8, 5] = 12.0 * integralYdA;
            integratedBTraspDB[8, 6] = 36.0 * ni * integralXYdA;
            integratedBTraspDB[8, 7] = 12.0 * (integralXYdA + ni * integralYYdA);
            integratedBTraspDB[8, 8] = 36.0 * integralYYdA;
            integratedBTraspDB[3, 8] = _kElementLocalCoord[8, 3];
            integratedBTraspDB[5, 8] = _kElementLocalCoord[8, 5];
            integratedBTraspDB[6, 8] = _kElementLocalCoord[8, 6];
            integratedBTraspDB[7, 8] = _kElementLocalCoord[8, 7];

            _kElementLocalCoord = mnl.Matrix<double>.Build.Dense(n, n);
            _kElementLocalCoord = D * integratedBTraspDB;
        }

        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            
            mnl.Vector<double> _fLocalCoord = mnl.Vector<double>.Build.Dense(2 * GlobalNodesElement.Length); //2 = DOF in local : DX and DY
            /*foreach (IPlateLoadCaseAttribute iAttribute in _attributes)
            {
                if (iAttribute is PlatePressureAttribute)
                {
                    PlatePressureAttribute attribute = (PlatePressureAttribute)iAttribute;
                    //calcultation of pressures in local coordinate system of the element
                    Vector3d dirX = attribute.Sys.V11;
                    dirX.Unitize();
                    Vector3d dirY = attribute.Sys.V22;
                    dirY.Unitize();
                    Vector3d dirZ = attribute.Sys.V33;
                    dirZ.Unitize();

                    Vector3d x = LocalCoordinateSystem.V11;
                    dirX.Unitize();
                    Vector3d y = LocalCoordinateSystem.V22;
                    dirY.Unitize();
                    Vector3d z = LocalCoordinateSystem.V33;
                    dirZ.Unitize();

                    //Set in local coordinates
                    double px = attribute.P11 * dirX.DotProduct(x) + attribute.P22 * dirY.DotProduct(x) + attribute.P33 * dirZ.DotProduct(x);
                    double py = attribute.P11 * dirX.DotProduct(y) + attribute.P22 * dirY.DotProduct(y) + attribute.P33 * dirZ.DotProduct(y);
                    double pz = attribute.P11 * dirX.DotProduct(z) + attribute.P22 * dirY.DotProduct(z) + attribute.P33 * dirZ.DotProduct(z);

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
            }*/
            return _fLocalCoord;
        }

        /// <summary>
        /// stesso di triangular membrane...cambiare struttura derivando?
        /// </summary>
        /// <param name="nodeI"></param>
        /// <param name="nodeJ"></param>
        /// <param name="nodeK"></param>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <param name="node3"></param>
        protected void LocalNodes(Node nodeI, Node nodeJ, Node nodeK, out Node node1, out Node node2, out Node node3)
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

            node1 = new Node(0, 0, 0, nodeI.Index, nodeI.Name); //Origin GlobalNodes.ElementAt(1 - 1);
            node2 = new Node(v12.DotProduct(vecx), v12.DotProduct(vecy), v12.DotProduct(vecz), nodeJ.Index, nodeJ.Name); //Axis y GlobalNodes.ElementAt(2 - 1);
            node3 = new Node(v13.DotProduct(vecx), v13.DotProduct(vecy), v13.DotProduct(vecz), nodeK.Index, nodeK.Name); //GlobalNodes.ElementAt(3 - 1);
            #endregion
        }

        /// <summary>
        /// x, and y are local coordinates, z is thickess can exist from -t/2 to +t/2
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        protected mnl.Matrix<double> B(double x, double y, double z)
        {
            /*
             * u = z * dw/dx -> (dw/dx = rotation), z = distance from centerline of plate
             *  epsilon_xx = du/dx = - z * d^2 w/ dx^2
             */

            /*
             * epsilon = [B] * q = [B] * [eta_tilde] * alpha
             * [eta_tilde] from writtening : w(x0,y0) = q1, dw(x0,y0)/dy = q2, -dw(x0,y0)/dx = q3 ...
             * q = [eta_tilde] * alpha ; q are the generalized displacements
             * w = eta * alpha ; w is the approx function of real displacement. By theory rotation etc are all in depends of w
             */

            #region matrixB
            mnl.Matrix<double> b = mnl.Matrix<double>.Build.Dense(3, 9);
            b[0, 3] = 2.0;
            b[0, 6] = 6.0 * x;
            b[0, 7] = 2.0 * y;

            b[1, 5] = 2.0;
            b[1, 7] = 2.0 * x;
            b[1, 8] = 6.0 * y;

            b[2, 5] = 2.0;
            b[2, 7] = 4.0 * (x + y);

            b = -z * b;
            _b = b * _etaInv;
            return _b;
            #endregion
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

        public bool Equals(TriangularFlexural other)
        {
            return Equals((object)other);
        }
    }
}
