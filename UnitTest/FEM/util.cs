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
            Console.WriteLine("dim: " + m.RowCount + " x " + m.ColumnCount);
            for (int r = 0; r < m.RowCount; r++)
            {
                for (int c = 0; c < m.ColumnCount; c++)
                {
                    Console.Write(m[r,c].ToString(format) + " \t");
                }
                Console.WriteLine();
            }
        }

        public static void WriteMatrix(mnl.Vector<double> v, string format)
        {
            Console.WriteLine("dim: "+ v.Count);
            for (int r = 0; r < v.Count; r++)
            {
                 Console.Write(v[r].ToString(format));
            }
        }

        public static void WriteVector(mnl.Vector<double> v, string format)
        {
            WriteMatrix(v, format);
        }
    }
}
