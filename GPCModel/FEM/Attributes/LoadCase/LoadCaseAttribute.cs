using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.FEM.Attributes
{
    [Serializable]
    public abstract class LoadCaseAttribute : Attribute, ISerializable
    {
        private readonly string _loadCaseName;

        public string LoadCaseName => _loadCaseName;

        public LoadCaseAttribute(string loadCaseName)
            : this(loadCaseName, string.Empty, Guid.NewGuid())
        {

        }

        public LoadCaseAttribute(string loadCaseName, string name) 
            : this(loadCaseName, name, Guid.NewGuid())
        {

        }

        public LoadCaseAttribute(string loadCaseName, string name, Guid guid) 
            : base(guid, name)
        {
            _loadCaseName = string.IsNullOrEmpty(loadCaseName) || string.IsNullOrWhiteSpace(loadCaseName) ? throw new ArgumentNullException() : loadCaseName;
        }

        public LoadCaseAttribute(LoadCaseAttribute loadCaseAttribute)
            : base(loadCaseAttribute.Guid, loadCaseAttribute.Name)
        {
            _loadCaseName = loadCaseAttribute.LoadCaseName;
        }



        protected LoadCaseAttribute(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _loadCaseName = (string)info.GetValue("LoadCaseName", typeof(string));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            LoadCaseAttribute lca = obj as LoadCaseAttribute;

            return !(obj is null) && _loadCaseName.Equals(lca._loadCaseName) && base.Equals(obj);
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LoadCaseName", _loadCaseName);
        }


        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<string>.Default.GetHashCode(_loadCaseName);
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