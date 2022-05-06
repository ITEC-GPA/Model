using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem.FemObjects.FiniteElements;


namespace GPC.Model.Fem
{

    internal abstract class FemModelGlobalStiffnessMatrix : ModelObjectId
    {

        protected GlobalStiffnessMatrix _globalStiffnessMatrix;

        internal GlobalStiffnessMatrix GlobalStiffnessMatrix => _globalStiffnessMatrix;

        public FemModelGlobalStiffnessMatrix(IEnumerable<FiniteElement> finiteElements)
        {
            BuildStiffnessMatrix(finiteElements.ToArray());
        }


        public abstract void BuildStiffnessMatrix(FiniteElement[] finiteElements);



    }
}
