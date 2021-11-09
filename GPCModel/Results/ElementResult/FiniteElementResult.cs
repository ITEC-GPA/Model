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

        protected readonly ResultLocation[] _resultLocations;

        protected int _stageId;

        /// <summary>
        /// Return a clone of the results
        /// </summary>
        public ResultLocation[] ResultLocations => (ResultLocation[])_resultLocations.Clone();

        public int StageId => _stageId;


        public FiniteElementResult(ILoadCase Case, CoordinateSystem coordinateSystem, IEnumerable<ResultLocation> resultLocation,
                                    int stageId = ModelObjectId.IDUNASSIGNED, string name = "")
            : base(Case, coordinateSystem, name)
        {

            if (resultLocation.Where(i => i != null).Select(i => i.GetType()).Distinct().Count() > 1)
                throw new ArgumentException("Multiple location type");

            if (resultLocation.Select(i => i.GetResultsEnumerator()).Cast<ResultType>().Where(i => i != null).Select(i => i.GetType()).Distinct().Count() > 1)
                throw new ArgumentException("Multiple result type");


            if (resultLocation.Where(i => i != null).Select(i => i.GetType()).Distinct().Count() > 1)
                throw new ArgumentException("Multiple location point type");

            _resultLocations = resultLocation.ToArray();

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
            info.AddValue("ResultLocationId", _resultLocations, typeof(ResultLocation[]));
            info.AddValue("StageId", _stageId, typeof(ResultLocation[]));
        }


        internal ResultLocation[] GetResults()
        {
            return _resultLocations;
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
