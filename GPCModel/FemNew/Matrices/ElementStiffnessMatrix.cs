using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Maths.Matrices;
using GPC.Model.Fem.FemObjects;
using GPC.Model.Fem;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GPC.Model.Fem
{

    public class ElementStiffnessMatrix : KeySquareSymmetricDenseMatrix<INodalDegreeOfFreedom>
    {


        public ElementStiffnessMatrix(IEnumerable<INodalDegreeOfFreedom> keys)
            : base(keys)
        {

        }

        public ElementStiffnessMatrix(IEnumerable<INodalDegreeOfFreedom> keys, Matrix<double> matrix)
            : base(keys, matrix)
        {

        }

        public ElementStiffnessMatrix(IEnumerable<NodalLocalDegreeOfFreedom> keys)
            : base(keys.Cast<INodalDegreeOfFreedom>())
        {

        }

        public ElementStiffnessMatrix(IEnumerable<NodalLocalDegreeOfFreedom> keys, Matrix<double> matrix)
            : base(keys.Cast<INodalDegreeOfFreedom>(), matrix)
        {

        }

        public ElementStiffnessMatrix(IEnumerable<NodalGlobalDegreeOfFreedom> keys)
            : base(keys.Cast<INodalDegreeOfFreedom>())
        {

        }

        public ElementStiffnessMatrix(IEnumerable<NodalGlobalDegreeOfFreedom> keys, Matrix<double> matrix)
            : base(keys.Cast<INodalDegreeOfFreedom>(), matrix)
        {

        }

        /// <summary>
        /// perform a multiplication L^T * M * L. Where L is the <paramref name="transformationMatrix"/> and M this object
        /// </summary>
        /// <returns>A new <see cref="ElementStiffnessMatrix"/> with keys equal to the one of this object</returns>
        public ElementStiffnessMatrix PrePostMultiply(TransformationMatrix transformationMatrix)
        {
            return new ElementStiffnessMatrix(GetRowKeys(), transformationMatrix.TransposeThisAndMultiply(this).Multiply(transformationMatrix));
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
