using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM
{
    class Util
    {
        /// <summary>
        /// format = F0, F1, F2 ...
        /// </summary>
        /// <param name="m"></param>
        /// <param name="format"></param>
        public static void WriteMatrix(mnl.Matrix<double> m, string format)
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

        public static mnl.Matrix<double> DPlaneStress(double E, double ni)
        {
            mnl.Matrix<double>  D = mnl.Matrix<double>.Build.Dense(3, 3);
            D[0, 0] = 1.0;
            D[0, 1] = ni;
            D[1, 0] = ni;
            D[1, 1] = 1.0;
            D[2, 2] = (1.0 - ni) / 2.0;
            D = E / (1.0 - ni * ni) * D;
            return D;
        }

        /// <summary>
        /// Return F(x,y) = F(i,x,y) with "i" assigned
        /// </summary>
        public static Func<int, Func<int, double, double, double>, Func<double, double, double>> F = (int index, Func<int, double, double, double> F) => {
            return (double input1, double input2) => F(index, input1, input2);
        };

        /// <summary>
        /// Return J(x,y) = J(x,y,nodes) with nodes assigned
        /// </summary>
        public static Func<Func<double, double, Node[], mnl.Matrix<double>>, Node[], Func<double, double, mnl.Matrix<double>>> J = (Func<double, double, Node[], mnl.Matrix<double>> J, Node[] local) => {
            return (double input1, double input2) => J(input1, input2, local);
        };

        /// <summary>
        /// Convert: dF/dCsi -> dF/dX and dF/dEta -> dF/dY
        /// </summary>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <param name="dFdCsi"></param>
        /// <param name="dFdEta"></param>
        /// <param name="Jacobian"></param>
        /// <returns></returns>
        public static mnl.Vector<double> GetdNdLocalFromdNdNatural(double csi, double eta, Func<double, double, double> dFdCsi, Func<double, double, double> dFdEta, Func<double, double, mnl.Matrix<double>> Jacobian)
        {
            mnl.Vector<double> dFdNatural = mnl.Vector<double>.Build.Dense(2);
            dFdNatural[0] = dFdCsi(csi, eta);
            dFdNatural[1] = dFdEta(csi, eta);

            mnl.Matrix<double> jacobian = Jacobian(csi, eta);

            return jacobian.Inverse() * dFdNatural;
        }
    }
}
