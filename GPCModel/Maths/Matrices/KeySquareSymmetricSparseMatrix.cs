using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GPC.Model.Maths.Matrices
{
    public class KeySquareSymmetricSparseMatrix<TKey> : KeySparseMatrix<TKey, TKey>
    {


        public KeySquareSymmetricSparseMatrix(IEnumerable<TKey> keys, SparseMatrix matrix)
            : base(keys, keys, matrix)
        {

        }

        public KeySquareSymmetricSparseMatrix(IEnumerable<TKey> keys)
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

    }
}
