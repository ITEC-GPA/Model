using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results.ResultLocations
{
    [Serializable]
    public abstract class ResultLocation : ModelObjectId, ISerializable
    {
        #region Variables

        protected ResultType _resultTypes;
        protected ILoadCase _case;

        #endregion

        #region Properties

        public ILoadCase Case => _case;

        public ResultType ResultTypes => _resultTypes;

        #endregion

        #region Public Constructors

        protected ResultLocation(ILoadCase loadcase, ResultType results, int id, string name)
            : base(id, name)
        {
            _case = loadcase;
            _resultTypes = results;
        }

        protected ResultLocation(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _resultTypes = (ResultType)info.GetValue("ResultType", typeof(ResultType));
            _case = (ILoadCase)info.GetValue("ILoadCase", typeof(ILoadCase));
        }

        #endregion

        #region Public Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ResultType", _resultTypes, typeof(ResultType));
            info.AddValue("ILoadCase", _case, typeof(ILoadCase));
        }

        #endregion

        #region  Equals - hashcode - Operators

        public override bool Equals(object obj)
        {

            return (obj is ResultLocation resultLocation) &&
                _resultTypes.Equals(resultLocation._resultTypes) &&
                base.Equals(resultLocation);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -19 + base.GetHashCode();
                hashCode = hashCode * -17 * _resultTypes.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ResultLocation obj1, ResultLocation obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ResultLocation obj1, ResultLocation obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}