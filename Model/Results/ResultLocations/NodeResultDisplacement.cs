using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results.ResultLocations
{
    [Serializable]
    public class NodeResultDisplacement : ResultLocation, ISerializable, INodeResultLocation
    {
        #region Properties

        public ResultDisplacement ResultDisplacement { get => (ResultDisplacement)_resultTypes; set => _resultTypes = value; }

        #endregion

        #region Public Constructors

        public NodeResultDisplacement(ILoadCase loadCase, ResultDisplacement results, int id = ModelObjectId.IDUNASSIGNED, string name = "")
            : base(loadCase, results, id, name)
        {

        }

        protected NodeResultDisplacement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #endregion

        #region Public Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public override bool Equals(object obj)
        {
            return Equals((NodeResultDisplacement)obj);
        }

        public bool Equals(NodeResultDisplacement other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return base.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -19 + base.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(NodeResultDisplacement obj1, NodeResultDisplacement obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(NodeResultDisplacement obj1, NodeResultDisplacement obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}