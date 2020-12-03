using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.LoadCases;
using GPC.Geometry;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    public abstract class Result : ISerializable, IEquatable<ResultPlateForces>
    {
        #region Variables
        protected CoordinateSystem _cSys;
        protected int _elementID;
        protected string _elementLabel;
        protected LoadCase _loadCase;
        #endregion

        #region Properties
        protected Result(int elementID, string elementLabel, LoadCase loadCase, CoordinateSystem cSys)
        {
            _elementID = elementID;
            _elementLabel = elementLabel;
            _loadCase = loadCase;
            _cSys = cSys;
        }

        public virtual bool Equals(ResultPlateForces other)
        {
            return !(other is null) &&
                    _cSys == other._cSys &&
                    _elementID == other._elementID &&
                    _elementLabel == other._elementLabel &&
                    _loadCase == other._loadCase;
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }


        #endregion

    }
}
