using System;
using GPC.Geometry;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public abstract class Result : ISerializable, IEquatable<ResultPlateForces>
    {
        #region Variables

        protected CoordinateSystem _cSys;
        protected int _elementID;
        protected string _elementLabel;
        protected int _caseId;

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementID">Id of the element where these result are referred to</param>
        /// <param name="elementLabel">Label of the element where these result are referred t</param>
        /// <param name="caseId">Represenet the id of the loadcase / loadCombination where these result are referred to</param>
        /// <param name="cSys">Coordinate system where these result are provided</param>
        protected Result(int elementID, string elementLabel, int caseId, CoordinateSystem cSys)
        {
            _elementID = elementID;
            _elementLabel = elementLabel;
            _caseId = caseId;
            _cSys = cSys;
        }

        #endregion

        public virtual bool Equals(ResultPlateForces other)
        {
            return !(other is null) &&
                    _cSys == other._cSys &&
                    _elementID == other._elementID &&
                    _elementLabel == other._elementLabel &&
                    _caseId == other._caseId;
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }



    }
}
