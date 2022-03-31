using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Maths.Matrices
{
    public static class IKeyMatrixExtensions
    {


        /// <inheritdoc cref="MathNet.Numerics.LinearAlgebra.Matrix{T}.At(int, int, T)"/>
        public static void SetElementAt<TRow, TColumn>(this IKeyMatrix<TRow, TColumn> matrix, TRow row, TColumn column, double value)
        {
            matrix[matrix.GetIndex(row), matrix.GetIndex(column)] = value;
        }


        /// <inheritdoc cref="MathNet.Numerics.LinearAlgebra.Matrix{T}.At(int, int)"/>
        public static double GetElementAt<TRow, TColumn>(this IKeyMatrix<TRow, TColumn> matrix, TRow row, TColumn column)
        {
            return matrix[matrix.GetIndex(row), matrix.GetIndex(column)];
        }



    }

}
