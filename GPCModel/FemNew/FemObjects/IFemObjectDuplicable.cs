using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Fem.FemObjects
{
    internal interface IFemObjectDuplicable<T> where T : FemObject
    {
        T Duplicate();
    }
}
