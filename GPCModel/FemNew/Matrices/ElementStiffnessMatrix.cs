using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Maths.Matrices;
using GPC.Model.Fem.FemObjects;
using GPC.Model.Fem;
using MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.Fem
{

    internal class ElementStiffnessMatrix : KeySquareSymmetricDenseMatrix<NodalLocalDegreeOfFreedom>
    {


        public ElementStiffnessMatrix(IEnumerable<NodalLocalDegreeOfFreedom> keys) 
            : base(keys)
        {
            
        }

        public ElementStiffnessMatrix(IEnumerable<NodalLocalDegreeOfFreedom> keys, Matrix<double> matrix)
            : base(keys, matrix)
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

    }
}
