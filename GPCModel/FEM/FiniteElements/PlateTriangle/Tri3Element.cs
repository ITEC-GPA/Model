using GPC.Geometry;
using GPC.Model.Fem.Attributes;
using GPC.Model.Fem.Properties;
using GPC.Utilities.Fem;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.Fem.FiniteElements
{
    public class Tri3Element : Plate
    {
        #region variables
        private Tri3PlaneStress _membranal;
        private Tri3DK _flexural;
        private mnl.Matrix<double> _kElementGlobalCoord; //the sum of the 2 stiffness matrix of TriangularMembranal and TriangularDK
        #endregion

        #region properties
        public override mnl.Matrix<double> KElementGlobalCoord => _kElementGlobalCoord;
        #endregion

        public Tri3Element(Node[] nodes) : base(nodes)
        {
            DOF.Add(Solver.DOF.DX);
            DOF.Add(Solver.DOF.DY);
            DOF.Add(Solver.DOF.DZ);
            DOF.Add(Solver.DOF.RX);
            DOF.Add(Solver.DOF.RY);
            DOF.Add(Solver.DOF.RZ);

            //kElementGlobal = 3 * 6 = 18x18
            _membranal = new Tri3PlaneStress(nodes); //TODO: cambiare con elemento con Drilling of freedom
            _flexural = new Tri3DK(nodes);
        }

        internal Tri3Element(Node[] nodes, PlateProperty property) : this(nodes)
        {
            SetProperty(property);
        }

        internal override void SetProperty(ElementProperty property)
        {
            _membranal.SetProperty(property);
            _flexural.SetProperty(property);
            base.SetProperty(property);
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

        public override bool AddLoadCaseAttribute(IPlateLoadCaseAttribute attribute)
        {
            //the attribute will add to the 2 finite element, Membrane and Discrete Kirchoff (DK). The attribute will have its impact in each finite element.
            //The nodal forces will be added
            return _membranal.AddLoadCaseAttribute(attribute) && _flexural.AddLoadCaseAttribute(attribute);
        }

        //TODO: Da ottimizzare/scrivere
        public void GetNodesResults(double[] globalDisplacementsNodes, out double[] localDisplacements, out mnl.Matrix<double>[] globalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon)
        {
            mnl.Vector<double> membranalGlobalDisplacements = mnl.Vector<double>.Build.Dense(3 * 3); //in plane displacement can be in DX, DY, DZ in global coordinates

            //node 1
            int startGlobal = 0;
            int startLocal = 0;
            for (int j = 0; j < 3; j++)
            {
                membranalGlobalDisplacements[startLocal + j] = globalDisplacementsNodes[startGlobal + j];
            }

            //node 2
            startGlobal = 6;
            startLocal = 3;
            for (int j = 0; j < 3; j++)
            {
                membranalGlobalDisplacements[startLocal + j] = globalDisplacementsNodes[startGlobal + j];
            }

            //node 3
            startGlobal = 12;
            startLocal = 6;
            for (int j = 0; j < 3; j++)
            {
                membranalGlobalDisplacements[startLocal + j] = globalDisplacementsNodes[startGlobal + j];
            }

            mnl.Vector<double> flexuralGlobalDisplacements = mnl.Vector<double>.Build.Dense(globalDisplacementsNodes); //dz + rx + ry can be in DX, DY, DZ, RX, RY, RZ in global coordinates

            //get results
            _membranal.GetNodesResults(membranalGlobalDisplacements.ToArray(), out double[] membranalLocalDisplacements, out mnl.Matrix<double>[] membranalGlobalPseudoDisplacements, out mnl.Matrix<double>[] membranalLocalPseudoDisplacements, out mnl.Matrix<double>[] membranalGlobalForces, out mnl.Matrix<double>[] membranalLocalForces, out mnl.Matrix<double>[] membranalGlobalStress, out mnl.Matrix<double>[] membranalLocalStress, out mnl.Matrix<double>[] membranalGlobalEpsilon, out mnl.Matrix<double>[] membranalLocalEpsilon);
            //_flexural.GetNodesResults(flexuralGlobalDisplacements.ToArray(), out double[] flexuralLocalDisplacements, out mnl.Matrix<double>[] flexuralGlobalPseudoDisplacements, out mnl.Matrix<double>[] flexuralLocalPseudoDisplacements, out mnl.Matrix<double>[] flexuralGlobalForces, out mnl.Matrix<double>[] flexuralLocalForces, out mnl.Matrix<double>[] flexuralGlobalStress, out mnl.Matrix<double>[] flexuralLocalStress, out mnl.Matrix<double>[] flexuralGlobalEpsilon, out mnl.Matrix<double>[] flexuralLocalEpsilon);

            //sum of results
            localDisplacements = new double[5 * 3]; //dx, dy, dz, rx, ry
            //node 1
            localDisplacements[0] = membranalLocalDisplacements[0]; //dx
            localDisplacements[1] = membranalLocalDisplacements[1]; //dy
            //localDisplacements[2] = flexuralLocalDisplacements[0]; //dz
            //localDisplacements[3] = flexuralLocalDisplacements[1]; //rx
            //localDisplacements[4] = flexuralLocalDisplacements[2]; //ry

            //node 2
            localDisplacements[5] = membranalLocalDisplacements[2]; //dx
            localDisplacements[6] = membranalLocalDisplacements[3]; //dy
            //localDisplacements[7] = flexuralLocalDisplacements[3]; //dz
            //localDisplacements[8] = flexuralLocalDisplacements[4]; //rx
            //localDisplacements[9] = flexuralLocalDisplacements[5]; //ry

            //node 3
            localDisplacements[10] = membranalLocalDisplacements[4]; //dx
            localDisplacements[11] = membranalLocalDisplacements[5]; //dy
            //localDisplacements[12] = flexuralLocalDisplacements[6]; //dz
            //localDisplacements[13] = flexuralLocalDisplacements[7]; //rx
            //localDisplacements[14] = flexuralLocalDisplacements[8]; //ry

            localPseudoDeformation = new mnl.Matrix<double>[3] { //three nodes
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3), //couchy epsilon + couchy curvatures
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3),
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3)
            };

            globalPseudoDeformation = new mnl.Matrix<double>[3] { //three nodes
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3), //couchy epsilon + couchy curvatures
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3),
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3)
            };

            localForces = new mnl.Matrix<double>[3] { //three nodes
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3), //couchy F + couchy M
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3),
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3)
            };

            globalForces = new mnl.Matrix<double>[3] { //three nodes
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3), //couchy F + couchy M
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3),
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3)
            };

            globalStress = new mnl.Matrix<double>[3 * 2]; //3 nodes, top + bottom
            localStress = new mnl.Matrix<double>[3 * 2];

            globalEpsilon = new mnl.Matrix<double>[3 * 2];
            localEpsilon = new mnl.Matrix<double>[3 * 2];

            for (int node = 0; node < 3; node++)
            {
                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        localPseudoDeformation[node][r, c] = membranalLocalPseudoDisplacements[0][r, c]; //inplane epsilon
                        globalPseudoDeformation[node][r, c] = membranalGlobalPseudoDisplacements[0][r, c]; //inplane epsilon

                        localForces[node][r, c] = membranalLocalForces[0][r, c]; //inplane force
                        globalForces[node][r, c] = membranalGlobalForces[0][r, c]; //inplane force
                    }
                }

                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        //localPseudoDeformation[node][r + 3, c + 3] = flexuralLocalPseudoDisplacements[node][r, c]; //add curvature
                        //globalPseudoDeformation[node][r + 3, c + 3] = flexuralGlobalPseudoDisplacements[node][r, c]; //add curvature

                        //localForces[node][r + 3, c + 3] = flexuralLocalForces[0][r, c]; //add M
                        //globalForces[node][r + 3, c + 3] = flexuralGlobalForces[0][r, c]; //add M
                    }
                }

                //top
                //localStress[node] = membranalLocalStress[0] + flexuralLocalStress[node];
                //globalStress[node] = membranalGlobalStress[0] + flexuralGlobalStress[node];

                //localEpsilon[node] = membranalLocalEpsilon[0] + flexuralLocalEpsilon[node];
                //globalEpsilon[node] = membranalGlobalEpsilon[0] + flexuralGlobalEpsilon[node];

                //bottom
                //localStress[node + 3] = membranalLocalStress[0] + flexuralLocalStress[node + 3];
                //globalStress[node + 3] = membranalGlobalStress[0] + flexuralGlobalStress[node + 3];

                //localEpsilon[node + 3] = membranalLocalEpsilon[0] + flexuralLocalEpsilon[node + 3];
                //globalEpsilon[node + 3] = membranalGlobalEpsilon[0] + flexuralGlobalEpsilon[node +3];
            }
        }

        /// <summary>
        /// Get area of triangle from its nodes
        /// </summary>
        /// <param name="nds">3 Points3d</param>
        /// <returns></returns>
        public static double GetArea(Point3d[] nds)
        {
            mnl.Matrix<double> t1 = mnl.Matrix<double>.Build.Dense(3, 3);
            t1[0, 0] = nds[0].X;
            t1[0, 1] = nds[0].Y;
            t1[0, 2] = 1.0;

            t1[1, 0] = nds[1].X;
            t1[1, 1] = nds[1].Y;
            t1[1, 2] = 1.0;

            t1[2, 0] = nds[2].X;
            t1[2, 1] = nds[2].Y;
            t1[2, 2] = 1.0;

            return 0.5 * t1.Determinant();
        }

        /// <summary>
        /// According to article, order of nodes are ANTICLOCKWISE
        /// </summary>
        /// <returns></returns>
        public static Node[] GetLocalNodes(Node[] globalNode, out CoordinateSystem cSys)
        {
            #region CalculationOfLocalCoordinates
            //Search for 3 local axis
            //Local axes calculater clockwise
            Node nodeI = globalNode[0];
            Node nodeJ = globalNode[1];
            Node nodeK = globalNode[2];
            Vector3d x = new Vector3d(nodeJ.Position.X - nodeI.Position.X, nodeJ.Position.Y - nodeI.Position.Y, nodeJ.Position.Z - nodeI.Position.Z);
            Vector3d vecx = new Vector3d(x);
            vecx.Unitize(); //calculated along Node I -> Node J

            Vector3d y = new Vector3d(nodeK.Position.X - nodeI.Position.X, nodeK.Position.Y - nodeI.Position.Y, nodeK.Position.Z - nodeI.Position.Z);
            Vector3d vecy = new Vector3d(y);
            vecy.Unitize(); //calculated along Node K -> Node J

            Vector3d z = vecx.CrossProduct(vecy);
            Vector3d vecz = new Vector3d(z);
            vecz.Unitize();

            //recalculation of y that can be non-ortogonal
            y = z.CrossProduct(x);
            vecy = new Vector3d(y);
            vecy.Unitize(); //recalculated direction y
            cSys = new Geometry.CoordinateSystem(new Point3d(0, 0, 0), vecx, vecy);

            //move to local axis
            //calculation in local nodes
            Vector3d v12 = new Vector3d(nodeJ.Position.X - nodeI.Position.X, nodeJ.Position.Y - nodeI.Position.Y, nodeJ.Position.Z - nodeI.Position.Z);
            Vector3d v13 = new Vector3d(nodeK.Position.X - nodeI.Position.X, nodeK.Position.Y - nodeI.Position.Y, nodeK.Position.Z - nodeI.Position.Z);

            Node[] localNodes = new Node[3];
            localNodes[0] = new Node(Point3d.Origin, nodeI.Name); //Origin GlobalNodes.ElementAt(1 - 1);
            localNodes[1] = new Node(v12.DotProduct(vecx), v12.DotProduct(vecy), v12.DotProduct(vecz), nodeJ.Name); //Axis x GlobalNodes.ElementAt(2 - 1);
            localNodes[2] = new Node(v13.DotProduct(vecx), v13.DotProduct(vecy), v13.DotProduct(vecz), nodeK.Name); //GlobalNodes.ElementAt(3 - 1);
            #endregion
            return localNodes;
        }

        #region ShapeFunctions
        public static double GetShapeFunction(int index, double csi, double eta)
        {
            return LinearShapeFunctionsTri3.NaturalShapeFunction(index, csi, eta);
        }

        public static double GetdNdCsi(int index, double csi, double eta)
        {
            return LinearShapeFunctionsTri3.DNdCsi(index);
        }

        public static double GetdNdEta(int index, double csi, double eta)
        {
            return LinearShapeFunctionsTri3.DNdEta(index);
        }
        #endregion
    }
}
