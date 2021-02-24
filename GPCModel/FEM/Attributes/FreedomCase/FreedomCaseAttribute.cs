using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GPC.Model.FreedomCases;

namespace GPC.Model.FEM.Attributes
{
    public abstract class FreedomCaseAttribute : Attribute, ISerializable
    {
        private FreedomCase _freedomCase;

        public FreedomCase FreedomCase => _freedomCase;

        public FreedomCaseAttribute(FreedomCase freedomCase)
            : this(freedomCase, string.Empty, Guid.NewGuid())
        {

        }

        public FreedomCaseAttribute(FreedomCase freedomCase, string name)
            : this(freedomCase, name, Guid.NewGuid())
        {

        }

        public FreedomCaseAttribute(FreedomCase freedomCase, string name, Guid guid)
            : base(guid, name)
        {
            _freedomCase = freedomCase ?? throw new ArgumentNullException(nameof(freedomCase));
        }

        protected FreedomCaseAttribute(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _freedomCase = (FreedomCase)info.GetValue("FreedomCase", typeof(FreedomCase));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            FreedomCaseAttribute lca = obj as FreedomCaseAttribute;

            return !(lca is null) && _freedomCase.Equals(lca._freedomCase) && base.Equals(lca);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("FreedomCase", _freedomCase);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<FreedomCase>.Default.GetHashCode(_freedomCase);
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