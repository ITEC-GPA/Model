using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;

namespace GPC.Model.Results
{
    [Serializable]
    public sealed class BeamResult : FiniteElementResult, ISerializable, IEquatable<BeamResult>, IElementResult
    {


        public double Length => ((ResultLocationStation)ResultLocations.First()).ElementLenght;     // TODO: va sistemato


        public BeamResult(ILoadCase Case, IEnumerable<ResultLocationStation> resultStation,
                            CoordinateSystem coordinateSystem, int stageId = ModelObjectId.IDUNASSIGNED)
            : base(Case, coordinateSystem, resultStation, stageId)
        {
            if (resultStation.Select(i => i.ElementLenght).Distinct().Count() > 1)
                throw new ArgumentException("All Result Station must have the same length");


        }


        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals((BeamResult)obj);
        }

        public bool Equals(BeamResult other)
        {

            if (ReferenceEquals(this, other))
                return true;

            return base.Equals(other);
        }

        public static bool operator ==(BeamResult obj1, BeamResult obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(BeamResult obj1, BeamResult obj2)
        {
            return !(obj1 == obj2);
        }
    }



}
