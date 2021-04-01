using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Elements
{
    /// <summary>
    /// The purpose of this element is to give an instance to the abstract class Element.
    /// This can be usefull for example for debug purposes 
    /// </summary>
    internal class GhostElement : Element
    {
        public GhostElement(int id)
        {
            Id = id;
        }

        public GhostElement(int id, string name)
        {
            Id = id;
            this._name = name;
        }


        public GhostElement(Guid guid) : base(guid)
        {
        }

        public GhostElement(Guid guid, string name, int id) : base(guid, name)
        {
            Id = id;
        }

        public GhostElement(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override bool Equals(object obj)
        {
            return obj is GhostElement element && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 624022166 + base.GetHashCode();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
