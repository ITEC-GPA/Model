using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GPC.Model.Maths.Matrices
{

    public class KeySparseMatrix<TRow, TColumn> : SparseMatrix, IKeyMatrix<TRow, TColumn>
    {
        protected Dictionary<TRow, int> _rowKeyIndex;
        protected Dictionary<TColumn, int> _columnKeyIndex;


        public KeySparseMatrix(IEnumerable<TRow> rows, IEnumerable<TColumn> columns, SparseMatrix matrix)
            : this(rows, columns)
        {
            SetSubMatrix(0, 0, matrix);
        }

        /// <summary>
        /// If <paramref name="rows"/> or <paramref name="columns"/> contains duplicates, a <see cref="SystemException"/> will be raised
        /// </summary>
        public KeySparseMatrix(IEnumerable<TRow> rows, IEnumerable<TColumn> columns)
            : base(rows.Count(), columns.Count())
        {

            _rowKeyIndex = new Dictionary<TRow, int>();
            _columnKeyIndex = new Dictionary<TColumn, int>();
            InitKeyIndices(rows, columns);

        }


        #region Constructors and Init methods


        private void InitKeyIndices(IEnumerable<TRow> rows, IEnumerable<TColumn> columns)
        {

            int i = 0;
            foreach (var item in rows)
            {
                _rowKeyIndex.Add(item, i++);
            }

            i = 0;
            foreach (var item in columns)
            {
                _columnKeyIndex.Add(item, i++);
            }

        }

        #endregion

        /// <exception cref="KeyNotFoundException"></exception>
        public int GetIndex(TRow row)
        {
            return _rowKeyIndex[row];
        }

        /// <exception cref="KeyNotFoundException"></exception>
        public int GetIndex(TColumn column)
        {
            return _columnKeyIndex[column];
        }

        public bool ContainsKey(TRow row)
        {
            return _rowKeyIndex.ContainsKey(row);
        }

        public bool ContainsKey(TColumn column)
        {
            return _columnKeyIndex.ContainsKey(column);
        }
        public TColumn[] GetColumnKeys()
        {
            return _columnKeyIndex.Keys.ToArray();
        }

        public TRow[] GetRowKeys()
        {
            return _rowKeyIndex.Keys.ToArray();
        }
    }
}
