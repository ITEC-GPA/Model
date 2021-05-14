using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public class ResultBeamStation : ResultPoint, ISerializable
    {
        private readonly double _distanceFromStartPoint;
        private readonly double _beamLenght;


        public double BeamLenght => _beamLenght;
        public double DistanceFromStartPoint => _distanceFromStartPoint;

        public double ParametricDistance => _distanceFromStartPoint / _beamLenght;



        public ResultBeamStation(int id, double distanceFromStartPoint, double beamLenght)
            : base(id, string.Empty)
        {
            _distanceFromStartPoint = distanceFromStartPoint;
            _beamLenght = beamLenght;
        }


        public ResultBeamStation(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            return obj is ResultBeamStation station &&
                   base.Equals(obj) &&
                   _distanceFromStartPoint == station._distanceFromStartPoint;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 721521037;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + _distanceFromStartPoint.GetHashCode();
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