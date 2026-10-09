using GPC.Model.Results.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Results.Storage
{
    /// <summary>
    /// The results of a node for a load case or combination, at its locations
    /// </summary>
    [Serializable]
    public sealed class NodeResult : ElementResult, ISerializable, IEquatable<NodeResult>
    {
        #region Public Constructors

        /// <summary>
        /// Creates the results
        /// </summary>
        /// <param name="resultStation">The results at the locations</param>
        /// <param name="stageId">The id of the stage</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        public NodeResult(List<INodeResultLocation> resultStation, int stageId = IDUNASSIGNED, string name = "", int id = IDUNASSIGNED)
            : base(resultStation.Cast<ResultLocation>().ToList(), stageId, name, id)
        {

        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private NodeResult(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The hash code (see <see cref="ElementResult.GetHashCode"/>)
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Equality with another result of the same type (an object of another type throws <see cref="InvalidCastException"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if the results are equal</returns>
        public override bool Equals(object obj)
        {
            return Equals((NodeResult)obj);
        }

        /// <summary>
        /// Equality of the results (see <see cref="ElementResult.Equals(object)"/>)
        /// </summary>
        /// <param name="other">The results to compare</param>
        /// <returns>True if the results are equal</returns>
        public bool Equals(NodeResult other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return base.Equals(other);
        }

        /// <summary>
        /// Serializes the results
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(NodeResult)"/>)
        /// </summary>
        /// <param name="obj1">The first results</param>
        /// <param name="obj2">The second results</param>
        /// <returns>True if the results are equal</returns>
        public static bool operator ==(NodeResult obj1, NodeResult obj2)
        {
            if (obj1 is null)
                return obj2 is null;

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(NodeResult)"/>)
        /// </summary>
        /// <param name="obj1">The first results</param>
        /// <param name="obj2">The second results</param>
        /// <returns>True if the results are different</returns>
        public static bool operator !=(NodeResult obj1, NodeResult obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
