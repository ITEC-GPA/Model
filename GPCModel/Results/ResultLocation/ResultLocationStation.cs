using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public class ResultLocationStation : ResultLocation, ISerializable
    {
        #region Variables

        private readonly double _distanceFromStartPoint;
        private readonly double _elementLenght;

        #endregion

        #region Properties

        public double ElementLenght => _elementLenght;

        public double DistanceFromStartPoint => _distanceFromStartPoint;

        public double ParametricDistance => _distanceFromStartPoint / _elementLenght;

        #endregion

        #region Public Constructors

        public ResultLocationStation(IEnumerable<IBeamResult> results, double distanceFromStartPoint, double elementLenght, int id = ModelObjectId.IDUNASSIGNED)
            : base(results.Cast<ResultType>().ToArray(), id)
        {
            _distanceFromStartPoint = distanceFromStartPoint > elementLenght ? throw new ArgumentException($"distanceFromStartPoint can not higher than elementLenght") : distanceFromStartPoint;
            _elementLenght = elementLenght == 0 ? throw new ArgumentException($"elementLenght can not be zero") : elementLenght;
        }

        protected ResultLocationStation(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _distanceFromStartPoint = (double)info.GetValue("DistanceFromStartPoint", typeof(double));
            _elementLenght = (double)info.GetValue("ElementLenght", typeof(double));
        }

        #endregion

        #region Public Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("DistanceFromStartPoint", _distanceFromStartPoint);
            info.AddValue("ElementLenght", _elementLenght);
        }

        public override bool Equals(object obj)
        {
            return Equals((ResultLocationStation)obj);
        }

        public bool Equals(ResultLocationStation other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return base.Equals(other) &&
                   _distanceFromStartPoint == other._distanceFromStartPoint &&
                   _elementLenght == other._elementLenght;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -19 + base.GetHashCode();
                hashCode = hashCode * -19 + _distanceFromStartPoint.GetHashCode();
                hashCode = hashCode * -19 + _elementLenght.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ResultLocationStation obj1, ResultLocationStation obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ResultLocationStation obj1, ResultLocationStation obj2)
        {
            return !(obj1 == obj2);
        }

		#endregion
	}
}