using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Maths.Matrices;

namespace GPC.Model.Fem
{
    internal class TransformationMatrix : KeySquareSymmetricSparseMatrix<NodalDegreeOfFreedom>
    {



        public TransformationMatrix(IEnumerable<NodalDegreeOfFreedom> keys) 
            : base(keys)
        {

        }



    }
}
