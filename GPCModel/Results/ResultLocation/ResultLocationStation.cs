using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{

    [Serializable]
    public class ResultLocationStation : ResultLocation, ISerializable
    {
        private readonly double _distanceFromStartPoint;
        private readonly double _elementLenght;


        public double ElementLenght => _elementLenght;

        public double DistanceFromStartPoint => _distanceFromStartPoint;

        public double ParametricDistance => _distanceFromStartPoint / _elementLenght;



        public ResultLocationStation(IEnumerable<IBeamResult> results, double distanceFromStartPoint, double elementLenght, int id = ModelObjectId.IDUNASSIGNED)
            : base(results.Cast<ResultType>().ToArray(), id)
        {
            _distanceFromStartPoint = distanceFromStartPoint > elementLenght ? throw new ArgumentException($"distanceFromStartPoint can not higher than elementLenght") : distanceFromStartPoint;
            _elementLenght = elementLenght == 0 ? throw new ArgumentException($"elementLenght can not be zero") : elementLenght;
        }


        public ResultLocationStation(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _distanceFromStartPoint = (double)info.GetValue("DistanceFromStartPoint", typeof(double));
            _elementLenght = (double)info.GetValue("ElementLenght", typeof(double));
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("DistanceFromStartPoint", _distanceFromStartPoint);
            info.AddValue("ElementLenght", _elementLenght);
        }

        public override bool Equals(object obj)
        {
            return obj is ResultLocationStation station &&
                   base.Equals(obj) &&
                   _distanceFromStartPoint == station._distanceFromStartPoint &&
                   _elementLenght == station._elementLenght;
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
    }
}