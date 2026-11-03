using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GPC.Model.LoadCases
{
    /// <summary>
    /// A load case: a named set of loads. Two load cases are equal if they have the same name
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class LoadCaseBase : ModelObject, ISerializable, ILoadCase
    {
        /// <summary>Explicit mass families participating in inertia actions. Null retains the historical solver default.
        /// Selection changes applied inertia loads, not the mass stored in the model.</summary>
        public InertiaMassSelection InertiaMasses { get; set; }
        #region PUBLIC CONSTRUCTOR

        /// <summary>Creates an input entity with the persistent identity supplied by its owner.</summary>
        public LoadCaseBase(Guid guid, string name) : this(name)
        { if (guid == Guid.Empty) throw new ArgumentException("EmptyEntityGuid", nameof(guid)); _guid = guid; }

        /// <summary>
        /// Creates a load case with a new Guid
        /// </summary>
        /// <param name="name">The name (not empty)</param>
        /// <exception cref="ArgumentException">If the name is null, empty or white space</exception>
        public LoadCaseBase(string name)
            : base(name)
        {
            if (String.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Loadcase name cannot be empty");
        }

        /// <summary>
        /// Deserialization constructor (see <see cref="ModelObject"/>)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected LoadCaseBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version=SerializationVersions.Read(info,"LoadCaseBaseVersion",1,0);
            if(version==1)InertiaMasses=(InertiaMassSelection)info.GetValue("InertiaMasses",typeof(InertiaMassSelection));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info,context);
            if(InertiaMasses!=null)
            {info.AddValue("LoadCaseBaseVersion",1);info.AddValue("InertiaMasses",InertiaMasses);}
        }

        #endregion

        #region Methods

        /// <summary>
        /// Equality of the names
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is a load case with the same name</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return (obj is LoadCaseBase objCasted) && base.Equals(objCasted);
        }

        /// <summary>
        /// The hash code of the name
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -391 + base.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null load cases are equal
        /// </summary>
        /// <param name="obj1">The first load case</param>
        /// <param name="obj2">The second load case</param>
        /// <returns>True if the load cases are equal</returns>
        public static bool operator ==(LoadCaseBase obj1, LoadCaseBase obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first load case</param>
        /// <param name="obj2">The second load case</param>
        /// <returns>True if the load cases are different</returns>
        public static bool operator !=(LoadCaseBase obj1, LoadCaseBase obj2)
        {
            return !(obj1 == obj2);
        }

        /// <summary>
        /// The text shown by the debugger
        /// </summary>
        /// <returns>"LoadCase " and the name</returns>
        private string GetDebuggerDisplay()
        {
            return $"LoadCase {Name}";
        }

        #endregion
    }
}
