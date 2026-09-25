using GPC.Model.Collections;
using GPC.Model.Combinations;
using GPC.Model.LoadCases;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
    #region Public Enum        

    /// <summary>
    /// The group of a standard
    /// </summary>
    public enum StandardGroupType
    {
        /// <summary>
        /// European standards (Eurocodes and national annexes)
        /// </summary>
        European,
        /// <summary>
        /// American standards
        /// </summary>
        American,
        /// <summary>
        /// Hong Kong standards
        /// </summary>
        HongKong
    }

    #endregion

    /// <summary>
    /// A design standard: name, remarks and group; the derived classes collect its coefficients
    /// </summary>
    public abstract class Standard : ModelObject
    {
        /// <summary>
        /// The remarks
        /// </summary>
        protected string _remarks;

        /// <summary>
        /// The remarks (e.g. the reference of the standard)
        /// </summary>
        public string Remarks
        {
            get => _remarks;
            set => _remarks = value;
        }

        /// <summary>
        /// Distinction between standards.
        /// Influences the selection of standards, materials, and checks.
        /// </summary>
        public abstract StandardGroupType StandardGroup { get; }

        /// <summary>
        /// Creates the standard
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="remarks">The remarks</param>
        public Standard(string name = "", string remarks = "")
            : base(name)
        {
            _remarks = remarks;
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected Standard(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _remarks = info.GetString("Remarks");
        }


        /// <summary>
        /// Sets the name (a null name is ignored)
        /// </summary>
        /// <param name="name">The name</param>
        public void SetName(string name)
        {
            if (name != null)
                _name = name;
        }

        /// <summary>
        /// Serializes the object
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Remarks", _remarks);
        }

        /// <summary>
        /// Equality with an object of the same type
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is equal</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return obj is Standard standard &&
                _remarks.Equals(standard.Remarks) &&
                base.Equals(obj);
        }

        /// <summary>
        /// The hash code of the name (the remarks are not used)
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// A standard that generates the load combinations
        /// </summary>
        public interface ICombinationsGenerator
        {
            /// <summary>
            /// Get all the combinations of the loadCaseBase <paramref name="loadCases"/> with the options of generation <paramref name="options"/>
            /// </summary>
            /// <param name="prefix">The common prefix for each combination in the collection</param>
            /// <param name="loadCases">The array of load case base to combine</param>
            /// <param name="options">The options of combinations parameter</param>
            /// <returns>The Combination collections</returns>
            UniqueNameCollection<Combination> CreateCombinations(LoadCaseBase[] loadCases, CombinationsOptions options, string prefix = "cmb");
        }

        #region Nested Class

        /// <summary>
        /// The options of the generation of the combinations
        /// </summary>
        public abstract class CombinationsOptions
        {
            /// <summary>
            /// Equality of the options
            /// </summary>
            /// <param name="obj">The object to compare</param>
            /// <returns>True if <paramref name="obj"/> are equal options</returns>
            public override abstract bool Equals(object obj);

            /// <summary>
            /// The hash code of the options
            /// </summary>
            /// <returns>The hash code</returns>
            public override abstract int GetHashCode();
        }

        #endregion
    }
}
