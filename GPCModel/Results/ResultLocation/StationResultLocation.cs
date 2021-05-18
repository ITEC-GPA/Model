using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{

    [Serializable]
    public class StationResultLocation : ResultLocationId, ISerializable
    {
        private readonly double _distanceFromStartPoint;
        private readonly double _elementLenght;


        public double ElementLenght => _elementLenght;

        public double DistanceFromStartPoint => _distanceFromStartPoint;

        public double ParametricDistance => _distanceFromStartPoint / _elementLenght;



        public StationResultLocation(int id, double distanceFromStartPoint, double elementLenght)
            : base(id, string.Empty)
        {
            _distanceFromStartPoint = distanceFromStartPoint > elementLenght ? throw new ArgumentException($"distanceFromStartPoint can not higher than elementLenght") : distanceFromStartPoint;
            _elementLenght = elementLenght == 0 ? throw new ArgumentException($"elementLenght can not be zero") : elementLenght;
        }


        public StationResultLocation(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            return obj is StationResultLocation station &&
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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            throw new NotImplementedException();
        }
    }
}