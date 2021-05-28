using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Utilities.Extensions;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public sealed class PlateResult : FiniteElementResult, ISerializable, IEquatable<PlateResult>
    {

        /// <param name="Case"></param>
        /// <param name="coordinateSystem"></param>
        /// <param name="result">Lenght of this list should be 3n. Where n is the number of result on each face</param>
        /// <param name="resultLocations">Lenght of this list should be 3n. Where n is the number of result on each face</param>
        /// <remarks>Result order: Lower face (z-), Mid face, Upper face (z+)</remarks>
        public PlateResult(ILoadCase Case, CoordinateSystem coordinateSystem, IPlateResult[] result, ResultLocationId[] resultLocations)
            : this(Case, coordinateSystem, result, resultLocations, ModelObjectId.IDUNASSIGNED)
        {

        }


        /// <param name="Case"></param>
        /// <param name="coordinateSystem"></param>
        /// <param name="result">Lenght of this list should be 3n. Where n is the number of result on each face</param>
        /// <param name="resultLocations">Lenght of this list should be 3n. Where n is the number of result on each face</param>
        /// <param name="stageId"></param>
        /// <remarks>Result order: Lower face (z-), Mid face, Upper face (z+)</remarks>
        public PlateResult(ILoadCase Case, CoordinateSystem coordinateSystem, IPlateResult[] result, ResultLocationId[] resultLocations, int stageId)
            : base(Case, coordinateSystem, (IEnumerable<ResultType>)result, resultLocations, stageId)
        {
            if (result.Count() % 3 != 0)
                throw new ArgumentException("Result lenght should be 3n");

            if (resultLocations.Count() % 3 != 0)
                throw new ArgumentException("Result lenght should be 3n");

        }

        public PlateResult(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }


        public (ResultType[] lowerFace, ResultType[] midFace, ResultType[] upperFace) GetFaceResults()
        {
            List<ResultType[]> splitted = Results.Split(Results.Length / 3);

            return (splitted[0], splitted[1], splitted[2]);
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
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(PlateResult obj1, PlateResult obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
