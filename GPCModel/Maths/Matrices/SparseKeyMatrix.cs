using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GPC.Model.Maths.Matrices
{

    internal class SparseKeyMatrix<TRow, TColumn> : KeyMatrix<TRow, TColumn, SparseMatrix>
    {

        public SparseKeyMatrix(IEnumerable<TRow> rows, IEnumerable<TColumn> columns)
            : base(rows, columns)
        {

        }

        protected override SparseMatrix InitMatrix(int rows, int columns)
        {
            return new SparseMatrix(rows, columns);
        }
    }
}
