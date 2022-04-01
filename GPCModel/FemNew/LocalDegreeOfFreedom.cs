using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Fem
{

    public enum LocalDegreeOfFreedom 
    {
        D1,
        D2,
        D3,
        R1,
        R2,
        R3,        
    }

    public enum GlobalDegreeOfFreedom
    {
        DX,
        DY,
        DZ,
        RX,
        RY,
        RZ,
    }
}
