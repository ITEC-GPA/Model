using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Maths.Matrices;
using GPC.Model.Fem;
using GPC.Model.Fem.FemObjects;

namespace GPC.Model.Fem.StiffnessMatrix
{

    internal class ElementStiffnessMatrix : KeySquareSymmetricDenseMatrix<NodalDegreeOfFreedom>
    {


        public ElementStiffnessMatrix(IEnumerable<NodalDegreeOfFreedom> keys) 
            : base(keys)
        {
            
        }


    }
}
