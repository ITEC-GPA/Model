using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.FEM.Properties;
using GPC.Model.FEM.Attributes;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    /// <summary>
    /// A ROBUST QUADRILATERAL MEMBRANE FINITE ELEMENT WITH DRILLING DEGREES OF FREEDOM - ADNAN IBRAHIMBEGOVIC,* ROBERT L. TAYLOR’ AND EDWARD L. WILSON’ - 1990
    /// + THESIS REPORT - Analysis and Evaluation of a Shell Finite Element with Drilling Degree of Freedom - L.Jin, Austin
    /// </summary>
    public class Quad4MQ2IbraMembranal : Plate
    {
        #region variables
        Node[] _localNodes;
        #endregion

        public Quad4MQ2IbraMembranal(Node[] nodes, PlateProperty property, int id) : base(nodes, property, id)
        {
            _DOF.Add(LinearSolver.DOF.DX);
            _DOF.Add(LinearSolver.DOF.DY);
            _DOF.Add(LinearSolver.DOF.DZ);
            //a displacement in Local coordinate plane (Dx, Dy) can be a DX, DY, DZ in Global space!
            _DOF.Add(LinearSolver.DOF.RX);
            _DOF.Add(LinearSolver.DOF.RY);
            _DOF.Add(LinearSolver.DOF.RZ);
            //a rotation in Local coordinate plane (Rx, Ry) can be a RX, RY, RZ in Global space!

            //Local matrix: 4 nodes x 3(dX, dY, rZ) gdl = 12x12 matrix
            //Global matrix: 4 nodes x 6(DX, DY, DZ, RX, RY, RZ) gdl = 24x24 matrix

            //DofGlobalToLocal^T * kLocal * DofGlobalToLocal
            //   [24x12]           [12x12]     [12x24]
        }

        public override void BuildMatrix()
        {
            //Node 1 = Origin = Node i
            //Axis x assigned as Node 1 to Node 2, Node j = Node 2
            //Axis y ortogonal to axis x, Node k = node 3

            //calculation of matrix for transformation from Local to Global coordinates
            _localNodes = Quad4Element.LocalNodes(_nodesGlobal, out _localCoordinateSystem);
            _localNodes.ToList().ForEach(x => Console.WriteLine(x));
            
            #region TransformationMatrixLocalCoordinatesToGlobalCoordinates
            mnl.Matrix<double> dofGlobalToLocalTranspose = mnl.Matrix<double>.Build.Dense(24, 12);

            Vector3d globalX = new Vector3d(1.0, 0.0, 0.0);
            Vector3d globalY = new Vector3d(0.0, 1.0, 0.0);
            Vector3d globalZ = new Vector3d(0.0, 0.0, 1.0);

            Vector3d localX = LocalCoordinateSystem.V1;
            Vector3d localY = LocalCoordinateSystem.V2;
            Vector3d localZ = LocalCoordinateSystem.V3;

            #region localToGlobalNode1
            //local node1 x-displacement in global coordinate
            dofGlobalToLocalTranspose[0, 0] = localX.DotProduct(globalX);
            dofGlobalToLocalTranspose[1, 0] = localX.DotProduct(globalY);
            dofGlobalToLocalTranspose[2, 0] = localX.DotProduct(globalZ);

            //local node1 y-displacement in global coordinate
            dofGlobalToLocalTranspose[0, 1] = localY.DotProduct(globalX);
            dofGlobalToLocalTranspose[1, 1] = localY.DotProduct(globalY);
            dofGlobalToLocalTranspose[2, 1] = localY.DotProduct(globalZ);

            //local node1 z-rotation in global coordinate
            dofGlobalToLocalTranspose[3, 2] = localZ.DotProduct(globalX);
            dofGlobalToLocalTranspose[4, 2] = localZ.DotProduct(globalY);
            dofGlobalToLocalTranspose[5, 2] = localZ.DotProduct(globalZ);
            #endregion

            #region localToGlobalNode2
            //local node2 x-displacement in global coordinate
            dofGlobalToLocalTranspose[6, 3] = localX.DotProduct(globalX);
            dofGlobalToLocalTranspose[7, 3] = localX.DotProduct(globalY);
            dofGlobalToLocalTranspose[8, 3] = localX.DotProduct(globalZ);

            //local node2 y-displacement in global coordinate
            dofGlobalToLocalTranspose[6, 4] = localY.DotProduct(globalX);
            dofGlobalToLocalTranspose[7, 4] = localY.DotProduct(globalY);
            dofGlobalToLocalTranspose[8, 4] = localY.DotProduct(globalZ);

            //local node2 z-rotation in global coordinate
            dofGlobalToLocalTranspose[9, 5] = localZ.DotProduct(globalX);
            dofGlobalToLocalTranspose[10, 5] = localZ.DotProduct(globalY);
            dofGlobalToLocalTranspose[11, 5] = localZ.DotProduct(globalZ);
            #endregion

            #region localToGlobalNode3
            //local node3 x-displacement in global coordinate
            dofGlobalToLocalTranspose[12, 6] = localX.DotProduct(globalX);
            dofGlobalToLocalTranspose[13, 6] = localX.DotProduct(globalY);
            dofGlobalToLocalTranspose[14, 6] = localX.DotProduct(globalZ);

            //local node3 y-displacement in global coordinate
            dofGlobalToLocalTranspose[12, 7] = localY.DotProduct(globalX);
            dofGlobalToLocalTranspose[13, 7] = localY.DotProduct(globalY);
            dofGlobalToLocalTranspose[14, 7] = localY.DotProduct(globalZ);

            //local node3 z-rotation in global coordinate
            dofGlobalToLocalTranspose[15, 8] = localZ.DotProduct(globalX);
            dofGlobalToLocalTranspose[16, 8] = localZ.DotProduct(globalY);
            dofGlobalToLocalTranspose[17, 8] = localZ.DotProduct(globalZ);
            #endregion

            #region localToGlobalNode4
            //local node1 x-displacement in global coordinate
            dofGlobalToLocalTranspose[18, 9] = localX.DotProduct(globalX);
            dofGlobalToLocalTranspose[19, 9] = localX.DotProduct(globalY);
            dofGlobalToLocalTranspose[20, 9] = localX.DotProduct(globalZ);

            //local node1 y-displacement in global coordinate
            dofGlobalToLocalTranspose[18, 10] = localY.DotProduct(globalX);
            dofGlobalToLocalTranspose[19, 10] = localY.DotProduct(globalY);
            dofGlobalToLocalTranspose[20, 10] = localY.DotProduct(globalZ);

            //local node1 z-rotation in global coordinate
            dofGlobalToLocalTranspose[21, 11] = localZ.DotProduct(globalX);
            dofGlobalToLocalTranspose[22, 11] = localZ.DotProduct(globalY);
            dofGlobalToLocalTranspose[23, 11] = localZ.DotProduct(globalZ);
            #endregion
            _dofGlobalToLocal = dofGlobalToLocalTranspose.Transpose();

            /*Console.WriteLine("dofGlobalToLocalTranspose.");
            for (int r = 0; r < dofGlobalToLocalTranspose.RowCount; r++)
            {
                for (int c = 0; c < dofGlobalToLocalTranspose.ColumnCount; c++)
                {
                    Console.Write(dofGlobalToLocalTranspose[r,c] + " ");
                }
                Console.WriteLine();
            }*/
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
            /*Console.WriteLine();
            Console.WriteLine("D = " + _d.ToString());*/
            #endregion

            #region stiffnessMatrixInLocalCoordinates
            double thk = ((PlateProperty)_property).MembraneThickness;

            mnl.Matrix<double> kSymmetric = mnl.Matrix<double>.Build.Dense(12, 12);
            GaussIntegration.GaussPoint[] gaussPoints = GaussIntegration.GetPointsRectangular(9); // 9 by reference article 1990
            for (int i = 0; i < gaussPoints.Length; i++) //trhough the gauss points
            {
                double csi = gaussPoints[i].Point.X;
                double eta = gaussPoints[i].Point.Y;

                mnl.Matrix<double> BSymmetric = GetB(csi, eta);
                Console.WriteLine("BSymmetric(csi=" + csi + ",eta=" + eta + ") =");
                Util.WriteMatrix(BSymmetric, "F3");
                mnl.Matrix<double> m = BSymmetric.Transpose() * _d * BSymmetric;
                mnl.Matrix<double> jacob = Quad4Element.J(csi, eta, _localNodes);
                Console.WriteLine("J(csi=" + csi + ",eta=" + eta + ") = " + jacob);
                double detJ = jacob.Determinant();
                Console.WriteLine("detJ=" + detJ);

                kSymmetric = kSymmetric + gaussPoints[i].Weight * detJ * m;
            }
            kSymmetric = thk * kSymmetric;
            Console.WriteLine("k symm tensor =");
            Util.WriteMatrix(kSymmetric, "F3");

            //Matrix P ---> reference: eq. 38 of the Article 1990
            double rho = 1.0 * ((PlateProperty)_property).GetG();
            mnl.Matrix<double> P = mnl.Matrix<double>.Build.Dense(12, 12);
            gaussPoints = GaussIntegration.GetPointsRectangular(1); //1 gauss point reference article 1990
            for (int i = 0; i < gaussPoints.Length; i++) //trhough the gauss points
            {
                double csi = gaussPoints[i].Point.X;
                double eta = gaussPoints[i].Point.Y;

                #region createOfbSigned
                mnl.Matrix<double> bSigned = biVectorSigned(1, csi, eta);
                bSigned = bSigned.Append(biVectorSigned(2, csi, eta));
                bSigned = bSigned.Append(biVectorSigned(3, csi, eta));
                bSigned = bSigned.Append(biVectorSigned(4, csi, eta));
                Console.WriteLine("bSigned=");
                Util.WriteMatrix(bSigned,"F4");
                #endregion

                mnl.Matrix<double> jacob = Quad4Element.J(csi, eta, _localNodes);
                //Console.WriteLine("J(csi="+csi+",eta="+eta+") = " + jacob);
                double detJ = jacob.Determinant();
                Console.WriteLine("detJ=" + detJ);

                P = P + gaussPoints[i].Weight * detJ * bSigned.Transpose() * bSigned;
            }

            P = thk * rho * P;
            Console.WriteLine("rho = G = " + rho);
            Console.WriteLine("P skew tensor =");
            Util.WriteMatrix(P, "F3");
            _kElementLocalCoord = kSymmetric + P;
            Console.WriteLine("KElementLocalCoord = ");
            Util.WriteMatrix(_kElementLocalCoord, "F3");
            #endregion
        }

        public override mnl.Matrix<double> GetB(double csi, double eta, double zeta = 0)
        {
            /*
             * THESIS REPORT - Analysis and Evaluation of a Shell Finite Element with Drilling Degree of Freedom
             * pg. 27
             * */
            mnl.Matrix<double> B = mnl.Matrix<double>.Build.Dense(3, 0);
            B = B.Append(BiMatrixSigned(1, csi, eta));
            B = B.Append(BiMatrixSigned(2, csi, eta));
            B = B.Append(BiMatrixSigned(3, csi, eta));
            B = B.Append(BiMatrixSigned(4, csi, eta));

            Console.WriteLine("B(csi="+csi+",eta="+eta+") = ");
            Util.WriteMatrix(B, "F3");
            return B;
        }

        /// <summary>
        /// gestore indici da usare nella varie funzioni
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="k"></param>
        /// <param name="l"></param>
        /// <param name="m"></param>
        private static void getIndex(int i, out int j, out int k, out int l, out int m)
        {
            m = i + 4;
            l = i + 3;
            k = i + 1;
            if (i == 4)
            {
                k = 1;
            }
            if (i == 1)
            {
                l = 8;
            }
            j = l - 4;

            if (l != m - 1 + 4 * Math.Floor(1.0 / i))
            {
                throw new Exception("l");
            }
            if (k != (m % 4.0) + 1)
            {
                throw new Exception("k");
            }
            Console.WriteLine("i="+i + " j="+j + " k="+k+" l="+l+" m="+m);
        }

        private mnl.Matrix<double> biVectorSigned(int i, double csi, double eta)
        {
            /*
             * THESIS REPORT - Analysis and Evaluation of a Shell Finite Element with Drilling Degree of Freedom
             * pg. 25-26 
             */
            Console.WriteLine("Calculation of biVectorSigned, i = " + i);
            getIndex(i, out int j, out int k, out int l, out int m);
            /*Console.WriteLine("i ="+ i);
            Console.WriteLine("j =" + j);
            Console.WriteLine("k =" + k);
            Console.WriteLine("l =" + l);
            Console.WriteLine("m =" + m);*/

            double lij = Lij(_localNodes[i - 1].Position, _localNodes[j - 1].Position);
            /*double yij = Yij(_localNodes[i - 1].Position, _localNodes[j - 1].Position);
            Console.WriteLine("y" + i + j + "=" + yij);*/
            /*double xij = Xij(_localNodes[i - 1].Position, _localNodes[j - 1].Position);
            Console.WriteLine("x" + i + j + "=" + xij);
            double angleij = Math.Acos(-xij / lij) + Math.PI / 2.0; //outward vector wanted ///////////////////////////////ATTENTION
            Console.WriteLine("angle" + i + j + "=" + angleij * 180.0 / Math.PI);
            double cij = Math.Cos(angleij);// Cij(_localNodes[i - 1].Position, _localNodes[j - 1].Position);
            double sij = Math.Sin(angleij);// Sij(_localNodes[i - 1].Position, _localNodes[j - 1].Position);*/

            Vector3d vIJ = _localNodes[i - 1].Position - _localNodes[j - 1].Position;
            vIJ.Unitize();
            Vector3d nIJ = vIJ.CrossProduct(new Vector3d(0, 0, 1));
            Console.WriteLine(nIJ);

            double cij = nIJ.X;//Math.Cos(angleij);// Cij(_localNodes[i - 1].Position, _localNodes[j - 1].Position);
            double sij = nIJ.Y;//Math.Sin(angleij);// Sij(_localNodes[i - 1].Position, _localNodes[j - 1].Position);

            Console.WriteLine("l" + i + j + "=" + lij);
            Console.WriteLine("c" + i + j + "=" + cij);
            Console.WriteLine("s" + i + j + "=" + sij);
            Console.WriteLine();

            double lik = Lij(_localNodes[i - 1].Position, _localNodes[k - 1].Position);
            /*double yik = Yij(_localNodes[i - 1].Position, _localNodes[k - 1].Position);
            Console.WriteLine("y" + i + k + "=" + yik);*/
            /*double xik = Xij(_localNodes[i - 1].Position, _localNodes[k - 1].Position);
            Console.WriteLine("x" + i + k + "=" + xik);
            double angleik = Math.Acos(xik / lik) - Math.PI / 2.0;
            Console.WriteLine("angle" + i + k + "=" + angleik * 180.0 / Math.PI);
            double cik = Math.Cos(angleik); //Cij(_localNodes[i - 1].Position, _localNodes[k - 1].Position);
            double sik = Math.Sin(angleik); //Sij(_localNodes[i - 1].Position, _localNodes[k - 1].Position);*/

            Vector3d vKI = _localNodes[k - 1].Position - _localNodes[i - 1].Position;
            vKI.Unitize();
            Vector3d nKI = vKI.CrossProduct(new Vector3d(0, 0, 1));
            Console.WriteLine(nKI);

            double cik = nKI.X;// Math.Cos(angleik);// Cij(_localNodes[i - 1].Position, _localNodes[k - 1].Position);
            double sik = nKI.Y;// Math.Sin(angleik);// Sij(_localNodes[i - 1].Position, _localNodes[k - 1].Position);
            Console.WriteLine("l" + i + k + "=" + lik);
            Console.WriteLine("c" + i + k + "=" + cik);
            Console.WriteLine("s" + i + k + "=" + sik);
            Console.WriteLine();

            #region bVectorSigned
            #region bi
            #region dNi
            double dNidCsi = Quad4Element.dNdCsi4nodes(i, csi, eta);
            double dNidEta = Quad4Element.dNdEta4nodes(i, csi, eta);
            mnl.Vector<double> dNidNatural = mnl.Vector<double>.Build.Dense(2);
            dNidNatural[0] = dNidCsi;
            dNidNatural[1] = dNidEta;
            Console.WriteLine("dN" + i + "dNatural(csi=" + csi + ",eta=" + eta + ") = " + dNidNatural);

            mnl.Matrix<double> J = Quad4Element.J(csi, eta, _localNodes);
            mnl.Matrix<double> invJ = J.Inverse();
            Console.WriteLine("J(csi="+csi+",eta="+eta+") = " + J);

            mnl.Vector<double> dNidLocal = invJ * dNidNatural;
            double dNidX = dNidLocal[0];
            double dNidY = dNidLocal[1];
            Console.WriteLine("dN" + i + "dLocal(csi=" + csi + ",eta=" + eta + ") = " + dNidLocal);
            #endregion

            mnl.Matrix<double> bVectorSigned = mnl.Matrix<double>.Build.Dense(1, 3);
            bVectorSigned[0,0] = -1.0 / 2.0 * dNidY;
            bVectorSigned[0,1] = +1.0 / 2.0 * dNidX;
            Console.WriteLine("bVectorSigned" + i + " = " + bVectorSigned);
            #endregion

            #region gi
            #region dNl
            double dNldCsi = Quad8Element.dNdCsi(l, csi, eta);
            double dNldEta = Quad8Element.dNdEta(l, csi, eta);
            mnl.Vector<double> dNldNatural = mnl.Vector<double>.Build.Dense(2);
            dNldNatural[0] = dNldCsi;
            dNldNatural[1] = dNldEta;
            Console.WriteLine("dN" + l + "dNatural(csi=" + csi + ",eta=" + eta + ") = " + dNldNatural);

            mnl.Vector<double> dNldLocal = invJ * dNldNatural;
            double dNldX = dNldLocal[0];
            double dNldY = dNldLocal[1];
            Console.WriteLine("dN" + l + "dLocalXY(csi="+csi+",eta="+eta+") = " + dNldLocal);
            #endregion

            #region dNm
            double dNmdCsi = Quad8Element.dNdCsi(m, csi, eta);
            double dNmdEta = Quad8Element.dNdEta(m, csi, eta);
            mnl.Vector<double> dNmdNatural = mnl.Vector<double>.Build.Dense(2);
            dNmdNatural[0] = dNmdCsi;
            dNmdNatural[1] = dNmdEta;
            Console.WriteLine("dN" + m + "dNatural(csi=" + csi + ",eta=" + eta + ") = " + dNmdNatural);

            mnl.Vector<double> dNmdLocal = invJ * dNmdNatural;
            double dNmdX = dNmdLocal[0];
            double dNmdY = dNmdLocal[1];
            Console.WriteLine("dN" + m + "dLocalXY(csi=" + csi + ",eta=" + eta + ") = " + dNmdLocal);
            #endregion

            double gi = -1.0 / 16.0 * (lij * cij * dNldY - lik * cik * dNmdY) + 1.0 / 16.0 * (lij * sij * dNldX - lik * sik * dNmdX) - Quad4Element.N4nodes(i, csi, eta);
            Console.WriteLine("g" + i + " = " + gi);
            #endregion

            bVectorSigned[0, 2] = gi;
            #endregion
            Console.WriteLine("N" + i + "(csi = "+csi+", eta="+eta+") = " + Quad4Element.N4nodes(i, csi, eta));
            Console.WriteLine("b vector signed (csi="+csi+", eta="+eta+") = " + bVectorSigned); 

            return bVectorSigned;
        }

        /// <summary>
        /// Reference pg. 27
        /// </summary>
        /// <param name="i"></param>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <returns></returns>
        private mnl.Matrix<double> BiMatrixSigned(int i, double csi, double eta)
        {
            /*
             * THESIS REPORT - Analysis and Evaluation of a Shell Finite Element with Drilling Degree of Freedom
             * pg. 25-26 
             */
            Console.WriteLine("Calculation of BiMatrixSigned, i = " + i + "(csi=" + csi + ",eta="+eta+")");
            getIndex(i, out int j, out int k, out int l, out int m);

            Console.WriteLine("i, j");
            double lij = Lij(_localNodes[i - 1].Position, _localNodes[j - 1].Position);
            /*double yij = Yij(_localNodes[i - 1].Position, _localNodes[j - 1].Position);
            Console.WriteLine("y" + i + j + "=" + yij);
            double xij = Xij(_localNodes[i - 1].Position, _localNodes[j - 1].Position);
            Console.WriteLine("x" + i + j + "=" + xij);
            double angleij = Math.Acos(-xij / lij) + Math.PI / 2.0; //outward vector wanted ////////////ATTENTION
            Console.WriteLine("angle"+i+j+"="+angleij * 180.0 / Math.PI);*/

            Vector3d vIJ = _localNodes[i - 1].Position - _localNodes[j - 1].Position;
            vIJ.Unitize();
            Vector3d nIJ = vIJ.CrossProduct(new Vector3d(0, 0, 1));
 
            double cij = nIJ.X;//Math.Cos(angleij);// Cij(_localNodes[i - 1].Position, _localNodes[j - 1].Position);
            double sij = nIJ.Y;//Math.Sin(angleij);// Sij(_localNodes[i - 1].Position, _localNodes[j - 1].Position);
            Console.WriteLine("l" + i + j + "=" + lij);
            Console.WriteLine("c" + i + j + "=" + cij);
            Console.WriteLine("s" + i + j + "=" + sij);
            Console.WriteLine();

            Console.WriteLine("i, k");
            double lik = Lij(_localNodes[i - 1].Position, _localNodes[k - 1].Position);
            /*double yik = Yij(_localNodes[i - 1].Position, _localNodes[k - 1].Position);
            Console.WriteLine("y" + i + k + "=" + yik);
            double xik = Xij(_localNodes[i - 1].Position, _localNodes[k - 1].Position);
            Console.WriteLine("x" + i + k + "=" + xik);
            double angleik = Math.Acos(xik / lik) - Math.PI / 2.0;
            Console.WriteLine("angle" + i + k + "=" + angleik * 180.0 / Math.PI);*/

            Vector3d vKI = _localNodes[k - 1].Position - _localNodes[i - 1].Position;
            vKI.Unitize();
            Vector3d nKI = vKI.CrossProduct(new Vector3d(0, 0, 1));

            double cik = nKI.X;// Math.Cos(angleik);// Cij(_localNodes[i - 1].Position, _localNodes[k - 1].Position);
            double sik = nKI.Y;// Math.Sin(angleik);// Sij(_localNodes[i - 1].Position, _localNodes[k - 1].Position);
            Console.WriteLine("l" + i + k + "=" + lik);
            Console.WriteLine("c" + i + k + "=" + cik);
            Console.WriteLine("s" + i + k + "=" + sik);
            Console.WriteLine();

            double dNldCsi = Quad8Element.dNdCsi(l, csi, eta);
            double dNldEta = Quad8Element.dNdEta(l, csi, eta);
            mnl.Vector<double> dNldNatural = mnl.Vector<double>.Build.Dense(2);
            dNldNatural[0] = dNldCsi;
            dNldNatural[1] = dNldEta;
            Console.WriteLine("dN" + l + "dNatural(csi=" + csi + ",eta=" + eta + ") = " + dNldNatural);

            double dNmdCsi = Quad8Element.dNdCsi(m, csi, eta);
            double dNmdEta = Quad8Element.dNdEta(m, csi, eta);
            mnl.Vector<double> dNmdNatural = mnl.Vector<double>.Build.Dense(2);
            dNmdNatural[0] = dNmdCsi;
            dNmdNatural[1] = dNmdEta;
            Console.WriteLine("dN" + m + "dNatural(csi=" + csi + ",eta=" + eta + ") = " + dNmdNatural);

            mnl.Matrix<double> J = Quad4Element.J(csi, eta, _localNodes);
            mnl.Matrix<double> invJ = J.Inverse();
            Console.WriteLine("j(csi="+csi+",eta="+eta+")=" + J);

            mnl.Vector<double> dNldLocal = invJ * dNldNatural;
            double dNldX = dNldLocal[0];
            double dNldY = dNldLocal[1];
            Console.WriteLine("dN" + l + "dLocalXY(csi=" + csi + ",eta=" + eta + ") = " + dNldLocal);

            mnl.Vector<double> dNmdLocal = invJ * dNmdNatural;
            double dNmdX = dNmdLocal[0];
            double dNmdY = dNmdLocal[1];
            Console.WriteLine("dN" + m + "dLocalXY(csi=" + csi + ",eta=" + eta + ") = " + dNmdLocal);

            #region Gi
            mnl.Matrix<double> Gi = mnl.Matrix<double>.Build.Dense(3,1);
            Gi[1 - 1, 0] = 1.0 / 8.0 * (lij * cij * dNldX - lik * cik * dNmdX);
            Gi[2 - 1, 0] = 1.0 / 8.0 * (lij * sij * dNldY - lik * sik * dNmdY);
            Gi[3 - 1, 0] = 1.0 / 8.0 * (lij * cij * dNldY - lik * cik * dNmdY + lij * sij * dNldX - lik * sik * dNmdX);

            Console.WriteLine("G"+i+ "[3-1](csi=" + csi + ",eta=" + eta + ")=" + 1.0 / 8.0 +"*("+lij +"*" +cij+ "*" +dNldY +"-"+ lik +"*" +cik +"*" +dNmdY+ "+" +lij +"*" +sij+ "*" +dNldX+ "-" +lik+ "*" +sik+ "*" +dNmdX+") = "+ Gi[3 - 1, 0]);
            Console.WriteLine("G"+i +"(csi="+csi+",eta="+eta+")="+Gi);
            #endregion

            #region Bi
            mnl.Matrix<double> Bi = mnl.Matrix<double>.Build.Dense(3,2);
            double dNidCsi = Quad4Element.dNdCsi4nodes(i, csi, eta);
            double dNidEta = Quad4Element.dNdEta4nodes(i, csi, eta);
            mnl.Vector<double> dNidNatural = mnl.Vector<double>.Build.Dense(2);
            dNidNatural[0] = dNidCsi;
            dNidNatural[1] = dNidEta;
            Console.WriteLine("dN" + i + "dNatural(csi=" + csi + ",eta=" + eta + ") = " + dNidNatural);

            mnl.Vector<double> dNidLocal = invJ * dNidNatural;
            double dNidX = dNidLocal[0];
            double dNidY = dNidLocal[1];
            Console.WriteLine("dN" + i + "dLocalXY(csi=" + csi + ",eta=" + eta + ") = " + dNidLocal);

            Bi[0, 0] = dNidX;

            Bi[1, 1] = dNidY;

            Bi[2, 0] = dNidY;
            Bi[2, 1] = dNidX;
            #endregion

            mnl.Matrix<double> bSigned = Bi.Append(Gi);
            Console.WriteLine("bSigned" + i + "=" + bSigned);
            return bSigned;
        }

        private static double Xij(Point3d pi, Point3d pj)
        {
            return pj.X - pi.X;
        }

        private static double Yij(Point3d pi, Point3d pj)
        {
            return pj.Y - pi.Y;
        }

        private static double Lij(Point3d pi, Point3d pj)
        {
            double xij = Xij(pi, pj);
            double yij = Yij(pi, pj);
            return Math.Sqrt(xij * xij + yij * yij);
        }

        /*private static double Cij(Point3d pi, Point3d pj)
        {
            double lij = Lij(pi, pj);
            double yij = Yij(pi, pj);
            return yij / lij;
        }

        private static double Sij(Point3d pi, Point3d pj)
        {
            double lij = Lij(pi, pj);
            double xij = Xij(pi, pj);
            return xij / lij; // "+" sign in according to reference to An improved quadrilateral flat element with drilling degrees of freedom for shell structural analysis - H. Nguyen-Van1 , N. Mai-Duy1 and T. Tran-Cong1 - 2009 
        }*/
               
        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            // Occorre fare integrazione sulle funzioni di forma lineari di un quad4 (è possibile usare quella dell'elemento quad4 membranale)
            // l'integrazione delle funzione di forma Ni sul dominio dell'elemento è la quota parte della forza che va nel nodo i
            //esempio: F(nodo 1 = p * integrazione(N1 dcsi deta) = somma gauss N1(csi gauss, eta gauss) * detj(csi gauss, eta guass) * weightgauss
            mnl.Vector<double> _fLocalCoord = mnl.Vector<double>.Build.Dense(3 * Nodes.Length); //3 = DOF in local : DX, DY, RZ

            /*double thk = ((PlateProperty)Property).MembraneThickness;
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

                    GaussIntegration.GaussPoint[] gaussPoints = GaussIntegration.GetPointsRectangular(4);
                    mnl.Matrix<double> J4nodeElement;
                    for (int i = 0; i < gaussPoints.Length; i++)
                    {
                        double csi = gaussPoints[i].Point.X;
                        double eta = gaussPoints[i].Point.Y;
                        double gaussWeight = gaussPoints[i].Weight;

                        J4nodeElement = mnl.Matrix<double>.Build.Dense(2, 2);

                        for (int j = 0; j < _localNodes.Length; j++)
                        {
                            J4nodeElement[0, 0] = J4nodeElement[0, 0] + Quad4Element.dNdCsi4nodes(j + 1, csi, eta) * _localNodes[j].Position.X; // dx/dcsi
                            J4nodeElement[0, 1] = J4nodeElement[0, 1] + Quad4Element.dNdCsi4nodes(j + 1, csi, eta) * _localNodes[j].Position.Y; // dy/dcsi
                            J4nodeElement[1, 0] = J4nodeElement[1, 0] + Quad4Element.dNdEta4nodes(j + 1, csi, eta) * _localNodes[j].Position.X; // dx/deta
                            J4nodeElement[1, 1] = J4nodeElement[1, 1] + Quad4Element.dNdEta4nodes(j + 1, csi, eta) * _localNodes[j].Position.Y; // dy/deta
                        }
                        double detJ = J4nodeElement.Determinant();
                        /*Console.WriteLine("N1(" + csi + "," + eta + ") = " + Quad4Element.N4nodes(1, csi, eta));
                        Console.WriteLine("N2(" + csi + "," + eta + ") = " + Quad4Element.N4nodes(2, csi, eta));
                        Console.WriteLine("N3(" + csi + "," + eta + ") = " + Quad4Element.N4nodes(3, csi, eta));
                        Console.WriteLine("N4(" + csi + "," + eta + ") = " + Quad4Element.N4nodes(4, csi, eta));
                        Console.WriteLine("F: detJ(" + csi + "," + eta + ") = " + detJ);*/

                        /*_fLocalCoord[0] = _fLocalCoord[0] + Quad4Element.N4nodes(1, csi, eta) * detJ * gaussWeight * px; //node1
                        _fLocalCoord[1] = _fLocalCoord[1] + Quad4Element.N4nodes(1, csi, eta) * detJ * gaussWeight * py; //node1

                        _fLocalCoord[2] = _fLocalCoord[2] + Quad4Element.N4nodes(2, csi, eta) * detJ * gaussWeight * px; //node2
                        _fLocalCoord[3] = _fLocalCoord[3] + Quad4Element.N4nodes(2, csi, eta) * detJ * gaussWeight * py; //node2

                        _fLocalCoord[4] = _fLocalCoord[4] + Quad4Element.N4nodes(3, csi, eta) * detJ * gaussWeight * px; //node3
                        _fLocalCoord[5] = _fLocalCoord[5] + Quad4Element.N4nodes(3, csi, eta) * detJ * gaussWeight * py; //node3
                    
                        _fLocalCoord[6] = _fLocalCoord[6] + Quad4Element.N4nodes(4, csi, eta) * detJ * gaussWeight * px; //node4
                        _fLocalCoord[7] = _fLocalCoord[7] + Quad4Element.N4nodes(4, csi, eta) * detJ * gaussWeight * py; //node4
                    }
                }
            }*/
            return _fLocalCoord;
        }

        public override void GetNodesResults(double[] globalDisplacementsNodes, out double[] localDisplacements, out mnl.Matrix<double>[] globalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon)
        {
            localDisplacements = GetLocalDisplacement(globalDisplacementsNodes);
            mnl.Vector<double> vecLocalDispl = mnl.Vector<double>.Build.Dense(localDisplacements);

            //TODO: aggiornare
            
            Point2d[] naturalCoordNodes = new Point2d[4];
            naturalCoordNodes[0] = new Point2d(-1.0, -1.0);
            naturalCoordNodes[1] = new Point2d(+1.0, -1.0);
            naturalCoordNodes[2] = new Point2d(+1.0, +1.0);
            naturalCoordNodes[3] = new Point2d(-1.0, +1.0);

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

            mnl.Vector<double>[] epsilonLocal = new mnl.Vector<double>[4];
            mnl.Vector<double>[] stressLocal = new mnl.Vector<double>[4];

            mnl.Matrix<double>[] epsilonLocalCouchy = new mnl.Matrix<double>[4];
            mnl.Matrix<double>[] stressLocalCouchy = new mnl.Matrix<double>[4];

            mnl.Matrix<double>[] epsilonGlobalCouchy = new mnl.Matrix<double>[4];
            mnl.Matrix<double>[] stressGlobalCouchy = new mnl.Matrix<double>[4];

            globalForces = new mnl.Matrix<double>[4];
            localForces = new mnl.Matrix<double>[4];

            double thickness = ((PlateProperty)_property).MembraneThickness;

            for (int i = 0; i < naturalCoordNodes.Length; i++) {
                #region CalculationOfStressAndDeformationsInLocalCoordinates
                epsilonLocal[i] = GetB(naturalCoordNodes[i].X, naturalCoordNodes[i].Y) * vecLocalDispl; //epsilon_xx; epsilon_yy; epsilon_xy
                stressLocal[i] = D * epsilonLocal[i]; //sigma_xx; sigma_yy; tau_xy
                /*Console.WriteLine("Strains in Local coordinates:" + epsilon.ToString());
                Console.WriteLine("Stress in Local coordinates:" + stress.ToString());*/
                #endregion

                #region ConvertInGlobalCoordinates
                epsilonLocalCouchy[i] = mnl.Matrix<double>.Build.Dense(3, 3);
                epsilonLocalCouchy[i][0, 0] = epsilonLocal[i][0]; //epsilon_xx
                epsilonLocalCouchy[i][1, 1] = epsilonLocal[i][1]; //epsilon_yy

                epsilonLocalCouchy[i][0, 1] = epsilonLocal[i][2]; //epsilon_xy
                epsilonLocalCouchy[i][1, 0] = epsilonLocal[i][2]; //epsilon_yx
                                                            //epsilonCouchy[2, 2] = -ni / E * (sigma_xx + sigma_yy) + alpha * Temperature ; //epsilon_zz
                                                            //Console.WriteLine("Epsilon local coordinate:" + epsilonCouchy.ToString());

                stressLocalCouchy[i] = mnl.Matrix<double>.Build.Dense(3, 3);
                stressLocalCouchy[i][0, 0] = stressLocal[i][0]; //sigma_xx
                stressLocalCouchy[i][1, 1] = stressLocal[i][1]; //sigma_yy

                stressLocalCouchy[i][0, 1] = stressLocal[i][2]; //sigma_xy
                stressLocalCouchy[i][1, 0] = stressLocal[i][2]; //sigma_yx
                                                          //Console.WriteLine("Stress local coordinate:" + stressCouchy.ToString());

                //Second order tensor -> Trotated = Q * T * Q^T
                epsilonGlobalCouchy[i] = rotation * epsilonLocalCouchy[i] * rotation.Transpose();
                //Console.WriteLine("Epsilon in global coordinates = " + epsilonGlobalCouchy);

                stressGlobalCouchy[i] = rotation * stressLocalCouchy[i] * rotation.Transpose();
                //Console.WriteLine("Stress in global coordinates = " + stressGlobalCouchy);
                #endregion

                globalForces[i] = thickness * stressGlobalCouchy[i];
                localForces[i] = thickness * stressLocalCouchy[i];
            }

            localPseudoDeformation = epsilonLocalCouchy;
            globalPseudoDeformation = epsilonGlobalCouchy;

            localStress = stressLocalCouchy;
            globalStress = stressGlobalCouchy;

            globalEpsilon = epsilonGlobalCouchy;
            localEpsilon = stressGlobalCouchy;
        }
    }
}