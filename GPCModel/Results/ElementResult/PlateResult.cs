using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Utilities.Extensions;

namespace GPC.Model.Results
{
    [Serializable]
    public sealed class PlateResult : FiniteElementResult, ISerializable, IEquatable<PlateResult>, IElementResult
    {


        /// <param name="Case"></param>
        /// <param name="coordinateSystem"></param>
        /// <param name="resultLocations"></param>
        /// <param name="stageId"></param>
        /// <param name="name"></param>
        public PlateResult(ILoadCase Case, CoordinateSystem coordinateSystem,
                                           ResultLocation[] resultLocations,
                                           int stageId = ModelObjectId.IDUNASSIGNED, string name = "")
            : base(Case, coordinateSystem, resultLocations, stageId, name)
        {


            if (resultLocations.Where(i => i != null).Select(i => i.GetType()).Distinct().Count() > 1)
                throw new ArgumentException("Multiple location type");


            // controllo che siano iplate result
            if (!(resultLocations.First().GetResults().First() is IPlateResult))
                throw new ArgumentException("Result type is not a IplateResult");


            // controllo che siano tutti lo stesso tipo di result
            if (resultLocations.First().GetResults().Select(i => i.GetType()).Distinct().Count() > 1)
                throw new ArgumentException("Different result types");


        }

        public PlateResult(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
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
            if (other == null)
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
