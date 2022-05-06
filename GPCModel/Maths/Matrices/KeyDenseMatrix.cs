using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GPC.Model.Maths.Matrices
{

    public class KeyDenseMatrix<TRow, TColumn> : DenseMatrix, IKeyMatrix<TRow, TColumn>
    {
        protected Dictionary<TRow, int> _rowKeyIndex;
        protected Dictionary<TColumn, int> _columnKeyIndex;



        public KeyDenseMatrix(IEnumerable<TRow> rows, IEnumerable<TColumn> columns, DenseMatrix matrix)
            : this(rows, columns)
        {
            SetSubMatrix(0, 0, matrix);
        }

        public KeyDenseMatrix(IEnumerable<TRow> rows, IEnumerable<TColumn> columns, Matrix<double> matrix)
            : this(rows, columns)
        {
            SetSubMatrix(0, 0, matrix);
        }

        /// <inheritdoc cref="KeySparseMatrix{TRow, TColumn}.KeySparseMatrix(IEnumerable{TRow}, IEnumerable{TColumn})"/>
        public KeyDenseMatrix(IEnumerable<TRow> rows, IEnumerable<TColumn> columns)
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


        #region Edit

        /// <inheritdoc cref="Matrix{T}.At(int, int, T)"/>
        public virtual void SetElementAt(TRow row, TColumn column, double value)
        {
            At(GetIndex(row), GetIndex(column), value);
        }

        public void SumElementAt(TRow row, TColumn column, double value)
        {
            At(GetIndex(row), GetIndex(column), At(GetIndex(row), GetIndex(column)) + value);
        }

        /// <summary>
        /// Sum the values of matrix to this one at the right location
        /// </summary>
        public void AddMatrix(TRow[] row, TColumn[] column, Matrix<double> matrix)
        {
            if (row.Length != matrix.RowCount || column.Length != matrix.ColumnCount 
                || row.Length != _rowKeyIndex.Count() || column.Length != _columnKeyIndex.Count())
            {
                throw new ArgumentException();
            }

            for (int r = 0; r < row.Length; r++)
            {
                for (int c = 0; c < column.Length; c++)
                {
                    SumElementAt(row[r], column[c], matrix[r, c]); // ottimizzabile 
                }
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

        /// <exception cref="KeyNotFoundException"></exception>
        // serve per evitare ambiguità se le chiavi sono uguali 
        protected int GetRowIndex(TRow row)
        {
            return _rowKeyIndex[row];
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
