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
        protected int _index;

        public int Index => _index;

        public FEMObject(int index) : this(index, string.Empty)
        {
        }

        public FEMObject(int index, string name) : base(Guid.NewGuid(), name)
        {
        }

        public FEMObject(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            info.AddValue("index", _index);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            _index = info.GetInt32("Index");
        }       

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            
            FEMObject objCasted = obj as FEMObject;
            return !(objCasted is null) && base.Equals(objCasted) && _index == objCasted._index;
        }

        public override int GetHashCode()
        {
            int hashCode = -738623263;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Index.GetHashCode();
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
