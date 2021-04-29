using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GPC.Model.LoadCases
{
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class LoadCaseBase : ModelObject, ISerializable, ILoadCase
    {
        #region PUBLIC CONSTRUCTOR

        public LoadCaseBase(string name)
            : this(name, Guid.NewGuid())
        {            
        }

        public LoadCaseBase(string name, Guid guid)
            : base(guid, name)
        {
            if (String.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Loadcase name cannot be empty");
        }


        public LoadCaseBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion 

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            LoadCaseBase objCasted = obj as LoadCaseBase;
            return !(objCasted is null) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(LoadCaseBase obj1, LoadCaseBase obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(LoadCaseBase obj1, LoadCaseBase obj2)
        {
            return !(obj1 == obj2);
        }

        private string GetDebuggerDisplay()
        {
            return $"LoadCase {Name}";
        }
    }
}
