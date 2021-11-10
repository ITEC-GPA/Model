using System;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;

namespace GPC.Model.Results
{
    [Serializable]
    public class NodeResult : ElementResult, ISerializable, IEquatable<NodeResult>, IFemResult
    {
        private readonly INodeResult _result;

        private readonly int _stageId;




        public ResultType Result => (ResultType)_result;

        public int StageId => _stageId;



        public NodeResult(ILoadCase Case, CoordinateSystem coordinateSystem, INodeResult result, int stageId = ModelObjectId.IDUNASSIGNED)
            : base(Case, coordinateSystem)
        {
            _result = result ?? throw new ArgumentNullException(nameof(result));
            _stageId = stageId;
        }


        public NodeResult(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _result = (INodeResult)info.GetValue("Result", typeof(INodeResult));
            _stageId = (int)info.GetValue("StageId", typeof(int));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Result", _result, typeof(INodeResult));
            info.AddValue("StageId", _stageId, typeof(int));
        }



        #region Equals - hascode - operators

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _result.GetHashCode();
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
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return _result.Equals(other.Result) && _stageId.Equals(other.StageId) && base.Equals(other);
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