using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GPC.Model.Maths.Matrices
{
    public class KeySquareSymmetricSparseMatrix<TKey> : KeySparseMatrix<TKey, TKey>
    {


        public KeySquareSymmetricSparseMatrix(IEnumerable<TKey> keys, SparseMatrix matrix)
            : base(keys, keys, matrix)
        {

        }

        public KeySquareSymmetricSparseMatrix(IEnumerable<TKey> keys, MathNet.Numerics.LinearAlgebra.Matrix<double> matrix)
            : base(keys, keys, matrix)
        {
            SetSubMatrix(0, 0, matrix);
        }

        public KeySquareSymmetricSparseMatrix(IEnumerable<TKey> keys)
            : base(keys, keys)
        {

        }

        #region Edit

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

        #endregion


        #region Get

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

        #endregion
    }
}
