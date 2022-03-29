using System;
using System.Collections.Generic;

namespace GPC.Model.Fem.Attributes
{


    /// <returns>Two <see cref="Attribute"/> are <see langword="true"/> if <see cref="Attribute.CaseName"/> are equals</returns>
    [Serializable]
    public class AttributeEqualityComparer : EqualityComparer<Attribute>
    {

        /// <returns><see langword="true"/> if <see cref="Attribute.CaseName"/> are equals</returns>
        public override bool Equals(Attribute x, Attribute y)
        {

            if (x.CaseName.Equals(y.CaseName))
                return true;

            return false;
        }


        public override int GetHashCode(Attribute obj)
        {
            unchecked
            {
                return -17 * obj.CaseName.GetHashCode();
            }
        }
    }
}
