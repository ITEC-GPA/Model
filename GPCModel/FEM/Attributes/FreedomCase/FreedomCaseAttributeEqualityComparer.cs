using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM.Attributes
{
    public class FreedomCaseAttributeEqualityComparer : EqualityComparer<FreedomCaseAttribute>
    {

        public override bool Equals(FreedomCaseAttribute x, FreedomCaseAttribute y)
        {

            if (x.FreedomCaseName.Equals(y.FreedomCaseName))
                return true;

            return false;
        }


        public override int GetHashCode(FreedomCaseAttribute obj)
        {
            return -17 * obj.FreedomCaseName.GetHashCode();
        }
    }
}
