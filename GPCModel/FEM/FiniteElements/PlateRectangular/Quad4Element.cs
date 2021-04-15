using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Properties;
using GPC.Utilities.Fem;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    public class Quad4Element : Plate
    {
        #region variables
        private Quad4GQ12Membranal _membranal;
        private Quad4DK _flexural;
        private mnl.Matrix<double> _kElementGlobalCoord; //the sum of the 2 stiffness matrix of TriangularMembranal and TriangularDK
        #endregion

        #region properties
        public override mnl.Matrix<double> KElementGlobalCoord => _kElementGlobalCoord;
        #endregion

        public Quad4Element(Node[] nodes) : base(nodes)
        {
            DOF.Add(Solver.DOF.DX);
            DOF.Add(Solver.DOF.DY);
            DOF.Add(Solver.DOF.DZ);
            DOF.Add(Solver.DOF.RX);
            DOF.Add(Solver.DOF.RY);
            DOF.Add(Solver.DOF.RZ);

            //kElementGlobal = 4 * 6 = 24
            _membranal = new  Quad4GQ12Membranal(nodes);
            //TODO: aggiungere Properietà
            _flexural = new Quad4DK(nodes);
            //TODO: aggiungere Properietà
        }

        internal Quad4Element(Node[] nodes, PlateProperty property) : this(nodes)
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

            _kElementGlobalCoord = mnl.Matrix<double>.Build.Dense(24, 24); //4 nodes x 6 dof = 24
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

            //3 x 3 of node1 to Node4
            rStartGlobal = 18;
            cStartGlobal = 0;
            rStartLocal = 9;
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

            //3 x 3 of node2 to Node4
            rStartGlobal = 18;
            cStartGlobal = 6;
            rStartLocal = 9;
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

            //3 x 3 of node3 to Node4
            rStartGlobal = 18;
            cStartGlobal = 12;
            rStartLocal = 9;
            cStartLocal = 6;
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

            //3x3 of Node 4
            rStartGlobal = 18;
            cStartGlobal = 18;
            rStartLocal = 9;
            cStartLocal = 9;
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

            //Node4
            globalStart = 18;
            localStart = 9;
            for (int i = 0; i < 3; i++)
            {
                f[globalStart + i] = f[globalStart + i] + fMembranal[localStart + i];
            }
            return f;
        }

        public override void AddLoadCaseAttribute(IPlateLoadCaseAttribute attribute)
        {
            //the attribute will add to the 2 finite element, Membrane and Discrete Kirchoff (DK). The attribute will have its impact in each finite element.
            //The nodal forces will be added
            _membranal.AddLoadCaseAttribute(attribute);
            _flexural.AddLoadCaseAttribute(attribute);
        }

        public override void GetNodesResults(double[] globalDisplacementsNodes, out double[] localDisplacements, out mnl.Matrix<double>[] globalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon)
        {

            mnl.Vector<double> membranalGlobalDisplacements = mnl.Vector<double>.Build.Dense(3 * 4); //in plane displacement can be in DX, DY, DZ in global coordinates
            
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

            //node 4
            startGlobal = 18;
            startLocal = 9;
            for (int j = 0; j < 3; j++)
            {
                membranalGlobalDisplacements[startLocal + j] = globalDisplacementsNodes[startGlobal + j];
            }

            mnl.Vector<double> flexuralGlobalDisplacements = mnl.Vector<double>.Build.Dense(globalDisplacementsNodes); //dz + rx + ry can be in DX, DY, DZ, RX, RY, RZ in global coordinates

            //get results
            _membranal.GetNodesResults(membranalGlobalDisplacements.ToArray(), out double[] membranalLocalDisplacements, out mnl.Matrix<double>[] membranalGlobalPseudoDisplacements, out mnl.Matrix<double>[] membranalLocalPseudoDisplacements, out mnl.Matrix<double>[] membranalGlobalForces, out mnl.Matrix<double>[] membranalLocalForces, out mnl.Matrix<double>[] membranalGlobalStress, out mnl.Matrix<double>[] membranalLocalStress, out mnl.Matrix<double>[] membranalGlobalEpsilon, out mnl.Matrix<double>[] membranalLocalEpsilon);
            _flexural.GetNodesResults(flexuralGlobalDisplacements.ToArray(), out double[] flexuralLocalDisplacements, out mnl.Matrix<double>[] flexuralGlobalPseudoDisplacements, out mnl.Matrix<double>[] flexuralLocalPseudoDisplacements, out mnl.Matrix<double>[] flexuralGlobalForces, out mnl.Matrix<double>[] flexuralLocalForces, out mnl.Matrix<double>[] flexuralGlobalStress, out mnl.Matrix<double>[] flexuralLocalStress, out mnl.Matrix<double>[] flexuralGlobalEpsilon, out mnl.Matrix<double>[] flexuralLocalEpsilon);

            //sum of results
            localDisplacements = new double[5 * 4]; //dx, dy, dz, rx, ry * 4 nodes
            //node 1
            localDisplacements[0] = membranalLocalDisplacements[0]; //dx
            localDisplacements[1] = membranalLocalDisplacements[1]; //dy
            localDisplacements[2] = flexuralLocalDisplacements[0]; //dz
            localDisplacements[3] = flexuralLocalDisplacements[1]; //rx
            localDisplacements[4] = flexuralLocalDisplacements[2]; //ry

            //node 2
            localDisplacements[5] = membranalLocalDisplacements[2]; //dx
            localDisplacements[6] = membranalLocalDisplacements[3]; //dy
            localDisplacements[7] = flexuralLocalDisplacements[3]; //dz
            localDisplacements[8] = flexuralLocalDisplacements[4]; //rx
            localDisplacements[9] = flexuralLocalDisplacements[5]; //ry

            //node 3
            localDisplacements[10] = membranalLocalDisplacements[4]; //dx
            localDisplacements[11] = membranalLocalDisplacements[5]; //dy
            localDisplacements[12] = flexuralLocalDisplacements[6]; //dz
            localDisplacements[13] = flexuralLocalDisplacements[7]; //rx
            localDisplacements[14] = flexuralLocalDisplacements[8]; //ry

            //node 4
            localDisplacements[15] = membranalLocalDisplacements[6]; //dx
            localDisplacements[16] = membranalLocalDisplacements[7]; //dy
            localDisplacements[17] = flexuralLocalDisplacements[9]; //dz
            localDisplacements[18] = flexuralLocalDisplacements[10]; //rx
            localDisplacements[19] = flexuralLocalDisplacements[11]; //ry

            localPseudoDeformation = new mnl.Matrix<double>[4] { //four nodes
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3), //couchy epsilon + couchy curvatures
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3),
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3),
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3)
            };

            globalPseudoDeformation = new mnl.Matrix<double>[4] { //four nodes
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3), //couchy epsilon + couchy curvatures
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3),
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3),
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3)
            };

            localForces = new mnl.Matrix<double>[4] { //four nodes
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3), //couchy F + couchy M
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3),
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3),
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3)
            };

            globalForces = new mnl.Matrix<double>[4] { //four nodes
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3), //couchy F + couchy M
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3),
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3),
                mnl.Matrix<double>.Build.Dense(3 + 3, 3 + 3)
            };

            globalStress = new mnl.Matrix<double>[4 * 3]; //4 nodes, top + center + bottom
            localStress = new mnl.Matrix<double>[4 * 3];

            globalEpsilon = new mnl.Matrix<double>[4 * 3];
            localEpsilon = new mnl.Matrix<double>[4 * 3];

            for (int node = 0; node < 4; node++) {
                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        localPseudoDeformation[node][r,c] = membranalLocalPseudoDisplacements[0][r,c]; //inplane epsilon
                        globalPseudoDeformation[node][r, c] = membranalGlobalPseudoDisplacements[0][r, c]; //inplane epsilon

                        localForces[node][r, c] = membranalLocalForces[0][r, c]; //inplane force
                        globalForces[node][r, c] = membranalGlobalForces[0][r, c]; //inplane force
                    }
                }

                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        localPseudoDeformation[node][r + 3, c + 3] = flexuralLocalPseudoDisplacements[node][r, c]; //add curvature
                        globalPseudoDeformation[node][r + 3, c + 3] = flexuralGlobalPseudoDisplacements[node][r, c]; //add curvature

                        localForces[node][r + 3, c + 3] = flexuralLocalForces[0][r, c]; //add M
                        globalForces[node][r + 3, c + 3] = flexuralGlobalForces[0][r, c]; //add M
                    }
                }

                //top
                localStress[node] = membranalLocalStress[0] + flexuralLocalStress[node];
                globalStress[node] = membranalGlobalStress[0] + flexuralGlobalStress[node];

                localEpsilon[node] = membranalLocalEpsilon[0] + flexuralLocalEpsilon[node];
                globalEpsilon[node] = membranalGlobalEpsilon[0] + flexuralGlobalEpsilon[node];

                //center
                localStress[node + 4] = membranalLocalStress[0];
                globalStress[node + 4] = membranalGlobalStress[0];

                localEpsilon[node + 4] = membranalLocalEpsilon[0];
                globalEpsilon[node + 4] = membranalGlobalEpsilon[0];

                //bottom
                localStress[node + 8] = membranalLocalStress[0] + flexuralLocalStress[node + 3];
                globalStress[node + 8] = membranalGlobalStress[0] + flexuralGlobalStress[node + 3];

                localEpsilon[node + 8] = membranalLocalEpsilon[0] + flexuralLocalEpsilon[node + 3];
                globalEpsilon[node + 8] = membranalGlobalEpsilon[0] + flexuralGlobalEpsilon[node +3];
            }           
        }

        #region PublicStaticFunction
        /// <summary>
        /// out Local Node in clockwise, centro nel primo nodo dell'elemento
        /// </summary>
        /// <returns></returns>
        public static Node[] GetLocalNodes(Node[] globalNodes, out CoordinateSystem cSys)
        {
            #region CalculationOfLocalCoordinates
            //Search for 3 local axis
            Node nodeI = globalNodes[0];
            Node nodeJ = globalNodes[1];
            Node nodeK = globalNodes[2];
            Node nodeL = globalNodes[3];

            Vector3d x = new Vector3d(nodeJ.Position.X - nodeI.Position.X, nodeJ.Position.Y - nodeI.Position.Y, nodeJ.Position.Z - nodeI.Position.Z);
            Vector3d vecx = new Vector3d(x);
            vecx.Unitize();

            Vector3d y = new Vector3d(nodeL.Position.X - nodeI.Position.X, nodeL.Position.Y - nodeI.Position.Y, nodeL.Position.Z - nodeI.Position.Z);
            Vector3d vecy = new Vector3d(y);
            vecy.Unitize();

            Vector3d z = x.CrossProduct(y);
            Vector3d vecz = new Vector3d(z);
            vecz.Unitize();

            //recalculation of y that can be non-ortogonal
            y = z.CrossProduct(x);
            vecy = new Vector3d(y);
            vecy.Unitize();
            //_vecXLocal = vecx.ToVector().ToArray();
            cSys = new CoordinateSystem(new Point3d(0, 0, 0), vecx, vecy);

            //move to local axis
            //calculation in local nodes
            Vector3d v12 = new Vector3d(nodeJ.Position.X - nodeI.Position.X, nodeJ.Position.Y - nodeI.Position.Y, nodeJ.Position.Z - nodeI.Position.Z);
            Vector3d v13 = new Vector3d(nodeK.Position.X - nodeI.Position.X, nodeK.Position.Y - nodeI.Position.Y, nodeK.Position.Z - nodeI.Position.Z);
            Vector3d v14 = new Vector3d(nodeL.Position.X - nodeI.Position.X, nodeL.Position.Y - nodeI.Position.Y, nodeL.Position.Z - nodeI.Position.Z);

            Node[] localNodes = new Node[4];
            localNodes[0] = new Node(0.0, 0, 0, nodeI.Name); //Origin GlobalNodes.ElementAt(1 - 1);
            localNodes[1] = new Node(v12.DotProduct(vecx), v12.DotProduct(vecy), v12.DotProduct(vecz), nodeJ.Name); //Axis x GlobalNodes.ElementAt(2 - 1);
            localNodes[2] = new Node(v13.DotProduct(vecx), v13.DotProduct(vecy), v13.DotProduct(vecz), nodeK.Name); //GlobalNodes.ElementAt(3 - 1);
            localNodes[3] = new Node(v14.DotProduct(vecx), v14.DotProduct(vecy), v14.DotProduct(vecz), nodeL.Name); //GlobalNodes.ElementAt(4 - 1);

            //controllo che nodi siano in ordine, orario o antiorario ma non in ordine sparso
            List<double> angles = localNodes.Select(p => Math.Atan(p.Position.Y / p.Position.X)).ToList();
            angles.RemoveAt(0); //primo nodo su se stesso -> NaN
            var anglesOrdered = angles.OrderBy(a => a).ToList();
            for (int i = 0; i < angles.Count; i++)
            {
                if (angles[i] != anglesOrdered[i])
                {
                    Console.WriteLine("Points unordered:");
                    globalNodes.ToList().ForEach(p => Console.WriteLine(p));
                    throw new Exception("Points unordered! :");
                    
                }
            }
            
            return localNodes;
            #endregion
        }

        /// <summary>
        /// Centro nel baricentro dell'elemento
        /// </summary>
        /// <param name="globalCoordinatesNodes"></param>
        /// <param name="cSys"></param>
        /// <returns></returns>
        public static Node[] GetLocalNodesFromCentroid(Node[] globalCoordinatesNodes, out CoordinateSystem cSys)
        {
            ///reference fig. 3
            //Search for 3 local axis
            Node[] global8Nodes = Quad4Element.Get8Nodes(globalCoordinatesNodes);

            Node nodeJ = global8Nodes[6 - 1];
            Node nodeL = global8Nodes[8 - 1];

            Vector3d x = new Vector3d(nodeJ.Position.X - nodeL.Position.X, nodeJ.Position.Y - nodeL.Position.Y, nodeJ.Position.Z - nodeL.Position.Z);
            Vector3d vecx = new Vector3d(x);
            vecx.Unitize();

            Node nodeK = global8Nodes[7 - 1];
            Node nodeI = global8Nodes[5 - 1];

            Vector3d y = new Vector3d(nodeK.Position.X - nodeI.Position.X, nodeK.Position.Y - nodeI.Position.Y, nodeK.Position.Z - nodeI.Position.Z);
            Vector3d vecy = new Vector3d(y);
            vecy.Unitize();

            Vector3d z = x.CrossProduct(y);
            Vector3d vecz = new Vector3d(z);
            vecz.Unitize();

            //recalculation of y that can be non-ortogonal
            y = z.CrossProduct(x);
            vecy = new Vector3d(y);
            vecy.Unitize();
            //_vecXLocal = vecx.ToVector().ToArray();
            cSys = new CoordinateSystem(new Point3d(0, 0, 0), vecx, vecy);

            //move to local axis
            //calculation in local nodes
            double Xg = globalCoordinatesNodes.ToList().Sum(p => p.Position.X) / 4.0;
            double Yg = globalCoordinatesNodes.ToList().Sum(p => p.Position.Y) / 4.0;
            double Zg = globalCoordinatesNodes.ToList().Sum(p => p.Position.Z) / 4.0;

            Node node1 = global8Nodes[1 - 1];
            Node node2 = global8Nodes[2 - 1];
            Node node3 = global8Nodes[3 - 1];
            Node node4 = global8Nodes[4 - 1];

            Vector3d vO1 = new Vector3d(node1.Position.X - Xg, node1.Position.Y - Yg, node1.Position.Z - Zg);
            Vector3d vO2 = new Vector3d(node2.Position.X - Xg, node2.Position.Y - Yg, node2.Position.Z - Zg);
            Vector3d vO3 = new Vector3d(node3.Position.X - Xg, node3.Position.Y - Yg, node3.Position.Z - Zg);
            Vector3d vO4 = new Vector3d(node4.Position.X - Xg, node4.Position.Y - Yg, node4.Position.Z - Zg);

            Node[] localNodes = new Node[4];
            localNodes[0] = new Node(vO1.DotProduct(vecx), vO1.DotProduct(vecy), vO1.DotProduct(vecz), node1.Name);
            localNodes[1] = new Node(vO2.DotProduct(vecx), vO2.DotProduct(vecy), vO2.DotProduct(vecz), node2.Name);
            localNodes[2] = new Node(vO3.DotProduct(vecx), vO3.DotProduct(vecy), vO3.DotProduct(vecz), node3.Name);
            localNodes[3] = new Node(vO4.DotProduct(vecx), vO4.DotProduct(vecy), vO4.DotProduct(vecz), node4.Name);

            return localNodes;
        }

        /// <summary>
        /// Convert 4 nodes in 8 nodes (only nodes / geometry)
        /// </summary>
        /// <param name="nodes4"></param>
        /// <returns></returns>
        public static Node[] Get8Nodes(Node[] nodes4)
        {
            Func<Node, Node, Node> middleNode = (Node n1, Node n2) => {

                Node n = new Node((n1.Position.X + n2.Position.X) / 2.0, (n1.Position.Y + n2.Position.Y) / 2.0, (n1.Position.Z + n2.Position.Z) / 2.0);
                return n;
            };

            Node[] nodes8 = new Node[8];
            nodes8[1 - 1] = nodes4[1 - 1];
            nodes8[2 - 1] = nodes4[2 - 1];
            nodes8[3 - 1] = nodes4[3 - 1];
            nodes8[4 - 1] = nodes4[4 - 1];
            nodes8[5 - 1] = middleNode(nodes4[2 - 1], nodes4[1 - 1]);
            nodes8[6 - 1] = middleNode(nodes4[3 - 1], nodes4[2 - 1]);
            nodes8[7 - 1] = middleNode(nodes4[4 - 1], nodes4[3 - 1]);
            nodes8[8 - 1] = middleNode(nodes4[1 - 1], nodes4[4 - 1]);

            return nodes8;
        }
        #endregion

        #region ShapeFunctions
        /// <summary>
        /// Funzioni di forma Quad4 lineare
        /// </summary>
        /// <param name="index">indice nodo 1-4</param>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <returns></returns>
        public static double GetShapeFunction(int index, double csi, double eta)
        {
            return LinearShapeFunctionQuad4.NaturalShapeFunction(index, csi, eta);
        }

        /// <summary>
        /// Derivate parziali delle funzioni di forma rispetto a csi
        /// </summary>
        /// <param name="index"></param>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <returns></returns>
        public static double GetdNdCsi(int index, double csi, double eta)
        {
            return LinearShapeFunctionQuad4.DNdCsi(index, csi, eta);
        }

        /// <summary>
        /// Derivate parziali delle funzioni di forma rispetto a eta
        /// </summary>
        /// <param name="index"></param>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <returns></returns>
        public static double GetdNdEta(int index, double csi, double eta)
        {
            return LinearShapeFunctionQuad4.DNdEta(index, csi, eta);
        }
        #endregion
    }
}
