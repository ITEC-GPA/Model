using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GPC.Model.FreedomCases;

namespace GPC.Model.FEM.Attributes
{
    public abstract class FreedomCaseAttribute : Attribute, ISerializable
    {
        private string _freedomCaseName;

        public string FreedomCaseName => _freedomCaseName;

        public FreedomCaseAttribute(string freedomCaseName)
            : this(freedomCaseName, string.Empty, Guid.NewGuid())
        {

        }

        public FreedomCaseAttribute(string freedomCaseName, string name)
            : this(freedomCaseName, name, Guid.NewGuid())
        {

        }

        public FreedomCaseAttribute(string freedomCaseName, string name, Guid guid)
            : base(guid, name)
        {
            _freedomCaseName = String.IsNullOrEmpty(freedomCaseName) || String.IsNullOrWhiteSpace(freedomCaseName) ? throw new ArgumentNullException() : freedomCaseName;
        }
        


        public FreedomCaseAttribute(FreedomCaseAttribute freedomCaseAttribute)
            : base(freedomCaseAttribute.Guid, freedomCaseAttribute._name)
        {
            _freedomCaseName = freedomCaseAttribute._freedomCaseName;
        }




        protected FreedomCaseAttribute(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _freedomCaseName = (string)info.GetValue("FreedomCase", typeof(string));
        }




        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            FreedomCaseAttribute lca = obj as FreedomCaseAttribute;

            return !(lca is null) && _freedomCaseName.Equals(lca._freedomCaseName) && base.Equals(lca);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("FreedomCase", _freedomCaseName);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<string>.Default.GetHashCode(_freedomCaseName);
            return hashCode;
        }

        #region Override Operator

        public static bool operator ==(FreedomCaseAttribute obj1, FreedomCaseAttribute obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(FreedomCaseAttribute obj1, FreedomCaseAttribute obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion Override Operator
    }
}