using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem;
using GPC.Model.Fem.FemObjects;
using GPC.Model.Maths.Matrices;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GPC.Model.Fem
{

    public class ElementLocalStiffnessMatrix : KeySquareSymmetricDenseMatrix<INodalDegreeOfFreedom>
    {


        public ElementLocalStiffnessMatrix(IEnumerable<NodalLocalDegreeOfFreedom> keys)
            : base(keys.Cast<INodalDegreeOfFreedom>())
        {

        }

        public ElementLocalStiffnessMatrix(IEnumerable<NodalLocalDegreeOfFreedom> keys, Matrix<double> matrix)
            : base(keys.Cast<INodalDegreeOfFreedom>(), matrix)
        {

        }


        internal string ToStringKeyMatrix()
        {
            int padSize = 8;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(base.ToTypeString());

            INodalDegreeOfFreedom[] columnKeys = GetColumnKeys();
            INodalDegreeOfFreedom[] rowsKeys = GetRowKeys();

            sb.Append("".PadLeft(padSize));
            columnKeys.ToList().ForEach(i => sb.Append(i.ToStringDegreeOfFreedom().PadLeft(padSize)));
            sb.Append(Environment.NewLine);


            for (int r = 0; r < RowCount; r++)
            {
                sb.Append(rowsKeys[r].ToStringDegreeOfFreedom().PadLeft(padSize));

                for (int c = 0; c < ColumnCount; c++)
                {
                    sb.Append(this[r, c].ToString("G2").PadLeft(padSize));
                }
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }
    }
}
