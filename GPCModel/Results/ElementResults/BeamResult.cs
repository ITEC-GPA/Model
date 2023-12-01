using GPC.Model.Results.ResultLocations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Results.ElementResults
{
    [Serializable]
    public sealed class BeamResult : ElementResult, ISerializable, IEquatable<BeamResult>
    {
        #region Public Constructors

        public BeamResult(List<IBeamResultLocation> resultStation, int stageId = IDUNASSIGNED, string name = "", int id = IDUNASSIGNED)
            : base(resultStation.Cast<ResultLocation>().ToList(), stageId, name, id)
        {

        }

        private BeamResult(SerializationInfo info, StreamingContext context)
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
            return Equals((BeamResult)obj);
        }

        public bool Equals(BeamResult other)
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

        public static bool operator ==(BeamResult obj1, BeamResult obj2)
        {
            if (obj1 is null)
                return obj2 is null;

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(BeamResult obj1, BeamResult obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
