using System;
using GPC.Geometry;
using System.Runtime.Serialization;
using System.Collections.Generic;
using GPC.Model.LoadCases;
using GPC.Model.Elements;

namespace GPC.Model.Results
{
    [Serializable]
    public abstract class Result : ModelObject, ISerializable
    {
        #region Variables

        protected CoordinateSystem _coordinateSystem;

        protected Element _element;

        protected ILoadCase _case;

        protected ResultPoint _resultPoint;

        #endregion

        public CoordinateSystem CoordinateSystem => _coordinateSystem;

        public virtual Element Element => _element;

        public ILoadCase Case => _case;

        public virtual ResultPoint ResultPoint => _resultPoint;


        #region Constructors



        /// <summary>
        /// 
        /// </summary>
        /// <param name="element">Element where these result are referred </param>
        /// <param name="Case">The case where these results are reffered </param>
        /// <param name="resultPoint"></param>
        /// <param name="coordinateSystem">The coordianteSystem where these results are referred</param>
        protected Result(Element element, ILoadCase Case, ResultPoint resultPoint, CoordinateSystem coordinateSystem) : base(Guid.NewGuid())
        {
            _element = element;
            _case = Case;
            _resultPoint = resultPoint;
            _coordinateSystem = coordinateSystem;

        }

        protected Result(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            Result other = obj as Result;

            return !(other is null) && _coordinateSystem == other._coordinateSystem &&
                                        _element == other._element &&
                                        _case == other._case &&
                                        _resultPoint == other._resultPoint &&
                                        _coordinateSystem == other._coordinateSystem && base.Equals(other);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<CoordinateSystem>.Default.GetHashCode(_coordinateSystem);
            hashCode = hashCode * -17 + EqualityComparer<Element>.Default.GetHashCode(_element);
            hashCode = hashCode * -17 + EqualityComparer<ILoadCase>.Default.GetHashCode(_case);
            hashCode = hashCode * -17 + EqualityComparer<ResultPoint>.Default.GetHashCode(_resultPoint);
            return hashCode;
        }

        public static bool operator ==(Result obj1, Result obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }


        public static bool operator !=(Result obj1, Result obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
