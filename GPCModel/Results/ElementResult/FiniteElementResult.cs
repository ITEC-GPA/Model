using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public abstract class FiniteElementResult : ElementResult, ISerializable
    {

        private readonly ResultType[] _results;
        private readonly ResultLocationId[] _points;

        public ResultType[] Results => _results;
        public ResultLocationId[] Points => _points;


        public FiniteElementResult(ILoadCase Case, CoordinateSystem coordinateSystem, IEnumerable<ResultType> result, IEnumerable<ResultLocationId> points)
            : this(Case, coordinateSystem, result, points, ModelObjectId.IDUNASSIGNED)
        {

        }

        public FiniteElementResult(ILoadCase Case, CoordinateSystem coordinateSystem, IEnumerable<ResultType> result, IEnumerable<ResultLocationId> points, int stageId, string name = "")
            : base(Case, coordinateSystem, stageId, name)
        {

            if (result.Count() != points.Count())
                throw new ArgumentException("Lists lenght are different");

            if (result.Where(i => i != null).Select(i => i.GetType()).Distinct().Count() > 1)
                throw new ArgumentException("Multiple result type");

            if (points.Where(i => i != null).Select(i => i.GetType()).Distinct().Count() > 1)
                throw new ArgumentException("Multiple location point type");


            _results = result.ToArray();
            _points = points.ToArray();
        }

        public FiniteElementResult(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _results = (ResultType[])info.GetValue("ResultType", typeof(ResultType[]));
            _points = (ResultLocationId[])info.GetValue("ResultLocationId", typeof(ResultLocationId[]));
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ResultType", _results);
            info.AddValue("ResultLocationId", _points);
        }

    }
}
