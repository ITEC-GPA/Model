using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public class NodeResult : ElementResult, ISerializable, IEquatable<NodeResult>
    {
        private readonly INodeResult _result;

        public ResultType Result => (ResultType)_result;


        public NodeResult(ILoadCase Case, CoordinateSystem coordinateSystem, INodeResult result)
            : this(Case, coordinateSystem, result, ModelObjectId.IDUNASSIGNED)
        {

        }

        public NodeResult(ILoadCase Case, CoordinateSystem coordinateSystem, INodeResult result, int stageId)
            : base(Case, coordinateSystem)
        {
            _result = result;
            _stageId = stageId;
        }



        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _result.GetHashCode();

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

            return _result.Equals(other.Result) && base.Equals(other);
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
    }
}