using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.FEM.Attributes
{
    [Serializable]
    public abstract class Attribute : ModelObject, ISerializable, ICloneable
    {

        protected readonly string _caseName;

        internal string CaseName => _caseName;

        public Attribute(string loadCaseName)
            : this(loadCaseName, string.Empty, Guid.NewGuid())
        {
        }


        public Attribute(string loadCaseName, string attributeName)
            : this(loadCaseName, attributeName, Guid.NewGuid())
        {

        }

        public Attribute(string loadCaseName, string name, Guid guid)
            : base(guid, name)
        {
            _caseName = string.IsNullOrEmpty(loadCaseName) || string.IsNullOrWhiteSpace(loadCaseName) ? throw new ArgumentNullException() : loadCaseName;
        }

        public Attribute(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _caseName = (string)info.GetValue("caseName", typeof(string));
        }


        public abstract object Clone();

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return (obj is Attribute objCasted) && _caseName.Equals(objCasted._caseName) && base.Equals(objCasted);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<string>.Default.GetHashCode(_caseName);
                return hashCode;
            }
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("caseName", _caseName);
        }



        #region Override Operator


        public static bool operator ==(Attribute obj1, Attribute obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }


        public static bool operator !=(Attribute obj1, Attribute obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}