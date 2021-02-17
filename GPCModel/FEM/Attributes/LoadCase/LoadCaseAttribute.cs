using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.FEM.Attributes
{
    [Serializable]
    public abstract class LoadCaseAttribute : Attribute, ISerializable
    {
        private LoadCase _loadCase;

        public LoadCase LoadCase => _loadCase;

        public LoadCaseAttribute(LoadCase loadCase)
            : this(loadCase, string.Empty, Guid.NewGuid())
        {

        }

        public LoadCaseAttribute(LoadCase loadCase, string name) 
            : this(loadCase, name, Guid.NewGuid())
        {

        }

        public LoadCaseAttribute(LoadCase loadCase, string name, Guid guid) 
            : base(guid, name)
        {
            _loadCase = loadCase ?? throw new ArgumentNullException("Loadcase cannot be null");
        }

        protected LoadCaseAttribute(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _loadCase = (LoadCase)info.GetValue("LoadCase", typeof(LoadCase));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            LoadCaseAttribute lca = obj as LoadCaseAttribute;

            return !(obj is null) && _loadCase.Equals(lca._loadCase) && base.Equals(obj);
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LoadCase", _loadCase);
        }


        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<LoadCase>.Default.GetHashCode(_loadCase);
            return hashCode;
        }



        #region Override Operator
        public static bool operator ==(LoadCaseAttribute obj1, LoadCaseAttribute obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(LoadCaseAttribute obj1, LoadCaseAttribute obj2)
        {
            return !(obj1 == obj2);
        }
        #endregion
    }
}