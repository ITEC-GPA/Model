using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM
{
    public class FEMObject : ModelObject
    {
        protected int _id;

        public int Id { get => _id; internal set => _id = value; }

        public FEMObject(int id) 
            : this(id, string.Empty)
        {

        }

        public FEMObject(int id, string name) 
            : base(Guid.NewGuid(), name)
        {
            _id = id;
        }

        public FEMObject(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            info.AddValue("Id", _id);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            _id = info.GetInt32("Id");
        }       

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            
            FEMObject objCasted = obj as FEMObject;
            return !(objCasted is null) && base.Equals(objCasted) && _id == objCasted._id;
        }

        public override int GetHashCode()
        {
            int hashCode = -738623263;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
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
