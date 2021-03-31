using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM
{
    public static class Util
    {
        #region Misc
        /// <summary>
        /// format = F0, F1, F2 ...
        /// </summary>
        /// <param name="m"></param>
        /// <param name="format"></param>
        public static void WriteMatrix(mnl.Matrix<double> m, string format = "F2")
        {
            #if DEBUG
            Console.WriteLine("dim: " + m.RowCount + " x " + m.ColumnCount);
            for (int r = 0; r < m.RowCount; r++)
            {
                for (int c = 0; c < m.ColumnCount; c++)
                {
                    Console.Write(m[r,c].ToString(format) + " \t");
                }
                Console.WriteLine();
            }
            #endif
        }

        public static void WriteMatrix(mnl.Vector<double> v, string format)
        {
            #if DEBUG
            Console.WriteLine("dim: "+ v.Count);
            for (int r = 0; r < v.Count; r++)
            {
                 Console.Write(v[r].ToString(format));
            }
            #endif
        }

        public static void WriteVector(mnl.Vector<double> v, string format)
        {
            #if DEBUG
            WriteMatrix(v, format);
            #endif
        }
        #endregion

        #region ReduceDimension
        /// <summary>
        /// Return F(x,y,z) = F(x,y,z) with "x" assigned
        /// </summary>
        public static Func<T2, T3, TOut> FFirstFix<T1, T2, T3, TOut>(T1 input1, Func<T1, T2, T3, TOut> fun) {
            return (T2 input2, T3 input3) => fun(input1, input2, input3);
        }

        /// <summary>
        /// Return F(x,y,z) = F(x,y,z) with "x" assigned
        /// </summary>
        public static Func<T2, T3, TOut> FirstFix<T1,T2,T3, TOut>(this Func<T1, T2, T3, TOut> fun, T1 input1)
        {
            return (T2 input2, T3 input3) => fun(input1, input2, input3);
        }

        /// <summary>
        /// Return F(x,y) = F(x,y,nodes) with "nodes" assigned
        /// </summary>
        public static Func<Func<double, double, Node[], mnl.Matrix<double>>, Node[], Func<double, double, mnl.Matrix<double>>> FFixedNodes = (Func<double, double, Node[], mnl.Matrix<double>> fun, Node[] nodes) => {
            return (double x, double y) => fun(x, y, nodes);
        };

        /// <summary>
        /// Return J(x,y) = J(x,y,dNdCsi, dNdEta,nodes) with "nodes" and derivative of shape function assigned
        /// arg1 = dFdInput1; arg1 = dFdInput2, arg3 = nodes
        /// </summary>
        public static Func<Func<int, double, double, double>, Func<int, double, double, double>, Node[], Func<double, double, mnl.Matrix<double>>> J2D = (Func<int, double, double, double> dFdInput1, Func<int, double, double, double> dFdInput2, Node[] nodes) => {
            return (double input1, double input2) => Jacob2D(input1, input2, dFdInput1, dFdInput2, nodes);
        };
        #endregion

        #region 2D
        /// <summary>
        /// Matrice jacobiana per cambiamento di variabile
        /// dN/dCsi = dx/dCsi * dN/dx + dy/dCsi * dN/dy
        /// dN/dEta = dx/dEta * dN/dx + dy/dEta * dN/dy
        /// => dN/dNatural = J * dN/dLocal;
        /// => dN/dLocal = J^-1 * dN/dNatural;
        /// => dF/dNatural = J^-1 dF/dLocal;
        /// </summary>
        /// <param name="csi">coordinata naturale</param>
        /// <param name="eta">coordinata naturale</param>
        /// <param name="dNdCsi">derivata funzioni di forma rispetto a Csi che descrive la GEOMETRIA (passaggio da coordinate locali a naturali) in funzione dell'indice di nodo e coordinate naturali</param>
        /// <param name="dNdEta">derivata funzioni di forma rispetto a Eta che descrive la GEOMETRIA (passaggio da coordinate locali a naturali) in funzione dell'indice di nodo e coordinate naturali</param>
        /// <param name="localNodes"></param>
        /// <returns>
        /// dx/dCsi, dy/dCsi
        /// dy/dEta, dy/dEta
        /// </returns>
        public static mnl.Matrix<double> Jacob2D(double csi, double eta, Func<int, double, double, double> dNdCsi, Func<int, double, double, double> dNdEta, Node[] localNodes)
        {
            double j11 = 0.0;
            double j12 = 0.0;
            double j21 = 0.0;
            double j22 = 0.0;
            for (int node = 0; node < localNodes.Length; node++)
            {
                int i = node + 1;
                double xi = localNodes[node].Position.X;
                double yi = localNodes[node].Position.Y;

                j11 = j11 + dNdCsi(i, csi, eta) * xi;
                j12 = j12 + dNdCsi(i, csi, eta) * yi;
                j21 = j21 + dNdEta(i, csi, eta) * xi;
                j22 = j22 + dNdEta(i, csi, eta) * yi;
            }

            mnl.Matrix<double> J = mnl.Matrix<double>.Build.Dense(2, 2);
            J[0, 0] = j11;

            J[0, 1] = j12;
            J[1, 0] = j21;

            J[1, 1] = j22;

            //Console.WriteLine("J(csi="+csi.ToString("F2")+",eta="+eta.ToString("F2")+"="+J);
            //Console.WriteLine("detJ(csi=" + csi.ToString("F2") + ",eta=" + eta.ToString("F2") + "=" + J.Determinant());
            return J;
        }

        /// <summary>
        /// Convert: dF/dCsi -> dF/dX and dF/dEta -> dF/dY
        /// </summary>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <param name="dFdCsi"></param>
        /// <param name="dFdEta"></param>
        /// <param name="Jacobian"></param>
        /// <returns></returns>
        public static mnl.Vector<double> GetdNdLocalFromdNdNatural2D(double csi, double eta, Func<double, double, double> dFdCsi, Func<double, double, double> dFdEta, Func<double, double, mnl.Matrix<double>> Jacobian)
        {
            mnl.Vector<double> dFdNatural = mnl.Vector<double>.Build.Dense(2);
            dFdNatural[0] = dFdCsi(csi, eta);
            dFdNatural[1] = dFdEta(csi, eta);

            mnl.Matrix<double> jacobian = Jacobian(csi, eta);

            return jacobian.Inverse() * dFdNatural;
        }
        #endregion

        #region 3D
        /// <summary>
        /// Matrice jacobiana per cambiamento di variabile
        /// dN/dCsi = dx/dCsi * dN/dx + dy/dCsi * dN/dy + dz/dCsi * dN/dz
        /// dN/dEta = dx/dEta * dN/dx + dy/dEta * dN/dy + dz/dEta * dN/dz
        /// dN/dZEta = dx/dEta * dN/dx + dy/dEta * dN/dy + dz/dZeta * dN/dz
        /// => dN/dNatural = J * dN/dLocal;
        /// => dN/dLocal = J^-1 * dN/dNatural;
        /// => dF/dNatural = J^-1 dF/dLocal;
        /// </summary>
        /// <param name="csi">coordinata naturale</param>
        /// <param name="eta">coordinata naturale</param>
        /// <param name="zeta">coordinata naturale</param>
        /// <param name="dNdCsi">derivata funzioni di forma rispetto a Csi che descrive la GEOMETRIA (passaggio da coordinate locali a naturali) in funzione dell'indice di nodo e coordinate naturali</param>
        /// <param name="dNdEta">derivata funzioni di forma rispetto a Eta che descrive la GEOMETRIA (passaggio da coordinate locali a naturali) in funzione dell'indice di nodo e coordinate naturali</param>
        /// <param name="dNdZeta">derivata funzioni di forma rispetto a Eta che descrive la GEOMETRIA (passaggio da coordinate locali a naturali) in funzione dell'indice di nodo e coordinate naturali</param>
        /// <param name="Nodes"></param>
        /// <returns>
        /// dx/dCsi, dy/dCsi, dz/dCsi
        /// dy/dEta, dy/dEta, dz/dEta
        /// dz/dEta, dz/dEta, dz/dZeta
        /// </returns>
        public static mnl.Matrix<double> Jacob3D(double csi, double eta, double zeta, Func<int, double, double, double, double> dNdCsi, Func<int, double, double, double, double> dNdEta, Func<int, double, double, double, double> dNdZeta, Node[] nodes)
        {
            double j11 = 0.0;
            double j12 = 0.0;
            double j13 = 0.0;

            double j21 = 0.0;
            double j22 = 0.0;
            double j23 = 0.0;

            double j31 = 0.0;
            double j32 = 0.0;
            double j33 = 0.0;
            for (int node = 0; node < nodes.Length; node++)
            {
                int i = node + 1;
                double xi = nodes[node].Position.X;
                double yi = nodes[node].Position.Y;
                double zi = nodes[node].Position.Z;

                j11 += dNdCsi(i, csi, eta, zeta) * xi;
                j12 += dNdCsi(i, csi, eta, zeta) * yi;
                j13 += dNdCsi(i, csi, eta, zeta) * zi;

                j21 += dNdEta(i, csi, eta, zeta) * xi;
                j22 += dNdEta(i, csi, eta, zeta) * yi;
                j23 += dNdEta(i, csi, eta, zeta) * zi;

                j31 += dNdZeta(i, csi, eta, zeta) * xi;
                j32 += dNdZeta(i, csi, eta, zeta) * yi;
                j33 += dNdZeta(i, csi, eta, zeta) * zi;
            }

            mnl.Matrix<double> J = mnl.Matrix<double>.Build.Dense(3, 3);
            J[0, 0] = j11;
            J[0, 1] = j12;
            J[0, 2] = j13;

            J[1, 0] = j21;
            J[1, 1] = j22;
            J[1, 2] = j23;

            J[2, 0] = j31;
            J[2, 1] = j32;
            J[2, 2] = j33;

            //Console.WriteLine("J(csi="+csi.ToString("F2")+",eta="+eta.ToString("F2")+"="+J);
            //Console.WriteLine("detJ(csi=" + csi.ToString("F2") + ",eta=" + eta.ToString("F2") + "=" + J.Determinant());
            return J;
        }

        /// <summary>
        /// Return J(x,y,z) = J(x,y,z, dNdCsi, dNdEta,dNdZeta, nodes) with "nodes" and derivative of shape function assigned
        /// arg1 = dFdInput1; arg2 = dFdInput2, arg3 = dFdInput3, arg4 = nodes
        /// </summary>
        public static Func<Func<int, double, double, double, double>, Func<int, double, double, double, double>, Func<int, double, double, double, double>, Node[], Func<double, double, double, mnl.Matrix<double>>> J3D = (Func<int, double, double, double, double> dFdInput1, Func<int, double, double, double, double> dFdInput2, Func<int, double, double, double, double> dFdInput3, Node[] nodes) => {
            return (double input1, double input2, double input3) => Jacob3D(input1, input2, input3, dFdInput1, dFdInput2, dFdInput3, nodes);
        };

        /// <summary>
        /// Convert: dF/dCsi -> dF/dX, dF/dEta -> dF/dY, dF/dZeta -> dF/dZ
        /// </summary>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <param name="zeta"></param>
        /// <param name="dFdCsi"></param>
        /// <param name="dFdEta"></param>
        /// <param name="Jacobian"></param>
        /// <returns></returns>
        public static mnl.Vector<double> GetdNdLocalFromdNdNatural3D(double csi, double eta, double zeta, Func<double, double, double, double> dFdCsi, Func<double, double, double, double> dFdEta, Func<double, double, double, double> dFdZeta, Func<double, double, double, mnl.Matrix<double>> Jacobian)
        {
            mnl.Vector<double> dFdNatural = mnl.Vector<double>.Build.Dense(3);
            dFdNatural[0] = dFdCsi(csi, eta, zeta);
            dFdNatural[1] = dFdEta(csi, eta, zeta);
            dFdNatural[2] = dFdZeta(csi, eta, zeta);

            mnl.Matrix<double> jacobian = Jacobian(csi, eta, zeta);

            return jacobian.Inverse() * dFdNatural;
        }
        #endregion

        #region GetXYZ
        public static double GetLocalCoordinate3D(string direction, double csi, double eta, double zeta, Func<int, double, double, double, double> ShapeFunctions, Node[] nodes)
        {
            double val = 0;
            
            for (int i = 1; i <= nodes.Length; i++)
            {
                double factor;
                switch (direction.ToUpper())
                {
                    case "X":
                        factor = nodes[i - 1].Position.X;
                        break;
                    case "Y":
                        factor = nodes[i - 1].Position.Y;
                        break;
                    case "Z":
                        factor = nodes[i - 1].Position.Z;
                        break;
                    default:
                        throw new IndexOutOfRangeException("direction can be X, Y or Z");
                }
                val += ShapeFunctions(i, csi, eta, zeta) * factor;
            }
            return val;
        }


        public static double GetLocalCoordinate2D(string direction, double csi, double eta, Func<int, double, double, double> ShapeFunctions, Node[] nodes)
        {
            double val = 0;

            for (int i = 1; i <= nodes.Length; i++)
            {
                double factor;
                switch (direction.ToUpper())
                {
                    case "X":
                        factor = nodes[i - 1].Position.X;
                        break;
                    case "Y":
                        factor = nodes[i - 1].Position.Y;
                        break;
                    default:
                        throw new IndexOutOfRangeException("direction can be X or Y");
                }
                val += ShapeFunctions(i, csi, eta) * factor;
            }
            return val;
        }
        #endregion
    }
}
