using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Properties;
using GPC.Model.FreedomCases;
using GPC.Model.Sections;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    /// <summary>
    /// Based on Finite Element by Rao - Chapter 9
    /// </summary>
    public class EulerBeam : Beam
    {
        public EulerBeam(Node[] nodes, Section section, double axisAngleRadians = 0.0) : base(nodes)
        {
            _DOF.Add(Solver.DOF.DX);
            _DOF.Add(Solver.DOF.DY);
            _DOF.Add(Solver.DOF.DZ);
            _DOF.Add(Solver.DOF.RX);
            _DOF.Add(Solver.DOF.RY);
            _DOF.Add(Solver.DOF.RZ);

            _axisAngleRadians = axisAngleRadians; //rotazione rispetto asse 1-X
            SetProperty(section);
        }

        public override void BuildMatrix()
        {
            #region localStiffnessMatrix
            Section section = (Section)_property;
            double E = section.Material.E;
            double G = E / (2.0 * (1.0 + section.Material.Ni));
            double A = section.Area;
            double Jzz = section.Jyy;
            double Jyy = section.Jxx;
            double Jt = section.Jt;
            _length = _nodesGlobal[0].Position.DistanceTo(_nodesGlobal[1].Position);
            double L = _length;
            double L2 = _length * _length;
            double L3 = _length * _length * _length;

            _kElementLocalCoord = GetStiffnessBeam(L, A, Jyy, Jzz, Jt, E, G);

            #region ApplyReleases
            HashSet<Tuple<EndSide, LocalDOF>> releases = GetEndReleases();

            for (int i = 0; i < releases.Count(); i++)
            {
                EndSide EndBeam = releases.ElementAt(i).Item1;
                LocalDOF localDOFs = releases.ElementAt(i).Item2;

                if (EndBeam == EndSide.End1)
                {
                    switch (localDOFs)
                    {
                        case LocalDOF.AxialU1:
                            #region
                            _kElementLocalCoord[0, 0] = 0.0;

                            _kElementLocalCoord[6, 0] = 0.0;
                            _kElementLocalCoord[0, 6] = 0.0;

                            _kElementLocalCoord[6, 6] = 0.0;
                            #endregion
                            break;
                        case LocalDOF.TorsionR1:
                            #region
                            _kElementLocalCoord[3, 3] = 0.0;

                            _kElementLocalCoord[9, 3] = 0.0;
                            _kElementLocalCoord[3, 9] = 0.0;

                            _kElementLocalCoord[9, 9] = 0.0;
                            #endregion
                            break;
                        case LocalDOF.U2:
                            #region
                            #region
                            _kElementLocalCoord[1, 1] = 0.0;

                            _kElementLocalCoord[1, 5] = 0.0;
                            _kElementLocalCoord[5, 1] = 0.0;

                            _kElementLocalCoord[1, 7] = 0.0;
                            _kElementLocalCoord[7, 1] = 0.0;

                            _kElementLocalCoord[1, 11] = 0.0;
                            _kElementLocalCoord[11, 1] = 0.0;
                            #endregion

                            #region
                            _kElementLocalCoord[5, 5] = (_kElementLocalCoord[5, 5] == 0.0) ? 0.0 : E * Jzz / L;

                            _kElementLocalCoord[5, 7] = 0.0;
                            _kElementLocalCoord[7, 5] = 0.0;

                            _kElementLocalCoord[5, 11] = (_kElementLocalCoord[5, 11] == 0.0) ? 0.0 : -E * Jzz / L;
                            _kElementLocalCoord[11, 5] = (_kElementLocalCoord[11, 5] == 0.0) ? 0.0 : -E * Jzz / L;
                            #endregion

                            #region
                            _kElementLocalCoord[7, 7] = 0.0;

                            _kElementLocalCoord[7, 11] = 0.0;
                            _kElementLocalCoord[11, 7] = 0.0;
                            #endregion

                            #region
                            _kElementLocalCoord[11, 11] = (_kElementLocalCoord[11, 11] == 0.0) ? 0.0 : E * Jzz / L;
                            #endregion
                            #endregion
                            break;
                        case LocalDOF.U3:
                            #region
                            #region
                            _kElementLocalCoord[2, 2] = 0.0;

                            _kElementLocalCoord[2, 4] = 0.0;
                            _kElementLocalCoord[4, 2] = 0.0;

                            _kElementLocalCoord[2, 8] = 0.0;
                            _kElementLocalCoord[8, 2] = 0.0;

                            _kElementLocalCoord[2, 10] = 0.0;
                            _kElementLocalCoord[10, 2] = 0.0;
                            #endregion

                            #region
                            _kElementLocalCoord[4, 4] = (_kElementLocalCoord[4, 4] == 0.0) ? 0.0 : E * Jyy / L;

                            _kElementLocalCoord[4, 8] = 0.0;
                            _kElementLocalCoord[8, 4] = 0.0;

                            _kElementLocalCoord[4, 10] = (_kElementLocalCoord[4, 10] == 0.0) ? 0.0 : -E * Jyy / L;
                            _kElementLocalCoord[10, 4] = (_kElementLocalCoord[10, 4] == 0.0) ? 0.0 : -E * Jyy / L;
                            #endregion

                            #region
                            _kElementLocalCoord[8, 8] = 0.0;

                            _kElementLocalCoord[8, 10] = 0.0;
                            _kElementLocalCoord[10, 8] = 0.0;
                            #endregion

                            #region
                            _kElementLocalCoord[10, 10] = (_kElementLocalCoord[10, 10] == 0.0) ? 0.0 : E * Jyy / L;
                            #endregion
                            #endregion
                            break;
                        case LocalDOF.R2:
                            #region
                            #region
                            _kElementLocalCoord[4, 4] = 0.0;

                            _kElementLocalCoord[4, 2] = 0.0;
                            _kElementLocalCoord[2, 4] = 0.0;

                            _kElementLocalCoord[4, 8] = 0.0;
                            _kElementLocalCoord[8, 4] = 0.0;

                            _kElementLocalCoord[4, 10] = 0.0;
                            _kElementLocalCoord[10, 4] = 0.0;
                            #endregion

                            #region
                            _kElementLocalCoord[2, 2] = (_kElementLocalCoord[2, 2] == 0.0) ? 0.0 : 3.0 * E * Jyy / L3;

                            _kElementLocalCoord[2, 8] = (_kElementLocalCoord[2, 8] == 0.0) ? 0.0 : -3.0 * E * Jyy / L3;
                            _kElementLocalCoord[8, 2] = (_kElementLocalCoord[8, 2] == 0.0) ? 0.0 : -3.0 * E * Jyy / L3;

                            _kElementLocalCoord[2, 10] = (_kElementLocalCoord[2, 10] == 0.0) ? 0.0 : -3.0 * E * Jyy / L2;
                            _kElementLocalCoord[10, 2] = (_kElementLocalCoord[10, 2] == 0.0) ? 0.0 : -3.0 * E * Jyy / L2;
                            #endregion

                            #region
                            _kElementLocalCoord[8, 8] = (_kElementLocalCoord[8, 8] == 0.0) ? 0.0 : 3.0 * E * Jyy / L3;

                            _kElementLocalCoord[8, 10] = (_kElementLocalCoord[8, 10] == 0.0) ? 0.0 : 3.0 * E * Jyy / L2;
                            _kElementLocalCoord[10, 8] = (_kElementLocalCoord[10, 8] == 0.0) ? 0.0 : 3.0 * E * Jyy / L2;
                            #endregion

                            #region
                            _kElementLocalCoord[10, 10] = (_kElementLocalCoord[10, 10] == 0.0) ? 0.0 : 3.0 * E * Jyy / L;
                            #endregion
                            #endregion
                            break;
                        case LocalDOF.R3:
                            #region
                            #region
                            _kElementLocalCoord[5, 5] = 0.0;

                            _kElementLocalCoord[5, 1] = 0.0;
                            _kElementLocalCoord[1, 5] = 0.0;

                            _kElementLocalCoord[5, 7] = 0.0;
                            _kElementLocalCoord[7, 5] = 0.0;

                            _kElementLocalCoord[5, 11] = 0.0;
                            _kElementLocalCoord[11, 5] = 0.0;
                            #endregion

                            #region
                            _kElementLocalCoord[1, 1] = (_kElementLocalCoord[1, 1] == 0.0) ? 0.0 : 3.0 * E * Jzz / L3;

                            _kElementLocalCoord[1, 7] = (_kElementLocalCoord[1, 7] == 0.0) ? 0.0 : -3.0 * E * Jzz / L3;
                            _kElementLocalCoord[7, 1] = (_kElementLocalCoord[7, 1] == 0.0) ? 0.0 : -3.0 * E * Jzz / L3;

                            _kElementLocalCoord[1, 11] = (_kElementLocalCoord[1, 11] == 0.0) ? 0.0 : 3.0 * E * Jzz / L2;
                            _kElementLocalCoord[11, 1] = (_kElementLocalCoord[11, 1] == 0.0) ? 0.0 : 3.0 * E * Jzz / L2;
                            #endregion

                            #region
                            _kElementLocalCoord[7, 7] = (_kElementLocalCoord[7, 7] == 0.0) ? 0.0 : 3.0 * E * Jzz / L3;

                            _kElementLocalCoord[7, 11] = (_kElementLocalCoord[7, 11] == 0.0) ? 0.0 : -3.0 * E * Jzz / L2;
                            _kElementLocalCoord[11, 7] = (_kElementLocalCoord[11, 7] == 0.0) ? 0.0 : -3.0 * E * Jzz / L2;
                            #endregion

                            #region
                            _kElementLocalCoord[11, 11] = (_kElementLocalCoord[11, 11] == 0.0) ? 0.0 : 3.0 * E * Jzz / L;
                            #endregion
                            #endregion
                            break;
                    }
                }
                else if (EndBeam == EndSide.End2)
                {
                    switch (localDOFs)
                    {
                        case LocalDOF.AxialU1:
                            #region
                            _kElementLocalCoord[6, 6] = 0.0;

                            _kElementLocalCoord[6, 0] = 0.0;
                            _kElementLocalCoord[0, 6] = 0.0;

                            _kElementLocalCoord[0, 0] = 0.0;
                            #endregion
                            break;
                        case LocalDOF.TorsionR1:
                            #region
                            _kElementLocalCoord[9, 9] = 0.0;

                            _kElementLocalCoord[9, 3] = 0.0;
                            _kElementLocalCoord[3, 9] = 0.0;

                            _kElementLocalCoord[3, 3] = 0.0;
                            #endregion
                            break;
                        case LocalDOF.U2:
                            #region
                            #region
                            _kElementLocalCoord[7, 7] = 0.0;

                            _kElementLocalCoord[7, 5] = 0.0;
                            _kElementLocalCoord[5, 7] = 0.0;

                            _kElementLocalCoord[7, 1] = 0.0;
                            _kElementLocalCoord[1, 7] = 0.0;

                            _kElementLocalCoord[7, 11] = 0.0;
                            _kElementLocalCoord[11, 7] = 0.0;
                            #endregion

                            #region
                            _kElementLocalCoord[11, 11] = (_kElementLocalCoord[11, 11] == 0.0) ? 0.0 : E * Jzz / L;

                            _kElementLocalCoord[11, 5] = (_kElementLocalCoord[11, 5] == 0.0) ? 0.0 : -E * Jzz / L;
                            _kElementLocalCoord[5, 11] = (_kElementLocalCoord[5, 11] == 0.0) ? 0.0 : -E * Jzz / L;

                            _kElementLocalCoord[1, 11] = 0.0;
                            _kElementLocalCoord[11, 1] = 0.0;
                            #endregion

                            #region
                            _kElementLocalCoord[5, 5] = (_kElementLocalCoord[5, 5] == 0.0) ? 0.0 : E * Jzz / L;

                            _kElementLocalCoord[5, 1] = 0.0;
                            _kElementLocalCoord[1, 5] = 0.0;
                            #endregion

                            #region
                            _kElementLocalCoord[1, 1] = 0.0;
                            #endregion
                            #endregion
                            break;
                        case LocalDOF.U3:
                            #region
                            #region
                            _kElementLocalCoord[8, 8] = 0.0;

                            _kElementLocalCoord[8, 2] = 0.0;
                            _kElementLocalCoord[2, 8] = 0.0;

                            _kElementLocalCoord[8, 4] = 0.0;
                            _kElementLocalCoord[4, 8] = 0.0;

                            _kElementLocalCoord[8, 10] = 0.0;
                            _kElementLocalCoord[10, 8] = 0.0;
                            #endregion

                            #region
                            _kElementLocalCoord[2, 2] = 0.0;

                            _kElementLocalCoord[2, 4] = 0.0;
                            _kElementLocalCoord[4, 2] = 0.0;

                            _kElementLocalCoord[2, 10] = 0.0;
                            _kElementLocalCoord[10, 2] = 0.0;
                            #endregion

                            #region
                            _kElementLocalCoord[4, 4] = (_kElementLocalCoord[4, 4] == 0.0) ? 0.0 : E * Jyy / L;

                            _kElementLocalCoord[4, 10] = (_kElementLocalCoord[4, 10] == 0.0) ? 0.0 : -E * Jyy / L;
                            _kElementLocalCoord[10, 4] = (_kElementLocalCoord[10, 4] == 0.0) ? 0.0 : -E * Jyy / L;
                            #endregion

                            #region
                            _kElementLocalCoord[10, 10] = (_kElementLocalCoord[10, 10] == 0.0) ? 0.0 : E * Jyy / L;
                            #endregion
                            #endregion
                            break;
                        case LocalDOF.R2:
                            #region
                            #region
                            _kElementLocalCoord[10, 10] = 0.0;

                            _kElementLocalCoord[10, 2] = 0.0;
                            _kElementLocalCoord[2, 10] = 0.0;

                            _kElementLocalCoord[10, 4] = 0.0;
                            _kElementLocalCoord[4, 10] = 0.0;

                            _kElementLocalCoord[10, 8] = 0.0;
                            _kElementLocalCoord[8, 10] = 0.0;
                            #endregion

                            #region
                            _kElementLocalCoord[2, 2] = (_kElementLocalCoord[2, 2] == 0.0) ? 0.0 : 3.0 * E * Jyy / L3;

                            _kElementLocalCoord[2, 4] = (_kElementLocalCoord[4, 2] == 0.0) ? 0.0 : -3.0 * E * Jyy / L2;
                            _kElementLocalCoord[4, 2] = (_kElementLocalCoord[2, 4] == 0.0) ? 0.0 : -3.0 * E * Jyy / L2;

                            _kElementLocalCoord[2, 8] = (_kElementLocalCoord[4, 8] == 0.0) ? 0.0 : -3.0 * E * Jyy / L3;
                            _kElementLocalCoord[8, 2] = (_kElementLocalCoord[8, 4] == 0.0) ? 0.0 : -3.0 * E * Jyy / L3;
                            #endregion

                            #region
                            _kElementLocalCoord[4, 4] = (_kElementLocalCoord[4, 4] == 0.0) ? 0.0 : 3.0 * E * Jyy / L;

                            _kElementLocalCoord[4, 8] = (_kElementLocalCoord[4, 8] == 0.0) ? 0.0 : 3.0 * E * Jyy / L2;
                            _kElementLocalCoord[8, 4] = (_kElementLocalCoord[8, 4] == 0.0) ? 0.0 : 3.0 * E * Jyy / L2;
                            #endregion

                            #region
                            _kElementLocalCoord[8, 8] = (_kElementLocalCoord[8, 8] == 0.0) ? 0.0 : 3.0 * E * Jyy / L3;
                            #endregion
                            #endregion
                            break;
                        case LocalDOF.R3:
                            #region
                            #region
                            _kElementLocalCoord[11, 11] = 0.0;

                            _kElementLocalCoord[11, 1] = 0.0;
                            _kElementLocalCoord[1, 11] = 0.0;

                            _kElementLocalCoord[11, 5] = 0.0;
                            _kElementLocalCoord[5, 11] = 0.0;

                            _kElementLocalCoord[11, 7] = 0.0;
                            _kElementLocalCoord[7, 11] = 0.0;
                            #endregion

                            #region
                            _kElementLocalCoord[1, 1] = (_kElementLocalCoord[1, 1] == 0.0) ? 0.0 : 3.0 * E * Jzz / L3;

                            _kElementLocalCoord[1, 5] = (_kElementLocalCoord[1, 5] == 0.0) ? 0.0 : 3.0 * E * Jzz / L2;
                            _kElementLocalCoord[5, 1] = (_kElementLocalCoord[5, 1] == 0.0) ? 0.0 : 3.0 * E * Jzz / L2;

                            _kElementLocalCoord[1, 7] = (_kElementLocalCoord[1, 7] == 0.0) ? 0.0 : -3.0 * E * Jzz / L3;
                            _kElementLocalCoord[7, 1] = (_kElementLocalCoord[7, 1] == 0.0) ? 0.0 : -3.0 * E * Jzz / L3;
                            #endregion

                            #region
                            _kElementLocalCoord[5, 5] = (_kElementLocalCoord[5, 5] == 0.0) ? 0.0 : 3.0 * E * Jzz / L;

                            _kElementLocalCoord[5, 7] = (_kElementLocalCoord[5, 7] == 0.0) ? 0.0 : -3.0 * E * Jzz / L2;
                            _kElementLocalCoord[7, 5] = (_kElementLocalCoord[7, 5] == 0.0) ? 0.0 : -3.0 * E * Jzz / L2;
                            #endregion

                            #region
                            _kElementLocalCoord[7, 7] = (_kElementLocalCoord[7, 7] == 0.0) ? 0.0 : 3.0 * E * Jzz / L3;
                            #endregion
                            #endregion
                            break;
                    }
                }
            }

            #region ShearU2AndRotationR3
            if (IsReleasedU3AndR2(EndSide.End1) || IsReleasedU3AndR2(EndSide.End2))
            {
                for (int col = 0; col < _kElementLocalCoord.ColumnCount; col++)
                {
                    _kElementLocalCoord[1, col] = 0.0;
                    _kElementLocalCoord[7, col] = 0.0;

                    _kElementLocalCoord[5, col] = 0.0;
                    _kElementLocalCoord[11, col] = 0.0;
                }
            }
            #endregion


            #region ShearU3AndRotationR2
            if (IsReleasedU3AndR2(EndSide.End1) || IsReleasedU3AndR2(EndSide.End2))
            {
                for (int col = 0; col < _kElementLocalCoord.ColumnCount; col++)
                {
                    _kElementLocalCoord[2, col] = 0.0;
                    _kElementLocalCoord[8, col] = 0.0;

                    _kElementLocalCoord[4, col] = 0.0;
                    _kElementLocalCoord[10, col] = 0.0;
                }
            }
            #endregion

            #region ApplyReleaseToTruss
            #region duobleReleaseR2
            if (IsDoubleReleasedR2() == true)
            {
                for (int col = 0; col < _kElementLocalCoord.ColumnCount; col++)
                {
                    _kElementLocalCoord[3-1, col] = 0.0;
                    _kElementLocalCoord[9-1, col] = 0.0;
                }
            }
            #endregion

            #region duobleReleaseR3
            if (IsDoubleReleasedR3() == true)
            {
                for (int col = 0; col < _kElementLocalCoord.ColumnCount; col++)
                {
                    _kElementLocalCoord[2-1, col] = 0.0;
                    _kElementLocalCoord[8-1, col] = 0.0;
                }
            }
            #endregion
            #endregion

            #endregion
            
            /*Console.WriteLine("kLocal");
            FEMUtilities.WriteMatrix(_kElementLocalCoord, "F2");*/
            #endregion

            #region transformationToGlobal
            double xj = _nodesGlobal[1].Position.X;
            double xi = _nodesGlobal[0].Position.X;

            double yj = _nodesGlobal[1].Position.Y;
            double yi = _nodesGlobal[0].Position.Y;

            double zj = _nodesGlobal[1].Position.Z;
            double zi = _nodesGlobal[0].Position.Z;

            double lox = (xj - xi) / L;
            double mox = (yj - yi) / L;
            double nox = (zj - zi) / L;

            double d = Math.Sqrt(lox * lox + nox * nox);

            mnl.Matrix<double> lambda1 = mnl.Matrix<double>.Build.Dense(3, 3); //eq. 9.57
            lambda1[0, 0] = lox;
            lambda1[0, 1] = mox;
            lambda1[0, 2] = nox;

            lambda1[1, 0] = -(lox * mox) / d; //loy
            lambda1[1, 1] = (lox * lox + nox * nox) / d; //moy
            lambda1[1, 2] = -(mox * nox)/d; //noz

            lambda1[2, 0] = -nox / d; //loz
            lambda1[2, 1] = 0.0; //moz
            lambda1[2, 2] = lox / d; //noz

            /*Console.WriteLine("lambda1");
            FEMUtilities.WriteMatrix(lambda1);*/

            mnl.Matrix<double> lambda2 = mnl.Matrix<double>.Build.Dense(3, 3); //eq. 9.58
            lambda2[0, 0] = 1.0;

            lambda2[1, 1] = Math.Cos(_axisAngleRadians);
            lambda2[1, 2] = Math.Sin(_axisAngleRadians);

            lambda2[2, 1] = -Math.Sin(_axisAngleRadians);
            lambda2[2, 2] = Math.Cos(_axisAngleRadians);

            /*Console.WriteLine("lambda2");
            FEMUtilities.WriteMatrix(lambda2);*/

            mnl.Matrix<double> lambda = mnl.Matrix<double>.Build.Dense(3, 3);
            if (lox == 0.0 && nox == 0.0)
            {
                //eq. 9.60
                lambda[0, 1] = mox;

                lambda[1, 0] = -mox * Math.Cos(_axisAngleRadians);
                lambda[1, 2] = mox * Math.Sin(_axisAngleRadians);

                lambda[2, 0] = Math.Sin(_axisAngleRadians);
                lambda[2, 2] = Math.Cos(_axisAngleRadians);
            }
            else
            {
                lambda = lambda2 * lambda1; //eq. 9.48
            }   
            /*Console.WriteLine("lambda");
            FEMUtilities.WriteMatrix(lambda);*/

            _dofGlobalToLocal = mnl.Matrix<double>.Build.Dense(12, 12);
            for (int i = 0; i < 4; i++)
            {
                for (int row = 0; row < lambda.RowCount; row++)
                {
                    for (int col = 0; col < lambda.ColumnCount; col++)
                    {
                        _dofGlobalToLocal[ 3 * i + row, 3 * i + col] = lambda[row, col];
                    }
                }
                
            }
            /*Console.WriteLine("localToGlobal");
            FEMUtilities.WriteMatrix(_dofGlobalToLocal);*/

            Vector3d ux = new Vector3d(_dofGlobalToLocal[0, 0], _dofGlobalToLocal[0, 1], _dofGlobalToLocal[0, 2]);
            Vector3d uy = new Vector3d(_dofGlobalToLocal[1, 0], _dofGlobalToLocal[1, 1], _dofGlobalToLocal[1, 2]);
            //Vector3d uz = new Vector3d(_dofGlobalToLocal[2, 0], _dofGlobalToLocal[2, 1], _dofGlobalToLocal[2, 2]);

            _localCoordinateSystem = new CoordinateSystem(new Point3d(0, 0, 0), ux, uy);
            Console.WriteLine("Local System Beam");
            Console.WriteLine("ux = " + _localCoordinateSystem.V1);
            Console.WriteLine("uy = " + _localCoordinateSystem.V2);
            Console.WriteLine("uz = " + _localCoordinateSystem.V3);
            #endregion
        }

        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            mnl.Vector<double> fLocal = mnl.Vector<double>.Build.Dense(12);
            //Axial: eq. 9.16-9.17 - The Finite Element Method in Engineering - S.Rao

            double q1 = 0;
            double q2 = 0;
            double q3 = 0;
            for (int i = 0; i < _attributesLoadCase.Count; i++)
            {
                if (_attributesLoadCase[i].GetType() == typeof(BeamDistribuitedLoadAttribute))
                {
                    BeamDistribuitedLoadAttribute q = (BeamDistribuitedLoadAttribute)_attributesLoadCase[i];
                    q1 += q.Q1;
                    q2 += q.Q2;
                    q3 += q.Q3;
                }
                else
                {
                    //TODO: gestione coordinate system
                    q1 = 0;
                    q2 = 0;
                    q3 = 0;
                }
            }

            #region NoEndRelease
            #region axial
            fLocal[0] = q1 * _length / 2.0;
            fLocal[6] = q1 * _length / 2.0;
            #endregion

            #region q2
            fLocal[1] = q2 * _length / 2.0;
            fLocal[7] = q2 * _length / 2.0;

            fLocal[5] = q2 * _length * _length / 12.0;
            fLocal[11] = -q2 * _length * _length / 12.0;
            #endregion

            #region q3
            fLocal[2] = q3 * _length / 2.0;
            fLocal[8] = q3 * _length / 2.0;

            fLocal[4] = -q3 * _length * _length / 12.0;
            fLocal[10] = q3 * _length * _length / 12.0;
            #endregion
            #endregion

            #region ApplyReleases
            foreach (BeamReleasesAttribute rel in _attributesFreedomCase)
            {
                EndSide EndBeam = rel.EndBeam;
                LocalDOF[] localDOFs = rel.LocalDOFReleased;

                if (EndBeam == EndSide.End1)
                {
                    for (int i = 0; i < localDOFs.Length; i++)
                    {
                        switch (localDOFs[i])
                        {
                            case LocalDOF.AxialU1:
                                fLocal[0] = 0.0;
                                fLocal[6] = q1 * _length;
                                break;
                            case LocalDOF.TorsionR1:
                                //TODO
                                break;
                            case LocalDOF.U2:
                                fLocal[2 - 1] = 0.0;
                                fLocal[6 - 1] = -BendingFixAndBipendolumUniformLoad(_length - 0.0, q2, _length);

                                fLocal[8 - 1] = ShearFixAndBipendolumUniformLoad(0.0, q2, _length);
                                fLocal[12 - 1] = BendingFixAndBipendolumUniformLoad(0.0, q2, _length);
                                break;
                            case LocalDOF.U3:
                                fLocal[3 - 1] = 0.0;
                                fLocal[5 - 1] = BendingFixAndBipendolumUniformLoad(_length - 0.0, q3, _length);

                                fLocal[9 - 1] = ShearFixAndBipendolumUniformLoad(0.0, q3, _length);
                                fLocal[11 - 1] = -BendingFixAndBipendolumUniformLoad(0.0, q3, _length);
                                break;
                            case LocalDOF.R2:
                                fLocal[3 - 1] = 3.0 / 8.0 * q3 * _length;
                                fLocal[5 - 1] = 0.0;

                                fLocal[9 - 1] = 5.0 / 8.0 * q3 * _length;
                                fLocal[11 - 1] = q3 * _length * _length / 8.0;
                                break;
                            case LocalDOF.R3:
                                fLocal[2 - 1] = 3.0 / 8.0 * q2 * _length;
                                fLocal[6 - 1] = 0.0;

                                fLocal[8 - 1] = 5.0 / 8.0 * q2 * _length;
                                fLocal[12 - 1] = -q2 * _length * _length / 8.0;
                                break;
                        }
                    }
                }

                if (EndBeam == EndSide.End2)
                {
                    for (int i = 0; i < localDOFs.Length; i++)
                    {
                        switch (localDOFs[i])
                        {
                            case LocalDOF.AxialU1:
                                fLocal[0] = q1 * _length;
                                fLocal[6] = 0.0;
                                break;
                            case LocalDOF.TorsionR1:
                                //TODO
                                break;
                            case LocalDOF.U2:
                                fLocal[2 - 1] = ShearFixAndBipendolumUniformLoad(0.0, q2, _length);
                                fLocal[6 - 1] = -BendingFixAndBipendolumUniformLoad(0.0, q2, _length);

                                fLocal[8 - 1] = 0.0; 
                                fLocal[12 - 1] = BendingFixAndBipendolumUniformLoad(_length, q2, _length);
                                break;
                            case LocalDOF.U3:
                                fLocal[3 - 1] = ShearFixAndBipendolumUniformLoad(0.0, q3, _length);
                                fLocal[5 - 1] = BendingFixAndBipendolumUniformLoad(0.0, q3, _length);

                                fLocal[9 - 1] = 0.0; 
                                fLocal[11 - 1] = -BendingFixAndBipendolumUniformLoad(_length, q3, _length);
                                break;
                            case LocalDOF.R2:
                                fLocal[3 - 1] = 5.0 / 8.0 * q3 * _length;
                                fLocal[5 - 1] = -q3 * _length * _length / 8.0;

                                fLocal[9 - 1] = 3.0 / 8.0 * q3 * _length;
                                fLocal[11 - 1] = 0.0;
                                break;
                            case LocalDOF.R3:
                                fLocal[2 - 1] = 5.0 / 8.0 * q2 * _length;
                                fLocal[6 - 1] = q2 * _length * _length / 8.0;

                                fLocal[8 - 1] = 3.0 / 8.0 * q2 * _length;
                                fLocal[12 - 1] = 0.0;
                                break;
                        }
                    }
                }
            }

            #region ApplyReleaseToTruss
            #region duobleReleaseR2
            if (IsDoubleReleasedR2() == true)
            {
                fLocal[3 - 1] = q3 * _length / 2.0;
                fLocal[5 - 1] = 0.0;

                fLocal[9 - 1] = q3 * _length / 2.0;
                fLocal[11 - 1] = 0.0;
            }
            #endregion

            #region duobleReleaseR3
            if (IsDoubleReleasedR3() == true)
            {
                fLocal[2 - 1] = q2 * _length / 2.0;
                fLocal[6 - 1] = 0.0;

                fLocal[8 - 1] = q2 * _length / 2.0;
                fLocal[12 - 1] = 0.0;
            }
            #endregion
            #endregion


            #region End1
            #region releaseU2R3End1
            if (IsReleasedU2AndR3(EndSide.End1)) {
                fLocal[2 - 1] = 0.0;
                fLocal[6 - 1] = 0.0;

                fLocal[8 - 1] = q2 * _length;
                fLocal[12 - 1] = -q2 * _length * _length / 2.0;
            }
            #endregion

            #region releaseU3R2End1
            if (IsReleasedU3AndR2(EndSide.End1))
            {
                fLocal[3 - 1] = 0.0;
                fLocal[5 - 1] = 0.0;

                fLocal[9 - 1] = q3 * _length;
                fLocal[11 - 1] = q3 * _length * _length / 2.0;
            }
            #endregion
            #endregion

            #region End2
            #region releaseU2R3End2
            if (IsReleasedU2AndR3(EndSide.End2))
            {
                fLocal[2 - 1] = q2 * _length;
                fLocal[6 - 1] = q2 * _length * _length / 2.0;

                fLocal[8 - 1] = 0.0;
                fLocal[12 - 1] = 0.0;
            }
            #endregion

            #region releaseU3R2End2
            if (IsReleasedU3AndR2(EndSide.End2))
            {
                fLocal[3 - 1] = q3 * _length;
                fLocal[5 - 1] = -q3 * _length * _length / 2.0;

                fLocal[9 - 1] = 0.0;
                fLocal[11 - 1] = 0.0;
            }
            #endregion
            #endregion

            #endregion

            return fLocal;
        }

        #region Duplicate
        public override FiniteElement Duplicate(ElementProperty property, List<LoadCaseAttribute> lcAttributes, List<FreedomCaseAttribute> fcAttributes)
        {
            //duplicate nodes
            Node[] duplicatedNodes = _nodesGlobal.Select(node => node.Duplicate()).ToArray();

            //duplicate beam
            EulerBeam duplicatedBeam = new EulerBeam(duplicatedNodes, (Section) property, _axisAngleRadians);
            duplicatedBeam.SetId(this.Id);

            foreach (FreedomCaseAttribute attribute in fcAttributes)
            {
                duplicatedBeam.AttributesFreedomCase.Add(attribute);
            }
            foreach (LoadCaseAttribute attribute in lcAttributes)
            {
                duplicatedBeam.AttributesLoadCase.Add(attribute);
            }
            return duplicatedBeam;
        }

        public override FiniteElement Duplicate()
        {
            return Duplicate(_property, _attributesLoadCase, _attributesFreedomCase);
        }
        #endregion

        #region GetForces
        public Dictionary<Beam.InternalAction, double> GetInternalNodalLocalForces(Beam.EndSide endSide, double[] globalDisplacements)
        {
            Dictionary<Beam.InternalAction, double> result = new Dictionary<InternalAction, double>();
                
                mnl.Vector<double> localDisplacementsNodes = GetLocalDisplacementVector(globalDisplacements);
                mnl.Vector<double> localForcesNodes = GetInternalNodalLocalForces(localDisplacementsNodes);

                if (endSide == Beam.EndSide.End1)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if (i == 4 || i == 1 || i == 2)
                        {
                            result.Add((InternalAction) i , localForcesNodes[i]);
                        }
                        else
                        {
                            result.Add((InternalAction)i, -localForcesNodes[i]);
                        }
                    }
                } else //End2
                {
                    for (int i = 6; i < 12; i++)
                    {
                        int j = i - 6;
                        if (j == 4 || j == 1 || j == 2)
                        {
                            result.Add((InternalAction)j, -localForcesNodes[i]);
                        }
                        else
                        {
                            result.Add((InternalAction)j, localForcesNodes[i]);
                        }
                    }
                }
                return result;
        }

        internal Dictionary<Beam.InternalAction, double> GetInternalAction(double station, double[] globalDisplacement)
        {
            Dictionary<Beam.InternalAction, double> forces = new Dictionary<InternalAction, double>();

            //Post-processing only for beam:
            double qx = 0;
            double qy = 0;
            double qz = 0;
            for (int i = 0; i < _attributesLoadCase.Count; i++)
            {
                if (_attributesLoadCase[i].GetType() == typeof(BeamDistribuitedLoadAttribute))
                {
                    BeamDistribuitedLoadAttribute q = (BeamDistribuitedLoadAttribute)_attributesLoadCase[i];
                    qx += q.Q1;
                    qy += q.Q2;
                    qz += q.Q3;
                }
            }

            var internalForcesNode1 = GetInternalNodalLocalForces(Beam.EndSide.End1, globalDisplacement);
            var internalForcesNode2 = GetInternalNodalLocalForces(Beam.EndSide.End2, globalDisplacement);

            var releases = GetEndReleases();

            /*if (releases.Count() == 0)
            {*/
                double Nx = internalForcesNode1[InternalAction.N] * N0(station, _length) + internalForcesNode2[InternalAction.N] * N1(station, _length);
                double V2x = internalForcesNode1[InternalAction.V2] * N0(station, _length) + internalForcesNode2[InternalAction.V2] * N1(station, _length);
                double V3x = internalForcesNode1[InternalAction.V3] * N0(station, _length) + internalForcesNode2[InternalAction.V3] * N1(station, _length);
                double Tx = internalForcesNode1[InternalAction.T] * N0(station, _length) + internalForcesNode2[InternalAction.T] * N1(station, _length);
                double M2x = internalForcesNode1[InternalAction.M2] * N0(station, _length) + internalForcesNode2[InternalAction.M2] * N1(station, _length);
                double M3x = internalForcesNode1[InternalAction.M3] * N0(station, _length) + internalForcesNode2[InternalAction.M3] * N1(station, _length);

                forces.Add(Beam.InternalAction.N, Nx - AxialBeamFixFixUniformLoad(qx, station, _length));
                forces.Add(Beam.InternalAction.V2, V2x - ShearBeamFixFixUniformLoad(qy, station, _length));
                forces.Add(Beam.InternalAction.V3, V3x - ShearBeamFixFixUniformLoad(qz, station, _length));

                forces.Add(Beam.InternalAction.T, Tx);
                forces.Add(Beam.InternalAction.M2, M2x - BendingBeamFixFixUniformLoad(qz, station, _length));
                forces.Add(Beam.InternalAction.M3, M3x - BendingBeamFixFixUniformLoad(qy, station, _length));
            /*} else
            {
                //TODO releases
            }*/

            return forces;            
        }
        #endregion

        #region GetDisplacement
        public Dictionary<LocalDOF, double> GetLocalDisplacementsAtEnd(Beam.EndSide endSide, double[] globalDisplacementsNode)
        {
            return GetLocalDisplacementsAtNode(endSide, globalDisplacementsNode);
        }

        public Dictionary<LocalDOF, double> GetLocalDisplacementsAtNode(Beam.EndSide endSide, double[] globalDisplacementsNode)
        {
            mnl.Vector<double> localDisplacementsNodes = GetLocalDisplacementVector(globalDisplacementsNode);

            Dictionary<LocalDOF, double> result = new Dictionary<LocalDOF, double>();
            if (endSide == Beam.EndSide.End1)
            {
                result.Add(LocalDOF.AxialU1, localDisplacementsNodes[0]);
                result.Add(LocalDOF.U2, localDisplacementsNodes[1]);
                result.Add(LocalDOF.U3, localDisplacementsNodes[2]);
                result.Add(LocalDOF.TorsionR1, localDisplacementsNodes[3]);
                result.Add(LocalDOF.R2, localDisplacementsNodes[4]);
                result.Add(LocalDOF.R3, localDisplacementsNodes[5]);
            } else
            {
                result.Add(LocalDOF.AxialU1, localDisplacementsNodes[6]);
                result.Add(LocalDOF.U2, localDisplacementsNodes[7]);
                result.Add(LocalDOF.U3, localDisplacementsNodes[8]);
                result.Add(LocalDOF.TorsionR1, localDisplacementsNodes[9]);
                result.Add(LocalDOF.R2, localDisplacementsNodes[10]);
                result.Add(LocalDOF.R3, localDisplacementsNodes[11]);
            }
                    
            return result;
        }

        public Dictionary<LocalDOF, double> GetLocalDisplacements(double station, double[] globalDisplacementsNodes)
        {
            Dictionary<LocalDOF, double> displLocalNode1 = GetLocalDisplacementsAtNode(Beam.EndSide.End1, globalDisplacementsNodes);
            Dictionary<LocalDOF, double> displLocalNode2 = GetLocalDisplacementsAtNode(Beam.EndSide.End2, globalDisplacementsNodes);

            double E = ((Section)_property).Material.E;
            double J11 = ((Section)_property).Jxx;
            double J22 = ((Section)_property).Jyy;
            double A = ((Section)_property).Area;

            #region uniformLoad
            double q1 = 0;
            double q2 = 0;
            double q3 = 0;
            for (int i = 0; i < _attributesLoadCase.Count; i++)
            {
                if (_attributesLoadCase[i].GetType() == typeof(BeamDistribuitedLoadAttribute))
                {
                    BeamDistribuitedLoadAttribute q = (BeamDistribuitedLoadAttribute)_attributesLoadCase[i];
                    q1 += q.Q1;
                    q2 += q.Q2;
                    q3 += q.Q3;
                }
            }
            #endregion

            Dictionary<LocalDOF, double> displStation = new Dictionary<LocalDOF, double>();
            #region BeamWithoutReleases
            for (int i = 0; i < displLocalNode1.Count; i++) //loop on DOF
                {
                    LocalDOF index = (LocalDOF)i;
                    displStation.Add(index, displLocalNode1[index] * N0(station, _length) + displLocalNode2[index] * N1(station, _length)); //linear interpolation
                    
                    if (index == LocalDOF.AxialU1)
                    {
                        double dq = (-1.0 / 4.0 * q1 * station * station + 1.0/4.0 * (2.0 * L * station - station * station) * q1) / (E * A); //from: integration of qL/2 * N0(x) + (-qL/2) * N1(x)
                        displStation[index] = displStation[index] + dq;
                    }

                    if (index == LocalDOF.U2)
                    {
                        displStation[index] = displStation[index] + DisplacementFixFixUniformLoad(q2, station, _length, E, J22) + DisplacementFixFixImposedRotation(station, displLocalNode1[LocalDOF.R3], _length) - DisplacementFixFixImposedRotation(_length - station, displLocalNode2[LocalDOF.R3], _length);
                    }

                    if (index == LocalDOF.U3)
                    {
                        displStation[index] = displStation[index] + DisplacementFixFixUniformLoad(q3, station, _length, E, J11) - DisplacementFixFixImposedRotation(station, displLocalNode1[LocalDOF.R2], _length) + DisplacementFixFixImposedRotation(_length - station, displLocalNode2[LocalDOF.R2], _length);
                    }

                    //TODO: add calculation of rotations (x = station)
                }
            #endregion

            BeamReleasesAttribute[] releases = _attributesFreedomCase.OfType<BeamReleasesAttribute>().ToArray();
            #region AddEffectOfReleases
            for (int j = 0; j < releases.Length; j++) //cycle over releases attributes
            {
                BeamReleasesAttribute release = releases[j];

                var end = release.EndBeam;
                var dofReleased = release.LocalDOFReleased;
               
                if (end == EndSide.End1)
                {
                    #region releaseAxial
                    if (dofReleased.Contains(LocalDOF.AxialU1))
                    {
                        double x = _length - station;
                        double dq = q1 / (E * A) * (_length * x - x*x / 2.0);
                        displStation[LocalDOF.AxialU1] = displLocalNode2[LocalDOF.AxialU1] + dq;
                    }
                    #endregion

                    #region releaseU2
                    if (dofReleased.Contains(LocalDOF.U2))
                    {
                        double rotation1 = displLocalNode1[LocalDOF.R3];
                        double rotation2 = displLocalNode2[LocalDOF.R3];
                        double dr1 = DisplacementFixAndFreeWithImposedRotationAtFreeEnd(_length - station, rotation1, _length);
                        double dr2 = DisplacementFixAndBiPendulumImposedRotationAtFixEnd(_length - station, rotation2, _length);
                        double dq = DisplacementFixAndBipendolumUniformLoad(_length - station, q2, _length, E, J22);
                        displStation[LocalDOF.U2] = displLocalNode2[LocalDOF.U2] - dr1 + dr2 + dq;
                    }
                    #endregion

                    #region releaseU3
                    if (dofReleased.Contains(LocalDOF.U3))
                    {
                        double rotation1 = displLocalNode1[LocalDOF.R2];
                        double rotation2 = displLocalNode2[LocalDOF.R2];
                        double dr1 = DisplacementFixAndFreeWithImposedRotationAtFreeEnd(_length - station, rotation1, _length);
                        double dr2 = DisplacementFixAndBiPendulumImposedRotationAtFixEnd(_length - station, rotation2, _length);
                        double dq = DisplacementFixAndBipendolumUniformLoad(_length - station, q3, _length, E, J11);
                        displStation[LocalDOF.U3] = displLocalNode2[LocalDOF.U3] + dr1 - dr2 + dq;
                    }
                    #endregion

                    #region releaseTorsion
                    if (dofReleased.Contains(LocalDOF.TorsionR1))
                    {
                        displStation[LocalDOF.TorsionR1] = displLocalNode2[LocalDOF.TorsionR1];
                    }
                    #endregion

                    #region releaseR2
                    if (dofReleased.Contains(LocalDOF.R2))
                    {
                        double displacement = displLocalNode2[LocalDOF.U3] - displLocalNode1[LocalDOF.U3];
                        double rotation = displLocalNode2[LocalDOF.R2];
                        double dv = DisplacementFixAndSimplySupportedWithImposedDisplacementAtEndSimplySupported(_length - station, displacement, _length);
                        double dr = DisplacementFixAndSimplySupportedWithImposedRotationAtEnd(_length - station, rotation, _length);
                        double dq = DisplacementFixAndSimplySupportedUniformLoad(q3, _length - station, _length, E, J11);
                        displStation[LocalDOF.U3] = displLocalNode2[LocalDOF.U3] - dv + dr + dq;
                    }
                    #endregion

                    #region releaseR3
                    if (dofReleased.Contains(LocalDOF.R3))
                    {
                        double displacement = displLocalNode2[LocalDOF.U2] - displLocalNode1[LocalDOF.U2];
                        double rotation = displLocalNode2[LocalDOF.R3];
                        double dv = DisplacementFixAndSimplySupportedWithImposedDisplacementAtEndSimplySupported(_length - station, displacement, _length);
                        double dr = DisplacementFixAndSimplySupportedWithImposedRotationAtEnd(_length - station, rotation, _length);
                        double dq = DisplacementFixAndSimplySupportedUniformLoad(q2, _length - station, _length, E, J22);
                        displStation[LocalDOF.U2] = displLocalNode2[LocalDOF.U2] - dv - dr + dq;
                    }
                    #endregion

                    #region releaseR2AndU3
                    if (dofReleased.Contains(LocalDOF.U3) && dofReleased.Contains(LocalDOF.R2))
                    {
                        double dr = (_length - station) * displLocalNode2[LocalDOF.R2];
                        double dq = DisplacementFixFreeUniformLoad(_length - station, _length, q3, E, J11);
                        displStation[LocalDOF.U3] = displLocalNode2[LocalDOF.U3] + dr + dq;

                        displStation[LocalDOF.R2] = displLocalNode2[LocalDOF.R2];
                    }
                    #endregion

                    #region releaseR3AndU2
                    if (dofReleased.Contains(LocalDOF.U2) && dofReleased.Contains(LocalDOF.R3))
                    {
                        double dr = -(_length - station) * displLocalNode2[LocalDOF.R3];
                        double dq = DisplacementFixFreeUniformLoad(_length - station, _length, q2, E, J22);
                        displStation[LocalDOF.U2] = displLocalNode2[LocalDOF.U2] + dr + dq;

                        displStation[LocalDOF.R3] = displLocalNode2[LocalDOF.R3];
                    }
                    #endregion
                }

                if (end == EndSide.End2)
                {
                    #region releaseAxial
                    if (dofReleased.Contains(LocalDOF.AxialU1))
                    {
                        var index = LocalDOF.AxialU1;
                        double x = station;
                        double dq = q1 / (E * A) * (_length * x - x * x / 2.0);
                        displStation[LocalDOF.AxialU1] = displLocalNode1[index] + dq;
                    }
                    #endregion

                    #region releaseU2
                    if (dofReleased.Contains(LocalDOF.U2))
                    {
                        double rotation1 = displLocalNode1[LocalDOF.R3];
                        double rotation2 = displLocalNode2[LocalDOF.R3];
                        double dr2 = DisplacementFixAndFreeWithImposedRotationAtFreeEnd(station, rotation2, _length);
                        double dr1 = DisplacementFixAndBiPendulumImposedRotationAtFixEnd(station, rotation1, _length);
                        double dq = DisplacementFixAndBipendolumUniformLoad(station, q2, _length, E, J22);
                        displStation[LocalDOF.U2] = displLocalNode1[LocalDOF.U2] + dr2 - dr1 + dq;
                    }
                    #endregion

                    #region releaseU3
                    if (dofReleased.Contains(LocalDOF.U3))
                    {
                        double rotation1 = displLocalNode1[LocalDOF.R2];
                        double rotation2 = displLocalNode2[LocalDOF.R2];
                        double dr2 = DisplacementFixAndFreeWithImposedRotationAtFreeEnd(station, rotation2, _length);
                        double dr1 = DisplacementFixAndBiPendulumImposedRotationAtFixEnd(station, rotation1, _length);
                        double dq = DisplacementFixAndBipendolumUniformLoad(station, q3, _length, E, J11);
                        displStation[LocalDOF.U3] = displLocalNode1[LocalDOF.U3] - dr2 + dr1 + dq;
                    }
                    #endregion

                    #region releaseTorsion
                    if (dofReleased.Contains(LocalDOF.TorsionR1)) {
                        displStation[LocalDOF.TorsionR1] = displLocalNode1[LocalDOF.TorsionR1];
                    }
                    #endregion

                    #region releaseR2
                    if (dofReleased.Contains(LocalDOF.R2))
                    {
                        double displacement = displLocalNode2[LocalDOF.U3] - displLocalNode1[LocalDOF.U3];
                        double rotation = displLocalNode1[LocalDOF.R2];
                        double dv = DisplacementFixAndSimplySupportedWithImposedDisplacementAtEndSimplySupported(station, displacement, _length);
                        double dr = DisplacementFixAndSimplySupportedWithImposedRotationAtEnd(station, rotation, _length);
                        double dq = DisplacementFixAndSimplySupportedUniformLoad(q3, station, _length, E, J11);
                        displStation[LocalDOF.U3] = displLocalNode1[LocalDOF.U3] + dv - dr + dq;
                    }
                    #endregion

                    #region releaseR3
                    if (dofReleased.Contains(LocalDOF.R3))
                    {
                        double displacement = displLocalNode2[LocalDOF.U2] - displLocalNode1[LocalDOF.U2];
                        double rotation = displLocalNode1[LocalDOF.R3];
                        double dv = DisplacementFixAndSimplySupportedWithImposedDisplacementAtEndSimplySupported(station, displacement, _length);
                        double dr = DisplacementFixAndSimplySupportedWithImposedRotationAtEnd(station, rotation, _length);
                        double dq = DisplacementFixAndSimplySupportedUniformLoad(q2, station, _length, E, J22);
                        displStation[LocalDOF.U2] = displLocalNode1[LocalDOF.U2] + dv + dr + dq;
                    }
                    #endregion

                    #region releaseR2AndU3
                    if (dofReleased.Contains(LocalDOF.U3) && dofReleased.Contains(LocalDOF.R2))
                    {
                        double dr = -station * displLocalNode1[LocalDOF.R2];
                        double dq = DisplacementFixFreeUniformLoad(station, _length, q3, E, J11);
                        displStation[LocalDOF.U3] = displLocalNode1[LocalDOF.U3] + dr + dq;

                        displStation[LocalDOF.R2] = displLocalNode1[LocalDOF.R2];
                    }
                    #endregion

                    #region releaseR3AndU2
                    if (dofReleased.Contains(LocalDOF.U2) && dofReleased.Contains(LocalDOF.R3))
                    {
                        double dr = station * displLocalNode1[LocalDOF.R3];
                        double dq = DisplacementFixFreeUniformLoad(station, _length, q2, E, J22);
                        displStation[LocalDOF.U2] = displLocalNode1[LocalDOF.U2] + dr + dq;

                        displStation[LocalDOF.R3] = displLocalNode1[LocalDOF.R3];
                    }
                    #endregion
                }
            }

            #region truss
            if (IsDoubleReleasedR3() == true)
            {
                //truss
                var dq = DisplacementSimplySupportedUniformLoad(q2, station, _length, E, J22);
                displStation[LocalDOF.U2] = displLocalNode1[LocalDOF.U2] * N0(station, _length) + displLocalNode2[LocalDOF.U2] * N1(station, _length) + dq;
            }

            if (IsDoubleReleasedR2() == true)
            {
                //truss
                var dq = DisplacementSimplySupportedUniformLoad(q3, station, _length, E, J11);
                displStation[LocalDOF.U3] = displLocalNode1[LocalDOF.U3] * N0(station, _length) + displLocalNode2[LocalDOF.U3] * N1(station, _length) + dq;
            }
            #endregion
            #endregion
            return displStation;
        }
        #endregion

        #region EndRelease
        public void AddEndRelease(int indexEndBeam, Beam.LocalDOF[] dof, string freedomCaseName, string name)
        {
            BeamReleasesAttribute release = new BeamReleasesAttribute(indexEndBeam, dof.ToHashSet(), freedomCaseName, name);
            _attributesFreedomCase.Add(release);
        }

        public void AddEndRelease(EndSide endBeam, Beam.LocalDOF[] dof, string freedomCaseName, string name)
        {
            BeamReleasesAttribute release = new BeamReleasesAttribute(endBeam, dof.ToHashSet(), freedomCaseName, name);
            _attributesFreedomCase.Add(release);
        }

        public HashSet<Tuple<EndSide, LocalDOF>> GetEndReleases()
        {
            HashSet<Tuple<EndSide, LocalDOF>> releases = new HashSet<Tuple<EndSide, LocalDOF>>();
            foreach (BeamReleasesAttribute rel in _attributesFreedomCase)
            {
                for (int i = 0; i < rel.LocalDOFReleased.Count(); i++)
                {
                    releases.Add(new Tuple<EndSide, LocalDOF>(rel.EndBeam, rel.LocalDOFReleased[i]));
                }
            }
            return releases;
        }

        public bool IsDoubleReleasedR3()
        {
            var searchRelese1 = new Tuple<EndSide, LocalDOF>(EndSide.End1, LocalDOF.R3);
            var searchRelese2 = new Tuple<EndSide, LocalDOF>(EndSide.End2, LocalDOF.R3);

            var releases = GetEndReleases();

            if (releases.Contains(searchRelese1) && releases.Contains(searchRelese2))
            {
                return true;
            } else
            {
                return false;
            }
        }

        public bool IsDoubleReleasedR2()
        {
            var searchRelese1 = new Tuple<EndSide, LocalDOF>(EndSide.End1, LocalDOF.R2);
            var searchRelese2 = new Tuple<EndSide, LocalDOF>(EndSide.End2, LocalDOF.R2);

            var releases = GetEndReleases();

            if (releases.Contains(searchRelese1) && releases.Contains(searchRelese2))
            {
                return true;
            } else
            {
                return false;
            }
        }

        public bool IsReleasedU2AndR3(EndSide end)
        {
            Tuple<EndSide, LocalDOF> searchRelese1;
            Tuple<EndSide, LocalDOF> searchRelese2;
            if (end == EndSide.End1)
            {
                searchRelese1 = new Tuple<EndSide, LocalDOF>(EndSide.End1, LocalDOF.U2);
                searchRelese2 = new Tuple<EndSide, LocalDOF>(EndSide.End1, LocalDOF.R3);
            } else
            {
                searchRelese1 = new Tuple<EndSide, LocalDOF>(EndSide.End2, LocalDOF.U2);
                searchRelese2 = new Tuple<EndSide, LocalDOF>(EndSide.End2, LocalDOF.R3);
            }
            
            var releases = GetEndReleases();

            if ((releases.Contains(searchRelese1) == true && releases.Contains(searchRelese2)) == true)
            {
                return true;
            } else
            {
                return false;
            }
        }

        public bool IsReleasedU3AndR2(EndSide end)
        {
            Tuple<EndSide, LocalDOF> searchRelese1;
            Tuple<EndSide, LocalDOF> searchRelese2;

            if (end == EndSide.End1)
            {
                searchRelese1 = new Tuple<EndSide, LocalDOF>(EndSide.End1, LocalDOF.U3);
                searchRelese2 = new Tuple<EndSide, LocalDOF>(EndSide.End1, LocalDOF.R2);
            }
            else
            {
                searchRelese1 = new Tuple<EndSide, LocalDOF>(EndSide.End2, LocalDOF.U3);
                searchRelese2 = new Tuple<EndSide, LocalDOF>(EndSide.End2, LocalDOF.R2);
            }

            var releases = GetEndReleases();
            if ((releases.Contains(searchRelese1) == true && releases.Contains(searchRelese2)))
            {
                return true;
            } else
            {
                return false;
            }
        }
        #endregion

        private static mnl.Matrix<double> GetStiffnessBeam(double L, double A, double Jyy, double Jzz, double Jt, double E, double G)
        {
            mnl.Matrix<double> kLocal = mnl.Matrix<double>.Build.Dense(12, 12);
            double L2 = L * L;
            double L3 = L * L * L;

            kLocal[1 - 1, 1 - 1] = E * A / L;

            kLocal[2 - 1, 2 - 1] = 12.0 * E * Jzz / L3;

            kLocal[3 - 1, 3 - 1] = 12.0 * E * Jyy / L3;

            kLocal[4 - 1, 4 - 1] = G * Jt / L;

            kLocal[5 - 1, 3 - 1] = -6.0 * E * Jyy / L2;
            kLocal[5 - 1, 5 - 1] = 4.0 * E * Jyy / L;

            kLocal[6 - 1, 2 - 1] = 6.0 * E * Jzz / L2;
            kLocal[6 - 1, 6 - 1] = 4.0 * E * Jzz / L;

            kLocal[7 - 1, 1 - 1] = -E * A / L;
            kLocal[7 - 1, 7 - 1] = E * A / L;

            kLocal[8 - 1, 2 - 1] = -12.0 * E * Jzz / L3;
            kLocal[8 - 1, 6 - 1] = -6.0 * E * Jzz / L2;
            kLocal[8 - 1, 8 - 1] = 12.0 * E * Jzz / L3;

            kLocal[9 - 1, 3 - 1] = -12.0 * E * Jyy / L3;
            kLocal[9 - 1, 5 - 1] = 6.0 * E * Jyy / L2;
            kLocal[9 - 1, 9 - 1] = 12.0 * E * Jyy / L3;

            kLocal[10 - 1, 4 - 1] = -G * Jt / L;
            kLocal[10 - 1, 10 - 1] = G * Jt / L;

            kLocal[11 - 1, 3 - 1] = -6.0 * E * Jyy / L2;
            kLocal[11 - 1, 5 - 1] = 2.0 * E * Jyy / L;
            kLocal[11 - 1, 9 - 1] = 6.0 * E * Jyy / L2;
            kLocal[11 - 1, 11 - 1] = 4.0 * E * Jyy / L;

            kLocal[12 - 1, 2 - 1] = 6.0 * E * Jzz / L2;
            kLocal[12 - 1, 6 - 1] = 2.0 * E * Jzz / L;
            kLocal[12 - 1, 8 - 1] = -6.0 * E * Jzz / L2;
            kLocal[12 - 1, 12 - 1] = 4.0 * E * Jzz / L;

            #region applySimmetry
            for (int row = 0; row < kLocal.RowCount; row++)
            {
                for (int col = 0; col < kLocal.ColumnCount; col++)
                {
                    kLocal[row, col] = kLocal[col, row];
                }
            }
            #endregion

            return kLocal;
        }

        #region PostProcessorFunctions
        /// <summary>
        /// used for interpolation = calculation of a general value in a station
        /// </summary>
        /// <param name="x">distance from starting node</param>
        /// <param name="L">length of the beam</param>
        /// <returns></returns>
        private static double N0(double x, double L)
        {
            return (L - x) / L;
        }

        /// <summary>
        /// used for interpolation = calculation of a general value in a station
        /// </summary>
        /// <param name="x">distance from starting node</param>
        /// <param name="L">length of the beam</param>
        /// <returns></returns>
        private static double N1(double x, double L)
        {
            return x / L;
        }

        /// <summary>
        /// Return the Bending moment in a fix-fix beam with uniform load
        /// </summary>
        /// <param name="q">load [F/L]</param>
        /// <param name="x">coordinate 0 to L</param>
        /// <param name="L">Lenght of the beam</param>
        /// <returns>Bending moment</returns>
        private static double BendingBeamFixFixUniformLoad(double q,double x, double L)
        {
            return -q / 12.0 * (L * L - 6.0 * L * x + 6.0 * x * x);
        }

        /// <summary>
        /// Return the shear in a fix-fix beam with uniform load
        /// </summary>
        /// <param name="q">load [F/L]</param>
        /// <param name="x">coordinate 0 to L</param>
        /// <param name="L">Lenght of the beam</param>
        /// <returns>Shear</returns>
        private static double AxialBeamFixFixUniformLoad(double q, double x, double L)
        {
            //y = m x + q
            double qEq = q * L / 2.0;
            double mEq = -2.0 * (qEq) / L;

            return -(mEq * x + qEq);
        }

        /// <summary>
        /// Return the shear in a fix-fix beam with uniform load
        /// </summary>
        /// <param name="q">load [F/L]</param>
        /// <param name="x">coordinate 0 to L</param>
        /// <param name="L">Lenght of the beam</param>
        /// <returns>Shear</returns>
        private static double ShearBeamFixFixUniformLoad(double q, double x, double L)
        {
            return q / 2.0 * (L - 2.0 * x);
        }
        /// <summary>
        /// Return the rotation in a fix-fix beam with uniform load
        /// </summary>
        /// <param name="q">load [F/L]</param>
        /// <param name="x">coordinate 0 to L</param>
        /// <param name="L">Lenght of the beam</param>
        /// <param name="E">Elastic Modulus</param>
        /// <param name="J">Second moment area - Inertia</param>
        /// <returns>rotation</returns>
        private static double RotationBeamFixFixUniformLoad(double q, double x, double L, double E, double J)
        {
            return -q * x /(12.0 * E * J) * (L*L - 3.0 * L * x + 2.0 * x*x);
        }

        /// <summary>
        /// Return the displacement in a fix-fix beam with uniform load
        /// </summary>
        /// <param name="q">load [F/L]</param>
        /// <param name="x">coordinate 0 to L</param>
        /// <param name="L">Lenght of the beam</param>
        /// <param name="E">Elastic Modulus</param>
        /// <param name="J">Second moment area - Inertia</param>
        /// <returns>displacement</returns>
        private static double DisplacementFixFixUniformLoad(double q, double x, double L, double E, double J)
        {
            return q * x*x * Math.Pow(L - x,2.0) / (24.0 * E * J);
        }

        /// <summary>
        /// get displacment of a fix-fix beam with an imposed rotation in the first node
        /// </summary>
        /// <param name="x">stattion</param>
        /// <param name="alpha">imposed rotation</param>
        /// <param name="L">length of the beam</param>
        /// <returns></returns>
        private static double DisplacementFixFixImposedRotation(double x, double alpha, double L)
        {
            return alpha / (L*L) * (x*x*x + x * L * (L-2.0 *x));
        }

        /// <summary>
        /// Restrains: fix one side, free other side, imposed rotation in the free side
        /// </summary>
        /// <param name="x"></param>
        /// <param name="alpha"></param>
        /// <param name="L"></param>
        /// <returns></returns>
        private static double DisplacementFixAndFreeWithImposedRotationAtFreeEnd(double x, double alpha, double L)
        {
            //return alpha * L / 2.0 * Math.Pow(x / L,2.0);
            return alpha * x * x / (2.0 * L);
        }

        /// <summary>
        /// Restrains: one side Fix - other simply supported beam with imposed displacement in the simply supported end
        /// </summary>
        /// <param name="x"></param>
        /// <param name="displacement"></param>
        /// <param name="L"></param>
        /// <returns></returns>
        private static double DisplacementFixAndSimplySupportedWithImposedDisplacementAtEndSimplySupported(double x, double displacement, double L)
        {
            return displacement / 2.0 * x*x / Math.Pow(L,3.0) * (3.0 * L - x);
        }

        private static double DisplacementFixAndSimplySupportedWithImposedRotationAtEnd(double x, double angle, double L)
        {
            return angle * x / (L * L) * (L * L - x / 2.0 * (3.0 * L - x));
        }

        private static double DisplacementSimplySupportedUniformLoad(double q, double x, double L, double E, double J)
        {
            return q * Math.Pow(L, 4.0) / (24.0 * E * J) * x / L * (1.0 - 2.0 * x * x / (L * L) + Math.Pow(x / L, 3.0));
        }

        private static double DisplacementFixAndSimplySupportedUniformLoad(double q, double x, double L, double E, double J)
        {
            return q / (48.0 * E * J) * x * x * (3.0 * L * L - 5.0 * L * x + 2.0 * x * x);
        }

        private static double DisplacementFixAndBiPendulumImposedRotationAtFixEnd(double x, double alpha, double L)
        {
            return alpha / L * x * (x - 2.0 * L) / 2.0;
        }

        private static double DisplacementFixAndBipendolumUniformLoad(double x, double q, double L, double E, double J)
        {
            return q / (24.0 * E * J) * x*x * Math.Pow(x - 2.0 * L, 2.0);
        }

        private static double BendingFixAndBipendolumUniformLoad(double x, double q, double L)
        {
            return -q / 6.0 * (2.0 * L*L - 6.0 * L * x + 3.0 *x*x);
        }

        private static double ShearFixAndBipendolumUniformLoad(double x, double q, double L)
        {
            return q * (L-x);
        }

        private static double DisplacementFixFreeUniformLoad(double x, double L, double q, double E, double J)
        {
            return q * x * x / (24.0 * E * J) * (6.0 * L * L - 4.0 * L * x + x * x);
        }
        #endregion
    }
}