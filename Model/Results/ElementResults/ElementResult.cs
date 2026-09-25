using GPC.Model.Results.ResultLocations;
using GPC.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Results.ElementResults
{
    /// <summary>
    /// The results of an element for a load case or combination, at its result locations
    /// </summary>
    [Serializable]
    public abstract class ElementResult : ModelObjectId, ISerializable
    {
        #region Variables

        /// <summary>
        /// The results at the locations of the element
        /// </summary>
        protected List<ResultLocation> _resultLocations;
        /// <summary>
        /// The id of the stage
        /// </summary>
        protected int _stageID;

        #endregion

        #region Properties

        /// <summary>
        /// The id of the stage
        /// </summary>
        public int StageID { get => _stageID; set => _stageID = value; }

        /// <summary>
        /// The results at the locations of the element
        /// </summary>
        public List<ResultLocation> Results { get => _resultLocations; set => _resultLocations = value; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates the results
        /// </summary>
        /// <param name="resultLocations">The results at the locations</param>
        /// <param name="stageId">The id of the stage</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        protected ElementResult(List<ResultLocation> resultLocations, int stageId = IDUNASSIGNED, string name = "", int id = IDUNASSIGNED)
            : base(id, name)
        {
            _resultLocations = resultLocations;
            _stageID = stageId;
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected ElementResult(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _resultLocations = (List<ResultLocation>)info.GetValue("ResultLocations", typeof(List<ResultLocation>));
            _stageID = info.GetInt32("StageID");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Serializes the results
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("StageID", _stageID);
            info.AddValue("ResultLocations", _resultLocations);
        }

        /// <summary>
        /// Equality of the stage id, of the results (in any order) and of the name
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> has equal results</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return (obj is ElementResult other) &&
                _stageID.Equals(other._stageID) &&
                _resultLocations.ScrambledEquals(other.Results) &&
                base.Equals(other);
        }

        /// <summary>
        /// The hash code of the name and of the results (in their order: two equal objects with the results in a different order have different
        /// hash codes)
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -391 + base.GetHashCode();
                for (int i = 0; i < _resultLocations.Count; i++)
                    hashCode = hashCode * -17 + _resultLocations[i].GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first results</param>
        /// <param name="obj2">The second results</param>
        /// <returns>True if the results are equal</returns>
        public static bool operator ==(ElementResult obj1, ElementResult obj2)
        {
            if (obj1 is null)
                return obj2 is null;

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first results</param>
        /// <param name="obj2">The second results</param>
        /// <returns>True if the results are different</returns>
        public static bool operator !=(ElementResult obj1, ElementResult obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
