using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem;
using GPC.Model.Fem.FemObjects.FiniteElements;
using GPC.Model.Maths.Matrices;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GPC.Model.Fem
{

    internal class GlobalStiffnessMatrix : KeySquareSymmetricSparseMatrix<INodalDegreeOfFreedom>
    {


        public GlobalStiffnessMatrix(IEnumerable<NodalGlobalDegreeOfFreedom> keys)
            : base(keys.Cast<INodalDegreeOfFreedom>())
        {

        }

        public GlobalStiffnessMatrix(IEnumerable<NodalGlobalDegreeOfFreedom> keys, Matrix<double> matrix)
            : base(keys.Cast<INodalDegreeOfFreedom>(), matrix)
        {

        }


        public virtual void AddMatrix(NodalGlobalDegreeOfFreedom[] rows, Matrix<double> matrix)
        {
            base.AddMatrix(rows.Cast<INodalDegreeOfFreedom>().ToArray(), matrix);
        }

    }
}
