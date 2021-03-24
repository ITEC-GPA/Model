using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    public class Quad8Element
    {
        #region ShapeFunction
        public static double dNdCsi(int index, double csi, double eta)
        {
            switch (index)
            {
                case 1:
                    return 1.0 / 4.0 * (2.0 * csi + eta) * (1.0 - eta);
                case 2:
                    return 1.0 / 4.0 * (2.0 * csi - eta) * (1.0 - eta);
                case 3:
                    return 1.0 / 4.0 * (2.0 * csi + eta) * (1.0 + eta);
                case 4:
                    return 1.0 / 4.0 * (2.0 * csi - eta) * (1.0 + eta);
                case 5:
                    return -csi * (1.0 - eta);
                case 6:
                    return 1.0 / 2.0 * (1.0 - eta * eta);
                case 7:
                    return -csi * (1.0 + eta);
                case 8:
                    return -1.0 / 2.0 * (1.0 - eta * eta);
                default:
                    throw new Exception();
            }
        }

        public static double dNdEta(int index, double csi, double eta)
        {
            switch (index)
            {
                case 1:
                    return 1.0 / 4.0 * (2.0 * eta + csi) * (1.0 - csi);
                case 2:
                    return 1.0 / 4.0 * (2.0 * eta - csi) * (1.0 + csi);
                case 3:
                    return 1.0 / 4.0 * (2.0 * eta + csi) * (1.0 + csi);
                case 4:
                    return 1.0 / 4.0 * (2.0 * eta - csi) * (1.0 - csi);
                case 5:
                    return -1.0 / 2.0 * (1.0 - csi * csi);
                case 6:
                    return -eta * (1.0 + csi);
                case 7:
                    return 1.0 / 2.0 * (1.0 - csi * csi);
                case 8:
                    return -eta * (1.0 - csi);
                default:
                    throw new Exception();
            }
        }
        #endregion

        /// <summary>
        /// jacobiano:
        /// dx/dCsi, dy/dCsi
        /// dy/dEta, dy/dEta
        /// </summary>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <returns></returns>
        public static mnl.Matrix<double> J(double csi, double eta, Node[] localNodes)
        {
            double j11 = 0.0;
            double j12 = 0.0;
            double j21 = 0.0;
            double j22 = 0.0;
            for (int node = 0; node < 8; node++)
            {
                int i = node + 1;
                double xi = localNodes[node].Position.X;
                double yi = localNodes[node].Position.Y;

                j11 = j11 + Quad8Element.dNdCsi(i, csi, eta) * xi;
                j12 = j12 + Quad8Element.dNdCsi(i, csi, eta) * yi;
                j21 = j21 + Quad8Element.dNdEta(i, csi, eta) * xi;
                j22 = j22 + Quad8Element.dNdEta(i, csi, eta) * yi;
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
    }
}
