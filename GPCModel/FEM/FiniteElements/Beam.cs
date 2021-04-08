using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Elements;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Properties;
using GPC.Model.Sections;
using MathNet.Numerics.LinearAlgebra;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    /// <summary>
    /// Based on Finite Element by Rao - Chapter 9
    /// </summary>
    public class Beam : FiniteElement
    {
        internal Beam(Node[] nodes, Section section) : base(nodes)
        {
            _DOF.Add(LinearSolver.DOF.DX);
            _DOF.Add(LinearSolver.DOF.DY);
            _DOF.Add(LinearSolver.DOF.DZ);
            _DOF.Add(LinearSolver.DOF.RX);
            _DOF.Add(LinearSolver.DOF.RY);
            _DOF.Add(LinearSolver.DOF.RZ);

            SetProperty(section);
        }

        public override void BuildMatrix()
        {
            #region localStiffnessMatrix
            Section section = (Section)_property;
            double E = section.Material.E;
            double G = E / (2.0 * (1.0 + section.Material.Ni));
            double A = section.Area;
            double Jzz = section.J22;
            double Jyy = section.J11;
            double Jt = section.Jt;
            double L = _nodesGlobal[0].Position.DistanceTo(_nodesGlobal[1].Position);
            double L2 = L * L;
            double L3 = L2 * L;
            double angle = section.AngleX1; //necessaria un'altra variabile?

            _kElementLocalCoord = mnl.Matrix<double>.Build.Dense(12, 12);

            _kElementLocalCoord[1 - 1, 1 - 1] = E * A / L;

            _kElementLocalCoord[2 - 1, 2 - 1] = 12.0 * E * Jzz / L3;

            _kElementLocalCoord[3 - 1, 3 - 1] = 12.0 * E * Jyy / L3;

            _kElementLocalCoord[4 - 1, 4 - 1] = G * Jt / L;

            _kElementLocalCoord[5 - 1, 3 - 1] = -6.0 * E * Jyy / L2;
            _kElementLocalCoord[5 - 1, 5 - 1] = 4.0 * E * Jyy / L;
            
            _kElementLocalCoord[6 - 1, 2 - 1] = 6.0 * E * Jzz / L2;
            _kElementLocalCoord[6 - 1, 6 - 1] = 4.0 * E * Jzz / L;

            _kElementLocalCoord[7 - 1, 1 - 1] = -E * A / L;
            _kElementLocalCoord[7 - 1, 7 - 1] = E * A / L;

            _kElementLocalCoord[8 - 1, 2 - 1] = -12.0 * E * Jzz / L3;
            _kElementLocalCoord[8 - 1, 6 - 1] = -6.0 * E * Jzz / L2;
            _kElementLocalCoord[8 - 1, 8 - 1] = 12.0 * E * Jzz / L3;

            _kElementLocalCoord[9 - 1, 3 - 1] = -12.0 * E * Jyy / L3;
            _kElementLocalCoord[9 - 1, 5 - 1] = 6.0 * E * Jyy / L2;
            _kElementLocalCoord[9 - 1, 9 - 1] = 12.0 * E * Jyy / L3;

            _kElementLocalCoord[10 - 1, 4 - 1] = -G * Jt / L;
            _kElementLocalCoord[10 - 1, 10 - 1] = G * Jt / L;

            _kElementLocalCoord[11 - 1, 3 - 1] = -6.0 * E * Jyy / L2;
            _kElementLocalCoord[11 - 1, 5 - 1] = 2.0 * E * Jyy / L;
            _kElementLocalCoord[11 - 1, 9 - 1] = 6.0 * E * Jyy / L2;
            _kElementLocalCoord[11 - 1, 11 - 1] = 4.0 * E * Jyy / L;

            _kElementLocalCoord[12 - 1, 2 - 1] = 6.0 * E * Jzz / L2;
            _kElementLocalCoord[12 - 1, 6 - 1] = 2.0 * E * Jzz / L;
            _kElementLocalCoord[12 - 1, 8 - 1] = -6.0 * E * Jzz / L2;
            _kElementLocalCoord[12 - 1, 12 - 1] = 4.0 * E * Jzz / L;

            /*Console.WriteLine("kLocal");
            FEMUtilities.WriteMatrix(_kElementLocalCoord, "F0");*/

            //applying symmetry
            for (int row = 0; row < _kElementLocalCoord.RowCount; row++)
            {
                for (int col = 0; col < _kElementLocalCoord.ColumnCount; col++)
                {
                    _kElementLocalCoord[row, col] = _kElementLocalCoord[col, row];
                }
            }
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

            mnl.Matrix<double> lambda1 = mnl.Matrix<double>.Build.Dense(3, 3);
            lambda1[0, 0] = lox;
            lambda1[0, 1] = mox;
            lambda1[0, 2] = nox;

            lambda1[1, 0] = -(lox * mox) / d;
            lambda1[1, 1] = (lox * lox + nox * nox) / d;
            lambda1[1, 2] = -(mox * nox)/d;

            lambda1[2, 0] = -nox / d;
            lambda1[2, 1] = 0.0;
            lambda1[2, 2] = lox / d;

            /*Console.WriteLine("lambda1");
            FEMUtilities.WriteMatrix(lambda1);*/

            mnl.Matrix<double> lambda2 = mnl.Matrix<double>.Build.Dense(3, 3);
            lambda2[0, 0] = 1.0;

            lambda2[1, 1] = Math.Cos(angle);
            lambda2[1, 2] = Math.Sin(angle);

            lambda2[2, 1] = -Math.Sin(angle);
            lambda2[2, 2] = Math.Cos(angle);

            /*Console.WriteLine("lambda2");
            FEMUtilities.WriteMatrix(lambda2);*/

            mnl.Matrix<double> lambda = mnl.Matrix<double>.Build.Dense(3, 3);
            if (lox == 0.0 && nox == 0.0)
            {
                /*if (mox != 1.0)
                {
                    throw new Exception("mox shuold be 1");
                }*/
                lambda[0, 1] = mox;

                lambda[1, 0] = -mox * Math.Cos(angle);
                lambda[1, 2] = mox * Math.Sin(angle);

                lambda[2, 1] = Math.Sin(angle);
                lambda[2, 2] = Math.Cos(angle);
            }
            else
            {
                lambda = lambda2 * lambda1;
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
            #endregion
        }

        public override FiniteElement Duplicate(ElementProperty property, List<LoadCaseAttribute> lcAttributes, List<FreedomCaseAttribute> fcAttributes)
        {
            throw new NotImplementedException();
        }

        public override Matrix<double> GetB(double csi = 0, double eta = 0, double zeta = 0)
        {
            throw new NotImplementedException();
        }

        public override void GetNodesResults(double[] globalDisplacementsNodes, out double[] localDisplacements, out Matrix<double>[] gloabalPseudoDeformation, out Matrix<double>[] localPseudoDeformation, out Matrix<double>[] globalForces, out Matrix<double>[] localForces, out Matrix<double>[] globalStress, out Matrix<double>[] localStress, out Matrix<double>[] globalEpsilon, out Matrix<double>[] localEpsilon)
        {
            throw new NotImplementedException();
        }

        public override void GetResultPositionNaturalCoordinates(double csi, double eta, double zeta, double[] globalDisplacementsNodes, out double x, out double y, out double z, out double[] localDisplacements, out Matrix<double> gloabalPseudoDeformation, out Matrix<double> localPseudoDeformation, out Matrix<double> globalForces, out Matrix<double> localForces, out Matrix<double> globalStress, out Matrix<double> localStress, out Matrix<double> globalEpsilon, out Matrix<double> localEpsilon)
        {
            throw new NotImplementedException();
        }

        protected override Vector<double> BuildFLocalCoord()
        {
            return mnl.Vector<double>.Build.Dense(12);
        }
    }
}
