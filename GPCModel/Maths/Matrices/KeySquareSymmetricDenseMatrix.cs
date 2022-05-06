using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GPC.Model.Maths.Matrices
{
    public class KeySquareSymmetricDenseMatrix<TKey> : KeyDenseMatrix<TKey, TKey>
    {


        public KeySquareSymmetricDenseMatrix(IEnumerable<TKey> keys, DenseMatrix matrix)
            : base(keys, keys, matrix)
        {

        }

        public KeySquareSymmetricDenseMatrix(IEnumerable<TKey> keys, Matrix<double> matrix)
            : base(keys, keys, matrix)
        {

        }

        public KeySquareSymmetricDenseMatrix(IEnumerable<TKey> keys)
            : base(keys, keys)
        {

        }


        /// <inheritdoc cref="Matrix{T}.At(int, int, T)"/>
        public void SetElementAt(TKey key, double value)
        {
            base.SetElementAt(key, key, value);
        }

        /// <inheritdoc cref="Matrix{T}.At(int, int, T)"/>
        public override void SetElementAt(TKey row, TKey column, double value)
        {
            base.SetElementAt(row, column, value);
            base.SetElementAt(column, row, value);
        }

        public virtual void AddMatrix(TKey[] rows, Matrix<double> matrix)
        {
            if (!matrix.IsSymmetric())
                throw new ArgumentException();

            base.AddMatrix(rows, rows, matrix);
        }


        /// <inheritdoc cref="Matrix{T}.At(int, int)"/>
        public double GetElementAt(TKey row)
        {
            return GetElementAt(row, row);
        }


        /// <inheritdoc cref="Matrix{T}.At(int, int)"/>
        public double GetElementAt(TKey row, TKey column)
        {
            return GetElementAt(row, column);
        }

    }
}
