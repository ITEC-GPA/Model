using GPC.Model.Results.ResultLocations;
using GPC.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Results.ElementResults
{
    [Serializable]
    public abstract class ElementResult : ModelObjectId, ISerializable
    {
        #region Variables

        protected List<ResultLocation> _resultLocations;
        protected int _stageID;

        #endregion

        #region Properties

        public int StageID { get => _stageID; set => _stageID = value; }

        public List<ResultLocation> Results { get => _resultLocations; set => _resultLocations = value; }

        #endregion

        #region Constructors

        protected ElementResult(List<ResultLocation> resultLocations, int stageId = IDUNASSIGNED, string name = "", int id = IDUNASSIGNED)
            : base(id, name)
        {
            _resultLocations = resultLocations;
            _stageID = stageId;
        }

        protected ElementResult(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _resultLocations = (List<ResultLocation>)info.GetValue("ResultLocations", typeof(List<ResultLocation>));
            _stageID = info.GetInt32("StageID");
        }

        #endregion

        #region Public Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("StageID", _stageID);
            info.AddValue("ResultLocations", _resultLocations);
        }

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

        public static bool operator ==(ElementResult obj1, ElementResult obj2)
        {
            if (obj1 is null)
                return obj2 is null;

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ElementResult obj1, ElementResult obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
