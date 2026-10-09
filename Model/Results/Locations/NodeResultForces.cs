using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results.ResultLocations
{
    /// <summary>
    /// The forces of a node (e.g. reactions)
    /// </summary>
    [Serializable]
    public class NodeResultForces : ResultLocation, ISerializable, INodeResultLocation
    {
        #region Properties

        /// <summary>
        /// The forces
        /// </summary>
        public ResultBeamForces ResultBeamForces { get => (ResultBeamForces)_resultTypes; set => _resultTypes = value; }
        // Compatibility view: x/y/z mean axes V1/V2/V3 of the result frame, not necessarily global XYZ.
        // State.Components follows Fx,Fy,Fz,Mx,My,Mz for nodal samples.
        public double Fx => ResultBeamForces.V1;
        public double Fy => ResultBeamForces.V2;
        public double Fz => ResultBeamForces.N;
        public double Mx => ResultBeamForces.M1;
        public double My => ResultBeamForces.M2;
        public double Mz => ResultBeamForces.T;
        public PostProcessing.NodalForceKind Kind { get; set; }
        public PostProcessing.ActionBody Body { get; set; }
        public int? OwnerElementId { get; set; }
        public PostProcessing.EntityFamily? OwnerElementFamily { get; set; }
        public string ElementEnd { get; set; }
        public string AggregationSet { get; set; }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates the result
        /// </summary>
        /// <param name="loadCase">The load case or combination</param>
        /// <param name="results">The forces</param>
        /// <param name="id">The id</param>
        /// <param name="name">The name</param>
        public NodeResultForces(ILoadCase loadCase, ResultBeamForces results, int id = ModelObjectId.IDUNASSIGNED, string name = "")
            : base(loadCase, results, id, name)
        {

        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected NodeResultForces(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Kind = SerializationFields.Read<PostProcessing.NodalForceKind>(info, "Kind");
            Body = SerializationFields.Read<PostProcessing.ActionBody>(info, "Body");
            OwnerElementId = SerializationFields.Read<int?>(info, "OwnerElementId");
            OwnerElementFamily = SerializationFields.Read<PostProcessing.EntityFamily?>(info, "OwnerElementFamily");
            ElementEnd = SerializationFields.Read<string>(info, "ElementEnd");
            AggregationSet = SerializationFields.Read<string>(info, "AggregationSet");
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
            info.AddValue("Kind", Kind); info.AddValue("Body", Body); info.AddValue("OwnerElementId", OwnerElementId);
            info.AddValue("OwnerElementFamily", OwnerElementFamily);
            info.AddValue("ElementEnd", ElementEnd); info.AddValue("AggregationSet", AggregationSet);
        }

        /// <summary>
        /// Equality with another result of the same type (an object of another type throws <see cref="InvalidCastException"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if the results are equal</returns>
        public override bool Equals(object obj)
        {
            return Equals((NodeResultForces)obj);
        }

        /// <summary>
        /// Equality of the result values and of the name (including the load case and analysis state)
        /// </summary>
        /// <param name="other">The result to compare</param>
        /// <returns>True if the results are equal</returns>
        public bool Equals(NodeResultForces other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return base.Equals(other) && Kind == other.Kind && Body == other.Body && OwnerElementId == other.OwnerElementId
                && OwnerElementFamily == other.OwnerElementFamily && ElementEnd == other.ElementEnd && AggregationSet == other.AggregationSet;
        }

        /// <summary>
        /// The hash code of the base
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -19 + base.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(NodeResultForces)"/>)
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>True if the results are equal</returns>
        public static bool operator ==(NodeResultForces obj1, NodeResultForces obj2)
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
        /// Inequality operator (see <see cref="Equals(NodeResultForces)"/>)
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>True if the results are different</returns>
        public static bool operator !=(NodeResultForces obj1, NodeResultForces obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
