using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.LoadCases;
using GPC.Geometry;

namespace GPC.Model.Results
{
    public abstract class Result
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
        #endregion

        #region Public Constructors
        #endregion

        #region Public Methods
        #endregion
    }
}
