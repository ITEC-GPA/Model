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

        private readonly IEnumerable<ResultType> _results;
        private readonly IEnumerable<ResultLocationId> _points;

        public IEnumerable<ResultType> Results => _results;
        public IEnumerable<ResultLocationId> Points => _points;


        public FiniteElementResult(ILoadCase Case, CoordinateSystem coordinateSystem, IEnumerable<ResultType> result, IEnumerable<ResultLocationId> points)
            : base(Case, coordinateSystem)
        {

            if (result.Count() != points.Count())
                throw new ArgumentException("Lists lenght are different");

            if (result.Select(i => i.GetType()).Distinct().Count() > 1)
                throw new ArgumentException("Multiple result type");

            if (points.Select(i => i.GetType()).Distinct().Count() > 1)
                throw new ArgumentException("Multiple location point type");


            _results = result;
            _points = points;
        }


    }
}
