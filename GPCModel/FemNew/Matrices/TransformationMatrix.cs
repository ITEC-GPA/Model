using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Maths.Matrices;

namespace GPC.Model.Fem
{
    public class TransformationMatrix : KeySquareSymmetricSparseMatrix<NodalGlobalDegreeOfFreedom>
    {



        public TransformationMatrix(IEnumerable<NodalGlobalDegreeOfFreedom> keys)
            : base(keys)
        {

        }


        public TransformationMatrix(IEnumerable<NodalGlobalDegreeOfFreedom> keys, CoordinateSystem coordinateSystem)
            : base(keys)
        {

            if (keys.Count() % 3 == 0)
            {
                var submatrix = coordinateSystem.TrfMatrix.RemoveColumn(3).Transpose(); // va trasporta perchè il nostro sistema di riferimento ha la trfmatrix definita al contrario

                for (int i = 0; i < keys.Count() / 3; i++)
                {
                    SetSubMatrix(i * 3, i * 3, submatrix);
                }
            }
            else
            {
                throw new ArgumentException("Degrees %3 != 0");
            }
        }

    }
}
