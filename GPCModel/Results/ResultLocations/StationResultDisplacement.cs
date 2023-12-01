using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results.ResultLocations
{
    [Serializable]
    public class StationResultDisplacement : ResultLocation, ISerializable
    {
        #region Variables

        private double _parametricCoordinate;

        #endregion

        #region Properties

        public double ParametricDistance { get => _parametricCoordinate; set => _parametricCoordinate = value; }

        public ResultDisplacement ResultDisplacement { get => (ResultDisplacement)_resultTypes; set => _resultTypes = value; }

        #endregion

        #region Public Constructors

        public StationResultDisplacement(ILoadCase loadCase, ResultDisplacement results, double parametricCoordinate, int id = ModelObjectId.IDUNASSIGNED, string name = "")
            : base(loadCase, results, id, name)
        {
            if (parametricCoordinate < 0.0 || parametricCoordinate > 1.0)
                throw new ArgumentException($"distanceFromStartPoint can not lower than 0 or higher than 1");
            _parametricCoordinate = parametricCoordinate;
        }

        protected StationResultDisplacement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _parametricCoordinate = info.GetDouble("ParametricDistance");
        }

        #endregion

        #region Public Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("DistanceFromStartPoint", _parametricCoordinate);
        }

        public override bool Equals(object obj)
        {
            return Equals((StationResultDisplacement)obj);
        }

        public bool Equals(StationResultDisplacement other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return base.Equals(other) &&
                _parametricCoordinate == other._parametricCoordinate;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -19 + base.GetHashCode();
                hashCode = hashCode * -19 + _parametricCoordinate.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(StationResultDisplacement obj1, StationResultDisplacement obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(StationResultDisplacement obj1, StationResultDisplacement obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}