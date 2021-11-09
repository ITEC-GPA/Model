using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;

namespace GPC.Model.Results
{
    [Serializable]
    public abstract class FiniteElementResult : ElementResult, ISerializable, IFemResult
    {

        private readonly ResultLocation[] _resultLocations;

        protected int _stageId;


        public ResultLocation[] ResultLocations => _resultLocations;
        public int StageId => _stageId;


        public FiniteElementResult(ILoadCase Case, CoordinateSystem coordinateSystem, IEnumerable<ResultLocation> points,
                                    int stageId = ModelObjectId.IDUNASSIGNED, string name = "")
            : base(Case, coordinateSystem, name)
        {

            if (points.Where(i => i != null).Select(i => i.GetType()).Distinct().Count() > 1)
                throw new ArgumentException("Multiple location type");

            if (points.Select(i => i.GetResultsEnumerator()).Cast<ResultType>().Where(i => i != null).Select(i => i.GetType()).Distinct().Count() > 1)
                throw new ArgumentException("Multiple result type");


            if (points.Where(i => i != null).Select(i => i.GetType()).Distinct().Count() > 1)
                throw new ArgumentException("Multiple location point type");

            _resultLocations = points.ToArray();

            _stageId = stageId;
        }

        public FiniteElementResult(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _resultLocations = (ResultLocation[])info.GetValue("ResultLocationId", typeof(ResultLocation[]));
            _stageId = (int)info.GetValue("StageId", typeof(int));
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ResultLocationId", _resultLocations);
            info.AddValue("StageId", _stageId);
        }



        public IEnumerator GetResultsEnumerator()
        {
            return _resultLocations.GetEnumerator();
        }


        #region Equals - hashcode


        public override bool Equals(object obj)
        {
            return obj is FiniteElementResult result &&
                   base.Equals(obj) &&
                   EqualityComparer<ResultLocation[]>.Default.Equals(_resultLocations, result._resultLocations) &&
                   _stageId == result._stageId;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -19 + base.GetHashCode();

                for (int i = 0; i < _resultLocations.Length; i++)
                {
                    hashCode = hashCode * -17 * _resultLocations[i].GetHashCode();
                }

                hashCode = hashCode * -19 + _stageId.GetHashCode();
                return hashCode;
            }
        }
        #endregion
    }
}
