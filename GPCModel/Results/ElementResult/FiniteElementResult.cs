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

        private readonly ResultType[] _results;
        private readonly ResultLocation[] _points;

        protected int _stageId;
        public ResultLocation[] Points => _points;


        public FiniteElementResult(ILoadCase Case, CoordinateSystem coordinateSystem, IEnumerable<ResultType> result, IEnumerable<ResultLocation> points)
        public int StageId => _stageId;
        {

        }

        public FiniteElementResult(ILoadCase Case, CoordinateSystem coordinateSystem, IEnumerable<ResultLocation> points,
                                    int stageId = ModelObjectId.IDUNASSIGNED, string name = "")
            : base(Case, coordinateSystem, name)

            if (result.Count() != points.Count())
                throw new ArgumentException("Lists lenght are different");

            if (result.Where(i => i != null).Select(i => i.GetType()).Distinct().Count() > 1)
                throw new ArgumentException("Multiple result type");

            if (points.Where(i => i != null).Select(i => i.GetType()).Distinct().Count() > 1)
                throw new ArgumentException("Multiple location point type");


            _stageId = stageId;
            _points = points.ToArray();
        }

        public FiniteElementResult(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _results = (ResultType[])info.GetValue("ResultType", typeof(ResultType[]));
            _stageId = (int)info.GetValue("StageId", typeof(int));
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ResultType", _results);
            info.AddValue("StageId", _stageId);
        }

    }
}
