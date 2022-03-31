using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GPC.Model.LoadCases;

namespace GPC.Model.Fem.Attributes
{
    [Serializable]
    public abstract class LoadCaseAttribute : Attribute, ISerializable
    {
        public string LoadCaseName => _caseName;

        public LoadCaseAttribute(string loadCaseName)
            : this(loadCaseName, string.Empty, Guid.NewGuid())
        {

        }

        public LoadCaseAttribute(string loadCaseName, string attributeName)
            : this(loadCaseName, attributeName, Guid.NewGuid())
        {

        }

        public LoadCaseAttribute(string loadCaseName, string name, Guid guid)
            : base(loadCaseName, name, guid)
        {

        }

        public LoadCaseAttribute(LoadCaseAttribute loadCaseAttribute)
            : base(loadCaseAttribute.LoadCaseName, loadCaseAttribute.Name, loadCaseAttribute.Guid)
        {

        }



        protected LoadCaseAttribute(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return (obj is LoadCaseAttribute objCasted) && base.Equals(objCasted);
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                return hashCode;
            }
        }



        #region Override Operator
        public static bool operator ==(LoadCaseAttribute obj1, LoadCaseAttribute obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            return obj1.Equals(obj2);
        }

        public static bool operator !=(LoadCaseAttribute obj1, LoadCaseAttribute obj2)
        {
            return !(obj1 == obj2);
        }
        #endregion
    }
}