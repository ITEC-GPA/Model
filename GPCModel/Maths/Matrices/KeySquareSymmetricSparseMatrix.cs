using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GPC.Model.Maths.Matrices
{
    public class KeySquareSymmetricSparseMatrix<TKey> : SparseMatrix, IKeyMatrix<TKey, TKey>
    {

        protected Dictionary<TKey, int> _keyIndex;


        public KeySquareSymmetricSparseMatrix(IEnumerable<TKey> keys, SparseMatrix matrix)
            : this(keys)
        {
            SetSubMatrix(0, 0, matrix);
        }

        public KeySquareSymmetricSparseMatrix(IEnumerable<TKey> keys)
            : base(keys.Count())
        {
            _keyIndex = new Dictionary<TKey, int>();
            InitKeyIndices(keys);

        }


        private void InitKeyIndices(IEnumerable<TKey> keys)
        {

            int i = 0;
            foreach (var item in keys)
            {
                _keyIndex.Add(item, i++);
            }

        }

        public bool ContainsKey(TKey key)
        {
            return _keyIndex.ContainsKey(key);
        }

        public int GetIndex(TKey key)
        {
            return _keyIndex[key];
        }

        /// <inheritdoc cref="MathNet.Numerics.LinearAlgebra.Matrix{T}.At(int, int, T)"/>
        public void SetElementAt(TKey key, double value)
        {
            At(GetIndex(key), GetIndex(key), value);
        }

        /// <inheritdoc cref="MathNet.Numerics.LinearAlgebra.Matrix{T}.At(int, int, T)"/>
        public void SetElementAtSymmetric(TKey row, TKey column, double value)
        {
            At(GetIndex(row), GetIndex(column), value);
            At(GetIndex(column), GetIndex(row), value);
        }

        /// <inheritdoc cref="MathNet.Numerics.LinearAlgebra.Matrix{T}.At(int, int)"/>
        public double GetElementAt(TKey row)
        {
            return At(GetIndex(row), GetIndex(row));
        }

        public TKey[] GetColumnKeys()
        {
            return _keyIndex.Keys.ToArray();
        }

        public TKey[] GetRowKeys()
        {
            return _keyIndex.Keys.ToArray();
        }
    }
}
