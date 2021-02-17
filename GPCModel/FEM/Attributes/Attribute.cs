using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM.Attributes
{
    public abstract class Attribute : ModelObject
    {
        protected Attribute()
        {
        }

        protected Attribute(Guid guid) : base(guid)
        {
        }

        protected Attribute(Guid guid, string name) : base(guid, name)
        {
        }

        protected Attribute(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}


