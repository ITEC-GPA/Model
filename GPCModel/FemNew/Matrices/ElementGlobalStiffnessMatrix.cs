using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem;
using GPC.Model.Maths.Matrices;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GPC.Model.Fem
{

    public class ElementGlobalStiffnessMatrix : KeySquareSymmetricDenseMatrix<INodalDegreeOfFreedom>
    {



        internal ElementGlobalStiffnessMatrix(IEnumerable<NodalGlobalDegreeOfFreedom> keys)
            : base(keys.Cast<INodalDegreeOfFreedom>())
        {

        }

        internal ElementGlobalStiffnessMatrix(IEnumerable<NodalGlobalDegreeOfFreedom> keys, Matrix<double> matrix)
            : base(keys.Cast<INodalDegreeOfFreedom>(), matrix)
        {

        }


        protected ElementGlobalStiffnessMatrix(IEnumerable<INodalDegreeOfFreedom> keys, Matrix<double> matrix) :
            base(keys, matrix)
        {

        }


        public NodalGlobalDegreeOfFreedom[] GetRowKeysGlobalDegreeOfFreedom()
        {
            return GetRowKeys().Cast<NodalGlobalDegreeOfFreedom>().ToArray();
        }


        /// <summary>
        /// perform a multiplication L^T * M * L. Where L is the <paramref name="transformationMatrix"/> and M this object
        /// </summary>
        /// <returns>A new <see cref="ElementLocalStiffnessMatrix"/> with keys equal to the one of this object</returns>
        public ElementGlobalStiffnessMatrix PrePostMultiply(TransformationMatrix transformationMatrix)
        {
            return new ElementGlobalStiffnessMatrix(GetRowKeys(), transformationMatrix.TransposeThisAndMultiply(this).Multiply(transformationMatrix));
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
