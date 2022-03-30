using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GPC.Model.Maths.Matrices
{
    internal abstract class KeyMatrix<TRow, TColumn, M> where M : Matrix
    {
        protected readonly M _matrix;

        protected Dictionary<TRow, int> _rowKeyIndex;
        protected Dictionary<TColumn, int> _columnKeyIndex;



        public KeyMatrix(IEnumerable<TRow> rows, IEnumerable<TColumn> columns)
        {
            _matrix = InitMatrix(rows.Count(), columns.Count());

            InitKeyIndices(rows, columns); // can raise exception if two equals item are added

        }

        protected abstract M InitMatrix(int rows, int columns);


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


    }

}
