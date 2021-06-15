using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

namespace GPC.Model.Results
{
    [Serializable]
    public sealed class BeamResult : FiniteElementResult, ISerializable, IEquatable<BeamResult>
    {
        public double Length => ((ResultStation)Points.First()).ElementLenght;     // TODO: va sistemato

        public BeamResult(ILoadCase Case, IEnumerable<IBeamResult> result, IEnumerable<ResultStation> points)
            : this(Case, result, points, ModelObjectId.IDUNASSIGNED)
        {

        }


        public BeamResult(ILoadCase Case, IEnumerable<IBeamResult> result, IEnumerable<ResultStation> points, int stageId)
            : base(Case, null, (IEnumerable<ResultType>)result, points, stageId)
        {
            if (points.Select(i => i.ElementLenght).Distinct().Count() > 1)
                throw new ArgumentException("All Result Station must have the same length");
            // errore

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
            if (other is null)
                return false;

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
