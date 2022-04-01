using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Maths.Matrices;

namespace GPC.Model.Fem
{
    public class TransformationMatrix : KeySquareSymmetricSparseMatrix<NodalGlobalDegreeOfFreedom>
    {



        public TransformationMatrix(IEnumerable<NodalGlobalDegreeOfFreedom> keys) 
            : base(keys)
        {

        }



    }
}
