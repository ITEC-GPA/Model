using System.Collections.Generic;

namespace GPC.Model.FEM.Attributes
{
    public class LoadCaseAttributeEqualityComparer : EqualityComparer<LoadCaseAttribute>
    {

        public override bool Equals(LoadCaseAttribute x, LoadCaseAttribute y)
        {

            if (x.LoadCaseName.Equals(y.LoadCaseName))
                return true;

            return false;
        }

        public override int GetHashCode(LoadCaseAttribute obj)
        {
            return -17 * obj.LoadCaseName.GetHashCode();
        }
    }
}
