using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;

namespace GPC.Model.Results
{
    [Serializable]
    public sealed class NodeResult : ElementResult, ISerializable, IEquatable<NodeResult>, IFemResult
    {
        #region Variables

        private readonly int _stageId;

        #endregion

        #region Properties

        public int StageId => _stageId;

        #endregion

        #region Public Constructors

        public NodeResult(ILoadCase Case, IEnumerable<ResultLocationId> resultLocations, int stageId = ModelObjectId.IDUNASSIGNED)
            : base(Case, resultLocations.ToArray())
        {
            _stageId = stageId;
        }

        private NodeResult(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _stageId = (int)info.GetValue("StageId", typeof(int));
        }

        #endregion

        #region Public Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("StageId", _stageId, typeof(int));
        }

		#endregion

		#region Equals - hascode - operators

		public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _stageId.GetHashCode();

                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals((NodeResult)obj);
        }

        public bool Equals(NodeResult other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return _stageId.Equals(other.StageId) && base.Equals(other);
        }

        public static bool operator ==(NodeResult obj1, NodeResult obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(NodeResult obj1, NodeResult obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}