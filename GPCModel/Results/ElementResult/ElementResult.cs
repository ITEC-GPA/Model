using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{


    [Serializable]
    public abstract class ElementResult : ModelObject, ISerializable
    {


        protected readonly CoordinateSystem _coordinateSystem;

        protected readonly ILoadCase _case;



        public CoordinateSystem CoordinateSystem => _coordinateSystem;

        public ILoadCase Case => _case;



        /// <param name="Case">The case where these results are reffered </param>
        /// <param name="coordinateSystem">Coordinate system where these result are provided</param>
        public ElementResult(ILoadCase Case, CoordinateSystem coordinateSystem)
            : base()
        {
            _case = Case ?? throw new ArgumentNullException(nameof(Case));
            _coordinateSystem = coordinateSystem ?? throw new ArgumentNullException(nameof(coordinateSystem));
        }


        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            // Coordinate system non messo nell'equals per scelta. Comparazione viene fatta solo su loadcase
            return (obj is ElementResult other) && _case == other._case && base.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // Coordinate system non messo nell'hashcode per scelta. Comparazione viene fatta solo su loadcase
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<ILoadCase>.Default.GetHashCode(_case);
                return hashCode;
            }
        }

        public static bool operator ==(ElementResult obj1, ElementResult obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }


        public static bool operator !=(ElementResult obj1, ElementResult obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
