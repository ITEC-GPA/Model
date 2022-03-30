using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GPC.Model.Maths.Matrices
{

    internal class DenseKeyMatrix<TRow, TColumn> : KeyMatrix<TRow, TColumn, DenseMatrix>
    {

        public DenseKeyMatrix(IEnumerable<TRow> rows, IEnumerable<TColumn> columns)
            : base(rows, columns)
        {

        }

        protected override DenseMatrix InitMatrix(int rows, int columns)
        {
            return new DenseMatrix(rows, columns);
        }
    }
}
