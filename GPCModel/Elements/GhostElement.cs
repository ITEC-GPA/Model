using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements
{
    /// <summary>
    /// The purpose of this element is to give an instance to the abstract class Element.
    /// This can be usefull for example for debug purposes 
    /// </summary>
    internal class GhostElement : Element
    {
        public GhostElement(int id) : base(id)
        {

        }

        public GhostElement(int id, string name) : base(id, name, Guid.NewGuid())
        {

        }


        public GhostElement(Guid guid) : base(guid)
        {

        }

        public GhostElement(Guid guid, string name, int id) : base(id, name, guid)
        {

        }

        public GhostElement(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }

        public override bool Equals(object obj)
        {
            return obj is GhostElement element && base.Equals(element);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
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
