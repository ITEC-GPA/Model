using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Combinations
{
    public abstract class CombinationAsce : ModelObject, ICombination
    {
        protected CombinationAsce(Guid guid) : base(guid)
        {
        }

        protected CombinationAsce(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string Name => throw new NotImplementedException();

        public bool isUltimate => throw new NotImplementedException();
    }
}
