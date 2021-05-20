using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model
{

    public class ModelObjectIdSet<T> : ModelObjectSet<T> where T : ModelObjectId
    {

        public ModelObjectIdSet()
        {

        }

        public ModelObjectIdSet(IEqualityComparer<T> comparer) : base(comparer)
        {

        }

    }
}
