using GPC.Model.Combinations;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Standards
{
    public class StandardUNIEN1993p11 : StandardEN1993p11
    {
        
        public StandardUNIEN1993p11()
        {
            _gammaM0 = 1.05;
            _gammaM1 = 1.10;
            _gammaM2 = 1.25;
        }

    }
}
