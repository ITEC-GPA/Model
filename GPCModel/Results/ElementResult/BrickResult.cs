using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public class BrickResult : FiniteElementResult, ISerializable, IEquatable<BrickResult>
    {

        public BrickResult(ILoadCase Case, CoordinateSystem coordinateSystem, IEnumerable<IBrickResult> result, IEnumerable<ResultLocation> points)
            : this(Case, coordinateSystem, result, points, ModelObjectId.IDUNASSIGNED)
        {

        }

        public BrickResult(ILoadCase Case, CoordinateSystem coordinateSystem, IEnumerable<IBrickResult> result, IEnumerable<ResultLocation> points, int stageId)
            : base(Case, coordinateSystem, (IEnumerable<ResultType>)result, points, stageId)
        {

        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals((BrickResult)obj);
        }

        public bool Equals(BrickResult other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return base.Equals(other);
        }

        public static bool operator ==(BrickResult obj1, BrickResult obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(BrickResult obj1, BrickResult obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
