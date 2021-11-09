using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{

    [Serializable]
    public class ResultStation : ResultLocation, ISerializable, IResultLocation
    {
        private readonly double _distanceFromStartPoint;
        private readonly double _elementLenght;


        public double ElementLenght => _elementLenght;

        public double DistanceFromStartPoint => _distanceFromStartPoint;

        public double ParametricDistance => _distanceFromStartPoint / _elementLenght;



        public ResultStation(IEnumerable<ResultType> results, double distanceFromStartPoint, double elementLenght, int id = ModelObjectId.IDUNASSIGNED)
            : base(results.ToArray(), id)
        {
            _distanceFromStartPoint = distanceFromStartPoint > elementLenght ? throw new ArgumentException($"distanceFromStartPoint can not higher than elementLenght") : distanceFromStartPoint;
            _elementLenght = elementLenght == 0 ? throw new ArgumentException($"elementLenght can not be zero") : elementLenght;
        }


        public ResultStation(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            throw new NotImplementedException();
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            return obj is ResultStation station &&
                   base.Equals(obj) &&
                   _distanceFromStartPoint == station._distanceFromStartPoint &&
                   _elementLenght == station._elementLenght;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 721521037;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + _distanceFromStartPoint.GetHashCode();
                hashCode = hashCode * -1521134295 + _elementLenght.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ResultStation obj1, ResultStation obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ResultStation obj1, ResultStation obj2)
        {
            return !(obj1 == obj2);
        }
    }
}