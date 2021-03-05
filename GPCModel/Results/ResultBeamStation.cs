using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]

    public class ResultBeamStation : ResultPoint, ISerializable
    {

        private double _distanceFromStartPoint;



        public ResultBeamStation(int id, string name, double distanceFromStartPoint) 
            : base(id, name)
        {
            this._distanceFromStartPoint = distanceFromStartPoint;
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
            int hashCode = 721521037;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + _distanceFromStartPoint.GetHashCode();
            return hashCode;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            throw new NotImplementedException();
        }
    }


}
