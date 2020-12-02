using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.LoadCases;
using GPC.Geometry;

namespace GPC.Model.Results
{
    public class ResultsPlateStress : Result
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
        public ResultsPlateStress(int elementID, string elementLabel, LoadCase loadCase, CoordinateSystem cSys, double sxx, double syy, double sxy) :
            base(elementID, elementLabel, loadCase, cSys)
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
            double result = 0;
            return result;
        }

        public double GetVMStress()
        {
            double result = 0;
            return result;
        }

        public double[] GetGlobalStress()
        {
            double[] result = new double[6];
            return result;
        }

        #endregion
    }
}
