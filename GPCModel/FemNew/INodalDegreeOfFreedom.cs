using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem.FemObjects;

namespace GPC.Model.Fem
{

    public interface INodalDegreeOfFreedom
    {
        Node Node { get; }

        string ToStringDegreeOfFreedom();
    }
}
