using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results.ResultLocations
{
    /// <summary>
    /// A result (values of a <see cref="ResultType"/>) of a load case or combination at a location of an element
    /// </summary>
    [Serializable]
    public abstract class ResultLocation : ModelObjectId, ISerializable
    {
        #region Variables

        /// <summary>
        /// The result values
        /// </summary>
        protected ResultType _resultTypes;
        /// <summary>
        /// The load case or combination
        /// </summary>
        protected ILoadCase _case;
        public GPC.Model.PostProcessing.ResultState State { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// The load case or combination
        /// </summary>
        public ILoadCase Case => _case;

        /// <summary>
        /// The result values
        /// </summary>
        public ResultType ResultTypes => _resultTypes;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates the result
        /// </summary>
        /// <param name="loadcase">The load case or combination</param>
        /// <param name="results">The result values</param>
        /// <param name="id">The id</param>
        /// <param name="name">The name</param>
        protected ResultLocation(ILoadCase loadcase, ResultType results, int id, string name)
            : base(id, name)
        {
            _case = loadcase;
            _resultTypes = results;
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected ResultLocation(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _resultTypes = (ResultType)info.GetValue("ResultType", typeof(ResultType));
            State = SerializationFields.Read<GPC.Model.PostProcessing.ResultState>(info, "State");
            _case = (ILoadCase)info.GetValue("ILoadCase", typeof(ILoadCase));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Serializes the result
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("State", State);
            info.AddValue("ResultType", _resultTypes, typeof(ResultType));
            info.AddValue("ILoadCase", _case, typeof(ILoadCase));
        }

        #endregion

        #region  Equals - hashcode - Operators

        /// <summary>
        /// Equality of the result values and of the name (including the load case and analysis state)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal result</returns>
        public override bool Equals(object obj)
        {

            return (obj is ResultLocation resultLocation) &&
                GetType() == resultLocation.GetType() &&
                object.Equals(_case, resultLocation._case) &&
                SameState(State, resultLocation.State) &&
                object.Equals(_resultTypes, resultLocation._resultTypes) &&
                base.Equals(resultLocation);
        }

        /// <summary>
        /// The hash code of the name and of the result values
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -19 + base.GetHashCode();
                hashCode = hashCode * -17 * (_resultTypes?.GetHashCode() ?? 0);
                hashCode = hashCode * -17 + (_case?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        private static bool SameState(PostProcessing.ResultState left, PostProcessing.ResultState right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left == null || right == null) return false;
            return left.DatasetId == right.DatasetId && left.ModelRevision == right.ModelRevision && left.Phase == right.Phase
                && left.Step == right.Step && left.ConcomitantStateId == right.ConcomitantStateId && left.Semantics == right.Semantics
                && left.MovingLoadPosition == right.MovingLoadPosition && left.Mode == right.Mode && left.Normalization == right.Normalization
                && left.InputFingerprint == right.InputFingerprint && left.IsCombined == right.IsCombined && left.IsCumulative == right.IsCumulative
                && (ReferenceEquals(left.Components, right.Components) || (left.Components != null && right.Components != null && System.Linq.Enumerable.SequenceEqual(left.Components, right.Components)));
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>True if the results are equal</returns>
        public static bool operator ==(ResultLocation obj1, ResultLocation obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>True if the results are different</returns>
        public static bool operator !=(ResultLocation obj1, ResultLocation obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
