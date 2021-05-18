using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public sealed class PlateResult : FiniteElementResult, ISerializable, IEquatable<PlateResult>
    {

        public PlateResult(ILoadCase Case, CoordinateSystem coordinateSystem, IEnumerable<IPlateResult> result, IEnumerable<ResultLocationId> points)
            : base(Case, coordinateSystem, (IEnumerable<ResultType>)result, points)
        {

        }


        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals((PlateResult)obj);
        }

        public bool Equals(PlateResult other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return base.Equals(other);
        }

        public static bool operator ==(PlateResult obj1, PlateResult obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(PlateResult obj1, PlateResult obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
