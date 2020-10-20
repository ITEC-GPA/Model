using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.LoadCases
{
    [Serializable]
    public abstract class LoadCase : ModelObject
    {
        private string _name;

        protected LoadCase(string name, Guid guid) 
            : base(guid)
        {
            this._name = name;
        }

        protected LoadCase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _name = info.GetString("Name");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Name", _name);
        }
    }
}
