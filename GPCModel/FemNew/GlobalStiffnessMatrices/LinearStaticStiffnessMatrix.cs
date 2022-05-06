using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem.FemObjects;
using GPC.Model.Fem.FemObjects.FiniteElements;


namespace GPC.Model.Fem
{

    internal class LinearStaticStiffnessMatrix : FemModelGlobalStiffnessMatrix
    {


        public LinearStaticStiffnessMatrix(IEnumerable<FiniteElement> finiteElements)
            : base(finiteElements)
        {

        }


        public override void BuildStiffnessMatrix(FiniteElement[] finiteElements)
        {

            var elementsGlobalStiffnessMatrix = new ElementGlobalStiffnessMatrix[finiteElements.Length];

            for (int e = 0; e < finiteElements.Length; e++)
            {
                var matrix = finiteElements[e].GetGlobalStiffnessMatrix();

                elementsGlobalStiffnessMatrix[e] = matrix;
            }


            var globalDofs = new HashSet<NodalGlobalDegreeOfFreedom>();



            for (int i = 0; i < elementsGlobalStiffnessMatrix.Length; i++)
            {

                NodalGlobalDegreeOfFreedom[] rows = elementsGlobalStiffnessMatrix[i].GetRowKeysGlobalDegreeOfFreedom();

                // qua bisognerà filtrare per se ci sono dof non supportati da questa matrice, dofs con stesso nodo e dof diversi
                for (int r = 0; r < rows.Length; r++)
                {
                    if (!globalDofs.Contains(rows[r]))
                    {
                        globalDofs.Add(rows[r]);
                    }
                }
            }



            _globalStiffnessMatrix = new GlobalStiffnessMatrix(globalDofs);


            for (int e = 0; e < elementsGlobalStiffnessMatrix.Length; e++)
            {
                var rows = elementsGlobalStiffnessMatrix[e].GetRowKeysGlobalDegreeOfFreedom();

                _globalStiffnessMatrix.AddMatrix(rows, elementsGlobalStiffnessMatrix[e]);

            }

        }
    }
}
