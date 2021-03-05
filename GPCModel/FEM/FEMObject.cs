using System;
using System.Runtime.Serialization;
using GPC.Model.Elements;

namespace GPC.Model.FEM
{
    public abstract class FEMObject : Element
    {
        public FEMObject(int id) 
            : this(id, string.Empty)
        {

        }

        public FEMObject(int id, string name) 
            : base(Guid.NewGuid(), name)
        {
            base.Id = id;
        }

        public FEMObject(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

        }       

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            
            FEMObject objCasted = obj as FEMObject;
            return !(objCasted is null) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(FEMObject obj1, FEMObject obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(FEMObject obj1, FEMObject obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
