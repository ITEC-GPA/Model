using GPC.Model.Results.ResultLocations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Results.ElementResults
{
    [Serializable]
    public sealed class BrickElementResult : ElementResult, ISerializable, IEquatable<BrickElementResult>
    {
        #region Public Constructors

        public BrickElementResult(List<IBrickResultLocation> resultStation, int stageId = IDUNASSIGNED, string name = "", int id = IDUNASSIGNED)
            : base(resultStation.Cast<ResultLocation>().ToList(), stageId, name, id)
        {

        }

        private BrickElementResult(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Public Methods

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals((BrickElementResult)obj);
        }

        public bool Equals(BrickElementResult other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return base.Equals(other);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public static bool operator ==(BrickElementResult obj1, BrickElementResult obj2)
        {
            if (obj1 is null)
                return obj2 is null;

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(BrickElementResult obj1, BrickElementResult obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
