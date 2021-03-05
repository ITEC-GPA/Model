using System;
using GPC.Geometry;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace GPC.Model.Results
{
    [Serializable]
    public abstract class Result : ModelObject, ISerializable
    {
        #region Variables

        protected CoordinateSystem _coordinateSystem;
        protected int _elementID;
        protected string _elementLabel;
        protected int _caseId;
        protected int _resultPointId;

        #endregion

        public CoordinateSystem CoordinateSystem => _coordinateSystem;
        public int ElementID => _elementID;
        public string ElementLabel => _elementLabel;
        public int CaseId => _caseId;
        public int ResultPointId => _resultPointId;


        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementID">Id of the element where these result are referred to</param>
        /// <param name="elementLabel">Label of the element where these result are referred t</param>
        /// <param name="caseId">Represent the id of the loadcase / loadCombination where these result are referred to</param>
        /// <param name="resultPointId">Represent the id of the point where the resultsa are provided</param>
        /// <param name="coordinateSystem">Coordinate system where these result are provided</param>
        protected Result(int elementID, string elementLabel, int caseId, int resultPointId, CoordinateSystem coordinateSystem) : base(Guid.NewGuid())
        {
            _elementID = elementID;
            _elementLabel = elementLabel;
            _caseId = caseId;
            _coordinateSystem = coordinateSystem;
            _resultPointId = resultPointId;
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
                                            _elementID == other._elementID &&
                                            _elementLabel == other._elementLabel &&
                                            _caseId == other._caseId &&
                                            _resultPointId == other._resultPointId && base.Equals(other);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<CoordinateSystem>.Default.GetHashCode(_coordinateSystem);
            hashCode = hashCode * -17 + _elementID.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<string>.Default.GetHashCode(_elementLabel);
            hashCode = hashCode * -17 + _caseId.GetHashCode();
            hashCode = hashCode * -17 + _resultPointId.GetHashCode();
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
