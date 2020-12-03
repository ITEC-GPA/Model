using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.LoadCases;
using GPC.Geometry;

namespace GPC.Model.Results
{
    public class ResultPlateStress : Result
    {
        #region Variables

        /// <summary>
        /// Local Stresses
        /// </summary>
        protected double _sxx;
        protected double _syy;
        protected double _szz;
        protected double _sxy;
        protected double _sxz;
        protected double _syz;
        /// <summary>
        /// Global Stresses
        /// </summary>
        protected double _sXX;
        protected double _sYY;
        protected double _sZZ;
        protected double _sXY;
        protected double _sXZ;
        protected double _sYZ;
        /// <summary>
        /// Principal Stresses
        /// </summary>
        protected double _s11;
        protected double _s22;
        /// <summary>
        /// Combined Stresses
        /// </summary>
        protected double _sVM;
        protected double _sTR;

        #endregion

        #region Properties

        protected double Sxx => _sxx;
        protected double Syy => _syy;
        protected double Szz => _szz;
        protected double Sxy => _sxy;
        protected double Sxz => _sxz;
        protected double Syz => _syz;

        #endregion

        #region Public Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementID">Id of the element where these result are referred to</param>
        /// <param name="elementLabel">Label of the element where these result are referred t</param>
        /// <param name="caseId">Represenet the id of the loadcase / loadCombination where these result are referred to</param>
        /// <param name="cSys">Coordinate system where these result are provided</param>
        /// <param name="cSys"></param>
        /// <param name="sxx"></param>
        /// <param name="syy"></param>
        /// <param name="sxy"></param>
        public ResultPlateStress(int elementID, string elementLabel, int resultPointId, int caseId, CoordinateSystem cSys, double sxx, double syy, double sxy) 
            : base(elementID, elementLabel, caseId, cSys)
        {
            _sxx = sxx;
            _sxx = syy;
            _szz = 0;
            _sxy = sxy;
            _sxz = 0;
            _syz = 0;
        }

        #endregion

        #region Public Methods Specific

        public double GetPrincipalStress()
        {
            throw new NotImplementedException();
        }

        public double GetVMStress()
        {
            throw new NotImplementedException();
        }

        public double[] GetGlobalStress()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
