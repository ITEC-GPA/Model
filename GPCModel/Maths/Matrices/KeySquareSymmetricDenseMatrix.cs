using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GPC.Model.Maths.Matrices
{
    public class KeySquareSymmetricDenseMatrix<TKey> : KeyDenseMatrix<TKey, TKey>
    {


        public KeySquareSymmetricDenseMatrix(IEnumerable<TKey> keys, DenseMatrix matrix)
            : base(keys, keys, matrix)
        {

        }

        public KeySquareSymmetricDenseMatrix(IEnumerable<TKey> keys, MathNet.Numerics.LinearAlgebra.Matrix<double> matrix)
            : base(keys, keys, matrix)
        {
            SetSubMatrix(0, 0, matrix);
        }

        public KeySquareSymmetricDenseMatrix(IEnumerable<TKey> keys)
            : base(keys, keys)
        {

        }


        /// <inheritdoc cref="MathNet.Numerics.LinearAlgebra.Matrix{T}.At(int, int, T)"/>
        public void SetElementAt(TKey key, double value)
        {
            At(GetRowIndex(key), GetRowIndex(key), value);
        }

        /// <inheritdoc cref="MathNet.Numerics.LinearAlgebra.Matrix{T}.At(int, int, T)"/>
        public void SetElementAtSymmetric(TKey row, TKey column, double value)
        {
            At(GetRowIndex(row), GetRowIndex(column), value);
            At(GetRowIndex(column), GetRowIndex(row), value);
        }

        /// <inheritdoc cref="MathNet.Numerics.LinearAlgebra.Matrix{T}.At(int, int)"/>
        public double GetElementAt(TKey row)
        {
            return At(GetRowIndex(row), GetRowIndex(row));
        }

        /// <inheritdoc cref="MathNet.Numerics.LinearAlgebra.Matrix{T}.At(int, int)"/>
        public double GetElementAt(TKey row, TKey column)
        {
            return At(GetRowIndex(row), GetRowIndex(column));
        }

    }
}
